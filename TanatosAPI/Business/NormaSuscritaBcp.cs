using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Scriban.Runtime;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(IVariableEntornoHelper variableEntorno, IDateTimeProvider dateTimeProvider, INormaSuscritaDao normaSuscritaDao, IHermesHelper hermesHelper, IHtmlRenderer renderer) : INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita) {
			return normaSuscrita != null && normaSuscrita.Vigencia;
		}

		public bool Pertenece(NormaSuscrita normaSuscrita, string sub) {
			return normaSuscrita.Sub == sub;
		}

		public bool PerteneceNegocio(NormaSuscrita normaSuscrita, long idNegocio) {
			return normaSuscrita.IdNegocio == idNegocio;
		}

		public bool EstaActiva(NormaSuscrita normaSuscrita) {
			return EstaVigente(normaSuscrita) && normaSuscrita.Activado;
		}

        public bool EsEditable(NormaSuscrita normaSuscrita) {
            return normaSuscrita.Editable;
        }

		public List<NormaSuscrita> FiltrarVigentes(List<NormaSuscrita> normasSuscritas) {
			return [.. normasSuscritas.Where(ns => EstaVigente(ns))];
		}

		public async Task<NormaSuscrita?> Obtener(long idNormaSuscrita, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, bool validarEditable = false, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(normaSuscrita)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La obligación no existe o no está vigente", "La obligación es inválida.");
			if (normaSuscrita != null) {
				if (validarSub != null && !Pertenece(normaSuscrita, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "La obligación es inválida.");
				if (validarIdNegocio != null && !PerteneceNegocio(normaSuscrita, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al negocio", "La obligación es inválida.");
				if (validarEditable && !EsEditable(normaSuscrita)) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no es editable por el usuario", "La obligación es inválida.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(normaSuscrita)) return null;

			return normaSuscrita;
		}

		public async Task<List<NormaSuscrita>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(sub, idNegocio, null, transaction);
			if (filtrarVigentes) normasSuscritas = FiltrarVigentes(normasSuscritas);
			return normasSuscritas;
		}
				
		public async Task<NormaSuscrita> CrearObligacionUsuario(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, NpgsqlTransaction? transaction = null) {
            nombre = nombre.Trim();
            descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
            multa = string.IsNullOrWhiteSpace(multa) ? null : multa.Trim();

			// Se valida que no exista otra obligación con el mismo nombre...
			List<NormaSuscrita> vigentes = await ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: true);
			if (vigentes.Any(o => o.Nombre == nombre)) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe una obligación con dicho nombre."); 

            DateTime now = dateTimeProvider.UtcNow;
            NormaSuscrita nuevo = new() {
                Id = 0,
                Sub = sub,
                IdNegocio = idNegocio,
                IdTemplate = null,
                IdNorma = null,
                Nombre = nombre,
                Descripcion = descripcion,
                Multa = multa,
                IdTipoPeriodicidad = idTipoPeriodicidad,
                IdCategoriaNorma = idCategoriaNorma,
                IdCargo = idCargo,
                OrdenVisual = null,
                Editable = true,
                FechaActivacion = activado ? now : null,
                FechaDesactivacion = null,
                Activado = activado,
                FechaCreacion = now,
                FechaEliminacion = null,
                Vigencia = true
            };
            nuevo.Id = await normaSuscritaDao.Insertar(nuevo, transaction);
			return nuevo;
        }

		public async Task Actualizar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
		}

		public async Task Activar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (!normaSuscrita.Activado) {
				normaSuscrita.FechaActivacion = dateTimeProvider.UtcNow;
				normaSuscrita.FechaDesactivacion = null;
				normaSuscrita.Activado = true;

				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
			}
		}

		public async Task Desactivar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Activado) {
                normaSuscrita.FechaDesactivacion = dateTimeProvider.UtcNow;
                normaSuscrita.Activado = false;

				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
            }
		}

		public async Task Eliminar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Vigencia) {
				await Desactivar(normaSuscrita, transaction);

                normaSuscrita.FechaEliminacion = dateTimeProvider.UtcNow;
                normaSuscrita.Vigencia = false;

                await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
            }
		}

		public async Task<string> EnviarCorreoNuevaObligacion(string? codigoAcceso, string correoElectronico, string? nombreEmpleado, long? idNormaSuscrita, string nombreObligacion, string? multa, DateTime proximoVencimientoChile, TipoPeriodicidad periodicidad) {
			SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
				De = new DireccionCorreo() {
					Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
					Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
				},
				Para = [
					new DireccionCorreo() {
						Correo = correoElectronico
					}
				],
				Asunto = $"Tienes una nueva obligación asignada - Todo en Orden",
				Cuerpo = await renderer.GenerarHtml("NuevaObligacion.html", new ScriptObject() {
					["NOMBRE_EMPLEADO"] = nombreEmpleado != null ? WebUtility.HtmlEncode(nombreEmpleado) : null,
					["NOMBRE_OBLIGACION"] = WebUtility.HtmlEncode(nombreObligacion),
					["PROXIMO_VENCIMIENTO"] = WebUtility.HtmlEncode(proximoVencimientoChile.ToString("dd/MM/yyyy HH:mm")),
					["PERIODICIDAD"] = WebUtility.HtmlEncode(periodicidad.Nombre),
					["MULTA"] = multa != null ? WebUtility.HtmlEncode(multa) : null,
					["ID_NORMA_SUSCRITA"] = idNormaSuscrita,
					["CODIGO_ACCESO"] = codigoAcceso != null ? Uri.EscapeDataString(codigoAcceso) : null
				})
			});

			return retorno.IdMensaje;
		}

		public async Task<string> EnviarWhatsappNuevaObligacion(string? codigoAcceso, string numeroWhatsapp, string? nombreEmpleado, long? idNormaSuscrita, string nombreObligacion, string? multa, DateTime proximoVencimientoChile, TipoPeriodicidad periodicidad) {
			SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
				De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
				Para = numeroWhatsapp,
				NombreTemplate = "nueva_obligacion",
				ParametrosCuerpo = [
					nombreEmpleado ?? "",
					nombreObligacion,
					proximoVencimientoChile.ToString("dd/MM/yyyy HH:mm"),
					periodicidad.Nombre,
					multa ?? "Sin multa"
				],
				ParametrosBoton = [
					$"{idNormaSuscrita}?codigo={Uri.EscapeDataString(codigoAcceso ?? "")}"
				]
			});

			return retorno.IdMensaje;
		}

		public async Task<string> EnviarCorreoObligacionModificada(string? codigoAcceso, string correoElectronico, string? nombreEmpleado, long? idNormaSuscrita, string nombreObligacion, string? multa, DateTime proximoVencimientoChile, TipoPeriodicidad periodicidad) {
			SalHermesEnviar retorno = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
				De = new DireccionCorreo() {
					Nombre = variableEntorno.Obtener("HERMES_DE_NOMBRE"),
					Correo = variableEntorno.Obtener("HERMES_DE_CORREO"),
				},
				Para = [
					new DireccionCorreo() {
						Correo = correoElectronico
					}
				],
				Asunto = $"Una obligación ha sufrido cambios - Todo en Orden",
				Cuerpo = await renderer.GenerarHtml("ObligacionModificada.html", new ScriptObject() {
					["NOMBRE_EMPLEADO"] = nombreEmpleado != null ? WebUtility.HtmlEncode(nombreEmpleado) : null,
					["NOMBRE_OBLIGACION"] = WebUtility.HtmlEncode(nombreObligacion),
					["PROXIMO_VENCIMIENTO"] = WebUtility.HtmlEncode(proximoVencimientoChile.ToString("dd/MM/yyyy HH:mm")),
					["PERIODICIDAD"] = WebUtility.HtmlEncode(periodicidad.Nombre),
					["MULTA"] = multa != null ? WebUtility.HtmlEncode(multa) : null,
					["ID_NORMA_SUSCRITA"] = idNormaSuscrita,
					["CODIGO_ACCESO"] = codigoAcceso != null ? Uri.EscapeDataString(codigoAcceso) : null
				})
			});

			return retorno.IdMensaje;
		}

		public async Task<string> EnviarWhatsappObligacionModificada(string? codigoAcceso, string numeroWhatsapp, string? nombreEmpleado, long? idNormaSuscrita, string nombreObligacion, string? multa, DateTime proximoVencimientoChile, TipoPeriodicidad periodicidad) {
			SalHermesEnviar retorno = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
				De = variableEntorno.Obtener("HERMES_DE_WHATSAPP"),
				Para = numeroWhatsapp,
				NombreTemplate = "obligacion_modificada",
				ParametrosCuerpo = [
					nombreEmpleado ?? "",
					nombreObligacion,
					proximoVencimientoChile.ToString("dd/MM/yyyy HH:mm"),
					periodicidad.Nombre,
					multa ?? "Sin multa"
				],
				ParametrosBoton = [
					$"{idNormaSuscrita}?codigo={Uri.EscapeDataString(codigoAcceso ?? "")}"
				]
			});

			return retorno.IdMensaje;
		}
	}
}
