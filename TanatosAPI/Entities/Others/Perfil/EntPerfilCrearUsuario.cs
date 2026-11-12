using Amazon.S3.Model;

namespace TanatosAPI.Entities.Others.Perfil {
	public class EntPerfilCrearUsuario {
		public required string Email { get; set; }
		public required string Nombre { get; set; }
		public required string Apellido { get; set; }
	}
}
