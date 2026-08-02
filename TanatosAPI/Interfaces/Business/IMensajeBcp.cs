using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
    public interface IMensajeBcp {
        public Task<Mensaje> Ingresar(string nombre, string correo, string contenido, string? sub = null);
    }
}
