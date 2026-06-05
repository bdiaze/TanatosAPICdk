using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntEmpleadoActualizar {
		public required long Id { get; set; }
		public required string Nombre { get; set; }
		public required long IdCargo { get; set; }
		public List<EntEmpleadoActualizarDestinatario> Destinatarios { get; set; } = [];
	}

    [ExcludeFromCodeCoverage]
    public class EntEmpleadoActualizarDestinatario {
		public required long IdTipoReceptor { get; set; }
		public required string Destino { get; set; }
	}
}
