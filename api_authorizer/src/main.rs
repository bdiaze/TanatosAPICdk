use lambda_runtime::{service_fn, LambdaEvent, Error};
use serde::{Deserialize, Serialize};
use jsonwebtoken::{Algorithm, DecodingKey, Header, TokenData, Validation, decode, decode_header, errors::ErrorKind};
use reqwest::{Client};
use tracing::{info, error};
use std::{collections::HashMap, time::SystemTime};
use std::env;
use lazy_static::lazy_static;
use tokio::sync::{RwLock, RwLockWriteGuard};
use std::time::{Duration, Instant, UNIX_EPOCH};
use dotenvy::dotenv;
use tracing_subscriber::fmt::time::ChronoUtc;

// JWT - JWKS
#[derive(Debug, Serialize, Deserialize)]
struct Claims {
    sub: String,
    iss: String,
    aud: Option<String>,
    client_id: String,
    iat: Option<usize>,
    exp: usize,
    nbf: Option<usize>,
}

#[derive(Debug, Deserialize, Clone)]
struct Jwks {
    keys: Vec<Jwk>,
}

#[derive(Debug, Deserialize, Clone)]
struct Jwk {
    kid: String,
    n: String,
    e: String,
}

// Lambda Authorizer
#[derive(Deserialize)]
struct AuthorizerRequest {
    #[serde(default)]
    headers: HashMap<String, String>,
}

#[derive(Debug, Serialize)]
struct AuthorizerResponse {
    #[serde(rename = "isAuthorized")]
    is_authorized: bool,
    context: HashMap<String, String>
}

lazy_static! {
    static ref JWKS_CACHE: RwLock<Option<(Jwks, Instant)>> = RwLock::new(None);
}

static INIT: std::sync::Once = std::sync::Once::new();

// Main
#[tokio::main]
async fn main() -> Result<(), Error> {
    tracing_subscriber::fmt()
        .with_timer(ChronoUtc::default())
        .with_ansi(false) 
        .with_max_level(tracing::Level::INFO)
        .init();

    INIT.call_once(|| {
        info!("Inicializando Authorizer...");
    });

    dotenv().ok();  

    lambda_runtime::run(service_fn(authorizer_handler)).await?;
    Ok(())
}

async fn authorizer_handler(event: LambdaEvent<AuthorizerRequest>) -> Result<AuthorizerResponse, Error> {
    info!("Comenzando con autorizacion de evento...");

    // Se obtiene el access token...
    let token = match extract_token(&event.payload.headers) {
        Some(t) => t,
        None => {
            error!("No se encontro el access token");
            return Ok(deny("No se encontro el access token"));
        }
    };

    info!("Se extrae correctamente el token desde el evento...");

    // Se obtiene configuración de cognito desde variables de entorno...
    let region: String = env::var("COGNITO_REGION")?;
    let pool_id: String = env::var("COGNITO_USER_POOL_ID")?;

    // Se obtiene el JWKS...
    let jwks: Jwks = match get_jwks(&region, &pool_id).await {
        Ok(j) => j,
        Err(err) => {
            error!("Error obteniendo JWKS: {}", err);
            return Ok(deny("No se pudo obtener JWKS"));
        }
    };

    info!("Se obtiene correctamente JWKS...");

    // Se valida token JWT
    match validate_jwt(token, &jwks, &region, &pool_id) {
        Ok(_) => {
            info!("El token es valido...");
            Ok(allow())
        },
        Err(reason) => {
            error!("El token no es valido - Reason: {}", reason);
            Ok(deny(reason))
        }
    }
}

fn extract_token(headers: &HashMap<String, String>) -> Option<&str> {
    // Se trata de extraer access token desde Authorization header...
    if let Some(auth) = headers.get("authorization") {
        if let Some(token) = auth.strip_prefix("Bearer ") {
            return Some(token);
        }
    }

    // Si no se encuentra, se trata de extraer access token desde cookie...
    if let Some(cookie_header) = headers.get("cookie") {
        for cookie in cookie_header.split(";") {
            let parts: Vec<&str> = cookie.trim().split("=").collect();
            if parts.len() == 2 && parts[0] == "access_token" {
                return Some(parts[1]);
            }
        }
    }
    None
}

