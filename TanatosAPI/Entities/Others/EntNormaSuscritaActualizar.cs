using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntNormaSuscritaActualizar {
		public required long Id { get; set; }
		public required long IdNegocio { get; set; }
		public required string Nombre { get; set; }
		public string? Descripcion { get; set; }
		public long? IdTipoPeriodicidad { get; set; }
		public string? Multa { get; set; }
		public long? IdCategoriaNorma { get; set; }
        public long? IdCargo { get; set; }
        public required bool Activado { get; set; }
		public List<EntFiscalizadorNormaSuscritaActualizar>? Fiscalizadores { get; set; }
		public List<EntNotificacionNormaSuscritaActualizar>? Notificaciones { get; set; }
		public DateTime? ProximoVencimiento { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class EntFiscalizadorNormaSuscritaActualizar {
		public required long IdTipoFiscalizador { get; set; }
	}

    [ExcludeFromCodeCoverage]
    public class EntNotificacionNormaSuscritaActualizar {
		public required long IdTipoUnidadTiempoAntelacion { get; set; }
		public required int CantAntelacion { get; set; }
	}
}
