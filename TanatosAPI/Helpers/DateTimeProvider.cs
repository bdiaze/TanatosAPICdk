using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class DateTimeProvider : IDateTimeProvider {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