async fn get_jwks(region: &str, pool_id: &str) -> Result<Jwks, Error> {
    let mut cache: RwLockWriteGuard<'_, Option<(Jwks, Instant)>> = JWKS_CACHE.write().await;
    let now: Instant = Instant::now();
    let cache_valid: bool = cache.as_ref().map(|(_, t): &(Jwks, Instant)| now.duration_since(*t) < Duration::from_secs(3600)).unwrap_or(false);

    if cache_valid {
        Ok(cache.as_ref().unwrap().0.clone())
    } else {
        let url: String = format!("https://cognito-idp.{}.amazonaws.com/{}/.well-known/jwks.json", region, pool_id);
        let client: Client = Client::new();
        let jwks: Jwks = client.get(&url).send().await?.json().await?;
        *cache = Some((jwks.clone(), Instant::now()));
        Ok(jwks)
    }
}

fn validate_jwt(token: &str, jwks: &Jwks, region: &str, pool_id: &str) -> Result<Claims, &'static str> {
    let header: Header = decode_header(token).map_err(|_| "Header de token inválido")?;
    let kid: String = header.kid.ok_or("No existe atributo kid en header de token")?;
    let jwk: &Jwk = jwks.keys.iter().find(|k: &&Jwk| k.kid == kid).ok_or("No existe JWK para kid del header token")?;

    let decoding_key: DecodingKey = DecodingKey::from_rsa_components(&jwk.n, &jwk.e).map_err(|_| "Key inválida")?;
    let mut validation: Validation = Validation::new(Algorithm::RS256);
    validation.set_issuer(&[format!("https://cognito-idp.{}.amazonaws.com/{}", region, pool_id)]);


    let token_data: TokenData<Claims> = decode::<Claims>(token, &decoding_key, &validation).map_err(|e| match e.kind() {
        ErrorKind::ExpiredSignature => "Token expirado",
        ErrorKind::ImmatureSignature => "Token aún no válido",
        _ => "JWT inválido"
    })?;

    // Validación manual de nbf e iat...
    let now: usize = SystemTime::now().duration_since(UNIX_EPOCH).unwrap().as_secs() as usize;
    if let Some(nbf) = token_data.claims.nbf {
        if now < nbf {
            return Err("Token aún no válido (nbf)");
        }
    }
    if let Some(iat) = token_data.claims.iat {
        if now + 60 < iat {
            return Err("Token otorgado con fecha futura");
        }
    }

    Ok(token_data.claims)
}

fn allow() -> AuthorizerResponse {
    AuthorizerResponse {
        is_authorized: true,
        context: HashMap::new()
    }
}

fn deny(reason: &str) -> AuthorizerResponse {
    let mut context: HashMap<String, String> = HashMap::new();
    context.insert("reason".to_string(), reason.to_string());

    AuthorizerResponse {
        is_authorized: false,
        context,
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use lambda_runtime::Context;
    use std::collections::HashMap;
    use dotenvy::dotenv;
    use std::env;

    #[tokio::test]
    async fn test_authorizer_header() {
        dotenv().ok();

        let mut headers: HashMap<String, String> = HashMap::new();
        headers.insert("authorization".to_string(), format!("Bearer {}", env::var("TEST_TOKEN").expect("No se ha seteado un TEST_TOKEN")));

        let event: AuthorizerRequest = AuthorizerRequest { headers };

        let context: Context = Context::default();
        let lambda_event: LambdaEvent<AuthorizerRequest> = lambda_runtime::LambdaEvent::new(event, context);

        let resp: AuthorizerResponse = authorizer_handler(lambda_event).await.unwrap();
        println!("Response: {:#?}", resp);
    }

    #[tokio::test]
    async fn test_authorizer_cookie() {
        dotenv().ok();

        let mut headers: HashMap<String, String> = HashMap::new();
        headers.insert("cookie".to_string(), format!("access_token={}; Path=/; HttpOnly", env::var("TEST_TOKEN").expect("No se ha seteado un TEST_TOKEN")));

        let event: AuthorizerRequest = AuthorizerRequest { headers };

        let context: Context = Context::default();
        let lambda_event: LambdaEvent<AuthorizerRequest> = lambda_runtime::LambdaEvent::new(event, context);

        let resp: AuthorizerResponse = authorizer_handler(lambda_event).await.unwrap();
        println!("Response: {:#?}", resp);
    }
}