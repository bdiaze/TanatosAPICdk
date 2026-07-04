using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others.Cargo {
    [ExcludeFromCodeCoverage]
    public class EntCargoCrear {
        public required long IdNegocio { get; set; }
        public required string Nombre { get; set; }
    }
}
