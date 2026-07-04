using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Test.Helpers {
	public class DateTimeHelperTest {
		public static TheoryData<DateTime, DateTime> FechasUTC => new() {
			{ new DateTime(2026, 4, 4, 23, 59, 0, DateTimeKind.Utc),  new DateTime(2026, 4, 4, 20, 59, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 4, 5, 23, 59, 0, DateTimeKind.Utc),  new DateTime(2026, 4, 5, 19, 59, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 4, 5, 00, 01, 0, DateTimeKind.Utc),  new DateTime(2026, 4, 4, 21, 01, 0, DateTimeKind.Unspecified) },
			{ new DateTime(2026, 4, 6, 00, 01, 0, DateTimeKind.Utc),  new DateTime(2026, 4, 5, 20, 01, 0, DateTimeKind.Unspecified) },
		};
		[Theory]
		[MemberData(nameof(FechasUTC))]
		public async Task TransformarFechaUTCATimezoneTest(DateTime fechaUtc, DateTime expectedLocal) {
			DateTime retorno = DateTimeHelper.TransformarFechaUTCATimezone(fechaUtc);
			Assert.Equal(expectedLocal, retorno);
		}

		public static TheoryData<DateTime, DateTime> FechasLocales => new() {
			{ new DateTime(2026, 4, 4, 20, 59, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 4, 23, 59, 0, DateTimeKind.Utc) },
			{ new DateTime(2026, 4, 5, 19, 59, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 23, 59, 0, DateTimeKind.Utc) },
			{ new DateTime(2026, 4, 4, 21, 01, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 5, 00, 01, 0, DateTimeKind.Utc) },
			{ new DateTime(2026, 4, 5, 20, 01, 0, DateTimeKind.Unspecified), new DateTime(2026, 4, 6, 00, 01, 0, DateTimeKind.Utc) },
		};
		[Theory]
		[MemberData(nameof(FechasLocales))]
		public async Task TransformarFechaTimezoneAUTCTest(DateTime fechaLocal, DateTime expectedUtc) {
			DateTime retorno = DateTimeHelper.TransformarFechaTimezoneAUTC(fechaLocal);
			Assert.Equal(expectedUtc, retorno);
		}

		public static TheoryData<DateTime, int, DateTime> SumasMeses => new() {
			// Local: 01-04-2026 12:00 (UTC 15:00) > 01-05-2026 12:00 (UTC 16:00)
			{ new DateTime(2026, 4, 1, 15, 00, 0, DateTimeKind.Utc), 1, new DateTime(2026, 5, 1, 16, 00, 0, DateTimeKind.Utc) },
			// Local: 01-05-2026 12:00 (UTC 16:00) > 01-06-2026 12:00 (UTC 16:00)
			{ new DateTime(2026, 5, 1, 16, 00, 0, DateTimeKind.Utc), 1, new DateTime(2026, 6, 1, 16, 00, 0, DateTimeKind.Utc) },
		};
		[Theory]
		[MemberData(nameof(SumasMeses))]
		public async Task SumarMesesTest(DateTime fechaReferenciaUtc, int cantMeses, DateTime expectedUtc) {
			DateTime retorno = DateTimeHelper.SumarMeses(fechaReferenciaUtc, cantMeses);
			Assert.Equal(expectedUtc, retorno);
		}

		[Fact]
		public async Task SumarMesesTest_NoUTC() {
			Assert.Throws<InvalidOperationException>(() => DateTimeHelper.SumarMeses(new DateTime(2026, 5, 1, 16, 00, 0, DateTimeKind.Unspecified), 1));
		}
	}
}
