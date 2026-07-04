using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Helpers {
	[ExcludeFromCodeCoverage]
	public class DateTimeProvider : IDateTimeProvider {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
