namespace TanatosAPI.Exceptions {
    public class ErrorValidacion(string mensaje, string? mensajeGenerico = null) : Exception(mensaje) {
        public string MensajeGenerico => mensajeGenerico ?? base.Message;

        public override string ToString() => mensajeGenerico != null ? $"{mensajeGenerico} - {base.ToString()}" : base.ToString();
    }
}
