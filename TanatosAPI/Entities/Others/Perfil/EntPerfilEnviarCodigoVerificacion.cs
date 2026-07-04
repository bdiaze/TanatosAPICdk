using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Perfil {
	[ExcludeFromCodeCoverage]
	public class EntPerfilEnviarCodigoVerificacion {
		public string? Nombre { get; set; }
		public required string CorreoElectronico { get; set; }
		public required string CodigoEncriptado { get; set; }
		public required TipoCodigoVerificacion TipoCodigo { get; set; } = TipoCodigoVerificacion.SignUp;
	}

	public enum TipoCodigoVerificacion {
		SignUp = 1,
		ForgotPassword = 2,
		ResendCode = 3,
		UpdateUserAttribute = 4,
		VerifyUserAttribute = 5,
		Authentication = 6,
		AdminCreateUser = 7,
		AccountTakeOverNotification = 8
	}
}
