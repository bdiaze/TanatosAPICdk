using Microsoft.IdentityModel.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Helpers {
	public class CronHelperTest {
		public static TheoryData<DateTime, string?, string> FechasCron => new() {
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), null, "30 12 15 6 ? 2026" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM MO ? YE", "30 12 15 6 ? 2026" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO * * ? *", "30 12 * * ? *" },
			{ new DateTime(2026, 6, 14, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * SUN *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * MON *" },
			{ new DateTime(2026, 6, 16, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * TUE *" },
			{ new DateTime(2026, 6, 17, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * WED *" },
			{ new DateTime(2026, 6, 18, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * THU *" },
			{ new DateTime(2026, 6, 19, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * FRI *" },
			{ new DateTime(2026, 6, 20, 12, 30, 0, DateTimeKind.Unspecified), "MI HO ? * DW *", "30 12 ? * SAT *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM * ? *", "30 12 15 * ? *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM 1,3,5,7,9,11|2,4,6,8,10,12 ? *", "30 12 15 2,4,6,8,10,12 ? *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM 1,4,7,10|2,5,8,11|3,6,9,12 ? *", "30 12 15 3,6,9,12 ? *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM 1,7|2,8|3,9|4,10|5,11|6,12 ? *", "30 12 15 6,12 ? *" },
			{ new DateTime(2026, 6, 15, 12, 30, 0, DateTimeKind.Unspecified), "MI HO DM MO ? *", "30 12 15 6 ? *" },
		};
		[Theory]
		[MemberData(nameof(FechasCron))]
		public async Task GenerarCronAWSDesdeFechaTest(DateTime fecha, string? baseCronAws, string expectedCron) {
			string retorno = CronHelper.GenerarCronAWSDesdeFecha(fecha, baseCronAws);
			Assert.Equal(expectedCron, retorno);
		}

		[Fact]
		public async Task GenerarCronAWSDesdeFechaTest_Invalido() {
			Assert.Throws<ArgumentException>(() => CronHelper.GenerarCronAWSDesdeFecha(DateTime.UtcNow, "MI HO DM MO ? YE X"));
		}
		
		[Theory]
		[InlineData("0 12 23 4 ? *", "0 12 23 4 *")]
		[InlineData("0 12 9 * ? *", "0 12 9 * *")]
		[InlineData("0 15 31 1,7 ? *", "0 15 31 1,7 *")]
		[InlineData("0 14 12 6 ? 2026", "0 14 12 6 *")]
		public async Task TransformarCronAWSAStandardTest(string cronAws, string expectedCronStandard) {
			string retorno = CronHelper.TransformarCronAWSAStandard(cronAws);
			Assert.Equal(expectedCronStandard, retorno);
		}

	}
}
