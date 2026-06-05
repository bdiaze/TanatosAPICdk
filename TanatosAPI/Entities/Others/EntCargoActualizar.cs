using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class EntCargoActualizar {
        public required long Id { get; set; }
        public required string Nombre { get; set; }
    }
}
