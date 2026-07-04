namespace TanatosAPI.Interfaces.Helpers {
    public interface IDateTimeProvider {
        public DateTime UtcNow { get; }
    }
}
