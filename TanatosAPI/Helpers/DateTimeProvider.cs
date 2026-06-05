using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class DateTimeProvider : IDateTimeProvider {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
