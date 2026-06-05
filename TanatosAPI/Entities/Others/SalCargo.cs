using System.Diagnostics.CodeAnalysis;

namespace TanatosAPI.Entities.Others {
    [ExcludeFromCodeCoverage]
    public class SalCargo {
        public required long Id { get; set; }
        public required string Nombre { get; set; }
    }
}
