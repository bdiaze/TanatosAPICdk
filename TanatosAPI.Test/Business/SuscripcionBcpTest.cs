using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using TanatosAPI.UseCases;

namespace TanatosAPI.Test.Business {
	public class SuscripcionBcpTest {
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
		private readonly ISuscripcionDao suscripcionDao = Substitute.For<ISuscripcionDao>();
		private readonly IFlowHelper flowHelper = Substitute.For<IFlowHelper>();
		private readonly SuscripcionBcp suscripcionBcp;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);

		public SuscripcionBcpTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);

			suscripcionBcp = new(dateTimeProvider, suscripcionDao, flowHelper);
		}

		public static Suscripcion SuscripcionDummy(
			long id = 1,
			string sub = "sub-test",
			long idPlan = 10,
			DateTime? fechaInicio = null,
			DateTime? fechaExpiracion = null,
			DateTime? fechaProximoCobro = null,
			DateTime? fechaCancelacion = null,
			short estado = 1,
			string? flowCustomerId = "flow-customer-id-test",
			string? flowSubscriptionId = "flow-subscription-id-test",
			DateTime? fechaCreacion = null,
			DateTime? fechaEliminacion = null,
			bool vigencia = true
		) => new() {
			Id = id,
			Sub = sub,
			IdPlan = idPlan,
			FechaInicio = fechaInicio,
			FechaExpiracion = fechaExpiracion,
			FechaProximoCobro = fechaProximoCobro,
			FechaCancelacion = fechaCancelacion,
			Estado = estado,
			FlowCustomerId = flowCustomerId,
			FlowSubscriptionId = flowSubscriptionId,
			FechaCreacion = fechaCreacion ?? DateTime.UtcNow,
			FechaEliminacion = fechaEliminacion,
			Vigencia = vigencia
		};

		#region Casos
		#region Sin Suscripciones
		// Usuario sin ningún tipo de suscripción...
		public static readonly List<Suscripcion> CASO_01_USUARIO_SIN_SUSCRIPCIONES = [];
		#endregion
		#region Solo suscrición gratuita
		// Usuario con suscripción gratuida activa...
		public static readonly List<Suscripcion> CASO_02_USUARIO_GRATUITA_ACTIVA = [
			SuscripcionDummy(idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15))
		];
		// Usuario con suscripción gratuita expirada (estado activa pero fecha expiración pasada)...
		public static readonly List<Suscripcion> CASO_03_USUARIO_GRATUITA_EXPIRADA = [
			SuscripcionDummy(idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15))
		];
		#endregion
		#region Con alguna suscripción de pago (sin gratuita)
		// Usuario sin suscripción gratuita, pero con suscripción de pago activa...
		public static readonly List<Suscripcion> CASO_04_USUARIO_PAGO_ACTIVO = [
			SuscripcionDummy(idPlan: 2, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Usuario sin suscripción gratuita, pero con suscripción de pago expirada (nuevo pago no procesado)...
		public static readonly List<Suscripcion> CASO_05_USUARIO_PAGO_EXPIRADA = [
			SuscripcionDummy(idPlan: 2, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15))
		];
		// Usuario sin suscripción gratuita, pero con suscripción de pago cancelada pero aún activa...
		public static readonly List<Suscripcion> CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA = [
			SuscripcionDummy(idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Usuario sin suscripción gratuita, pero con suscripción de pago cancelada ya expirada...
		public static readonly List<Suscripcion> CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA = [
			SuscripcionDummy(idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15))
		];
		// Usuario con suscripción cancelada expirada, con una suscripción de pago activa...	
		public static readonly List<Suscripcion> CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO = [
			SuscripcionDummy(id: 1, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 2, idPlan: 2, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		#endregion
		#region Con alguna suscripción de pago (y con gratuita previa)
		// Usuario con suscripción gratuita anterior, y con suscripción de pago activa...
		public static readonly List<Suscripcion> CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 2, idPlan: 2, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Usuario con suscripción gratuita anterior, y con suscripción de pago expirada (nuevo pago no procesado)...
		public static readonly List<Suscripcion> CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-75), fechaExpiracion: FECHA_DUMMY.AddDays(-45)),
			SuscripcionDummy(id: 2, idPlan: 2, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15))
		];
		// Usuario con suscripción gratuita anterior, y con suscripción de pago cancelada pero aún activa...
		public static readonly List<Suscripcion> CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Usuario sin suscripción gratuita anterior, pero con suscripción de pago cancelada ya expirada...
		public static readonly List<Suscripcion> CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-75), fechaExpiracion: FECHA_DUMMY.AddDays(-45)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15))
		];
		#endregion
		#region Con suscripción de pago pendiente de primer pago
		// Sin suscripción previa (aún a la espera de confirmación del pago)
		public static readonly List<Suscripcion> CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS = [
			SuscripcionDummy(idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY)
		];
		// Con suscripción gratuita previa, (aún a la espera de confirmación del pago)
		public static readonly List<Suscripcion> CASO_13_USUARIO_GRAT_ANT_PAGO_PEND = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-75), fechaExpiracion: FECHA_DUMMY.AddDays(-45)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY)
		];
		// Con suscripción gratuita activa, (y pago pendiente de suscripción futura)
		public static readonly List<Suscripcion> CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Con suscripción de pago cancelada activa (y pago pendiente de suscripción futura)
		public static readonly List<Suscripcion> CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT = [
			SuscripcionDummy(id: 1, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Con suscripción de pago cancelada expirada (aún a la espera de confirmación del pago)
		public static readonly List<Suscripcion> CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT = [
			SuscripcionDummy(id: 1, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY)
		];
		// Con suscripción gratuita previa, de pago cancelada expirada, pago cancelada activa y pago futura pendiente
		public static readonly List<Suscripcion> CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-75), fechaExpiracion: FECHA_DUMMY.AddDays(-45)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 3, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro:  FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 4, idPlan: 2, estado: 4 /* Pago Pendiente */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		// Con suscripción gratuita previa, de pago cancelada expirada, pago cancelada activa y pago futura cancelada
		public static readonly List<Suscripcion> CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-75), fechaExpiracion: FECHA_DUMMY.AddDays(-45)),
			SuscripcionDummy(id: 2, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-45), fechaExpiracion: FECHA_DUMMY.AddDays(-15), fechaProximoCobro: FECHA_DUMMY.AddDays(-15)),
			SuscripcionDummy(id: 3, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 4, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: null, fechaExpiracion: null, fechaProximoCobro: FECHA_DUMMY.AddDays(15))
		];
		#endregion
		#region Con suscripción gratuita posterior
		// Con suscripción gratuita activa
		public static readonly List<Suscripcion> CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT = [
			SuscripcionDummy(id: 1, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 2, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(15), fechaExpiracion: FECHA_DUMMY.AddDays(45))
		];
		// Con suscripción de pago cancelada activa
		public static readonly List<Suscripcion> CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT = [
			SuscripcionDummy(id: 1, idPlan: 2, estado: 2 /* Cancelada */, fechaInicio: FECHA_DUMMY.AddDays(-15), fechaExpiracion: FECHA_DUMMY.AddDays(15), fechaProximoCobro: FECHA_DUMMY.AddDays(15)),
			SuscripcionDummy(id: 2, idPlan: 1, flowCustomerId: null, flowSubscriptionId: null, fechaInicio: FECHA_DUMMY.AddDays(15), fechaExpiracion: FECHA_DUMMY.AddDays(45))
		];
		#endregion
		#endregion

		public static TheoryData<Suscripcion?, bool> EstaVigenteCases => new() {
			{ SuscripcionDummy(vigencia: true), true },
			{ SuscripcionDummy(vigencia: false), false },
			{ null, false },
		};
		[Theory]
		[MemberData(nameof(EstaVigenteCases))]
		public void EstaVigenteTest(Suscripcion? suscripcion, bool expectedResult) {
			Assert.Equal(expectedResult, suscripcionBcp.EstaVigente(suscripcion));
		}

		public static TheoryData<(Suscripcion suscripcion, string sub), bool> Pertenece => new() {
			{ (SuscripcionDummy(sub: "sub-correcto"), "sub-correcto"), true },
			{ (SuscripcionDummy(sub: "sub-incorrecto"), "sub-correcto"), false },
		};
		[Theory]
		[MemberData(nameof(Pertenece))]
		public void PerteneceAlUsuarioTest((Suscripcion suscripcion, string sub) entrada, bool expectedResult) {
			Assert.Equal(expectedResult, suscripcionBcp.PerteneceAlUsuario(entrada.suscripcion, entrada.sub));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> Expiradas => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { 1, 2 } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { 1 } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { 1, 2 } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { 1 } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { 1 } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 1, 2 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 1, 2 } }
		};
		[Theory]
		[MemberData(nameof(Expiradas))]
		public void FiltrarExpiradasTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarExpiradas(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarExpiradasTest_SinDummy() {
			HashSet<long> expectedIds = [1, 2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarExpiradas(CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> ExpiradasConFlow => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { 2 } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { 2 } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { 1 } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 2 } }
		};
		[Theory]
		[MemberData(nameof(ExpiradasConFlow))]
		public void FiltrarExpiradasConFlowTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarExpiradasConFlow(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarExpiradasConFlowTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarExpiradasConFlow(CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> EnCurso => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { 1 } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { 1 } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { 2 } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { 1 } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 1 } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { 1 } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { 1 } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 3 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 3 } }
		};
		[Theory]
		[MemberData(nameof(EnCurso))]
		public void FiltrarEnCursoTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarEnCurso(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarEnCursoTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarEnCurso(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> EnCursoConFlow => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { 1 } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { 2 } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 1 } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { 1 } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 3 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 3 } }
		};
		[Theory]
		[MemberData(nameof(EnCursoConFlow))]
		public void FiltrarEnCursoConFlowTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarEnCursoConFlow(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarEnCursoConFlowTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarEnCursoConFlow(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> Futuras => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { 1 } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { 2 } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { 2 } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { 2 } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 4 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 4 } }
		};
		[Theory]
		[MemberData(nameof(Futuras))]
		public void FiltrarFuturasTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarFuturas(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarFuturasTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarFuturas(CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> FuturasConFlow => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { 1 } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { 2 } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 4 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { 4 } }
		};
		[Theory]
		[MemberData(nameof(FuturasConFlow))]
		public void FuturasConFlowTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarFuturasConFlow(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FuturasConFlowTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarFuturasConFlow(CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		public static TheoryData<List<Suscripcion>, HashSet<long>> PagosEnCurso => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, new HashSet<long>() { } },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, new HashSet<long>() { } },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_04_USUARIO_PAGO_ACTIVO, new HashSet<long>() { 1 } },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, new HashSet<long>() { 1 } },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, new HashSet<long>() { 2 } },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, new HashSet<long>() { } },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, new HashSet<long>() { } },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, new HashSet<long>() { 1 } },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, new HashSet<long>() { 2 } },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, new HashSet<long>() { 2 } },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, new HashSet<long>() { } },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, new HashSet<long>() { } },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, new HashSet<long>() { 2 } },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, new HashSet<long>() { 4 } },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, new HashSet<long>() { } }
		};
		[Theory]
		[MemberData(nameof(PagosEnCurso))]
		public void FiltrarPagosEnCursoTest(List<Suscripcion> suscripciones, HashSet<long> expectedIds) {
			List<Suscripcion> retorno = suscripcionBcp.FiltrarPagosEnCurso(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}

		[Fact]
		public void FiltrarPagosEnCursoTest_SinDummy() {
			HashSet<long> expectedIds = [2];
			List<Suscripcion> retorno = suscripcionBcp.FiltrarPagosEnCurso(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.Equal(expectedIds.Count, retorno.Count);
			Assert.All(retorno, s => Assert.Contains(s.Id, expectedIds));
		}


		public static TheoryData<List<Suscripcion>, bool> AlgunaPagoEnCurso => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, false },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, false },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, false },
			{ CASO_04_USUARIO_PAGO_ACTIVO, true },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, true },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, false },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, false },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, true },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, true },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, false },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, false },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, true },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, true },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, true },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, true },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, true },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, false },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, false },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, true },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, true },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, false }
		};
		[Theory]
		[MemberData(nameof(AlgunaPagoEnCurso))]
		public void AlgunaConPagoEnCursoTest(List<Suscripcion> suscripciones, bool expectedResult) {
			bool retorno = suscripcionBcp.AlgunaConPagoEnCurso(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedResult, retorno);
		}

		[Fact]
		public void AlgunaConPagoEnCursoTest_SinDummy() {
			bool retorno = suscripcionBcp.AlgunaConPagoEnCurso(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.True(retorno);
		}

		public static TheoryData<List<Suscripcion>, DateTime?> ProximaFechaCobro => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, null },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, null },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, null },
			{ CASO_04_USUARIO_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, FECHA_DUMMY.AddDays(-15) },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, null },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, null },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, FECHA_DUMMY.AddDays(-15) },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, null },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, null },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, FECHA_DUMMY },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, FECHA_DUMMY },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, FECHA_DUMMY },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, null },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, null },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, null }
		};
		[Theory]
		[MemberData(nameof(ProximaFechaCobro))]
		public void ProximaFechaCobroTest(List<Suscripcion> suscripciones, DateTime? expectedResult) {
			DateTime? retorno = suscripcionBcp.ProximaFechaCobro(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedResult, retorno);
		}

		[Fact]
		public void ProximaFechaCobro_SinDummy() {
			DateTime? retorno = suscripcionBcp.ProximaFechaCobro(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.Equal(FECHA_DUMMY.AddDays(15), retorno);
		}


		public static TheoryData<List<Suscripcion>, DateTime?> ProximaFechaExpiracion => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, null },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, null },
			{ CASO_04_USUARIO_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, null },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, null },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, null },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, null },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, null },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, null },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, null },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, FECHA_DUMMY.AddDays(45) },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, FECHA_DUMMY.AddDays(45) },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, FECHA_DUMMY.AddDays(15) }
		};
		[Theory]
		[MemberData(nameof(ProximaFechaExpiracion))]
		public void ProximaFechaExpiracionTest(List<Suscripcion> suscripciones, DateTime? expectedResult) {
			DateTime? retorno = suscripcionBcp.ProximaFechaExpiracion(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedResult, retorno);
		}

		[Fact]
		public void ProximaFechaExpiracionTest_SinDummy() {
			DateTime? retorno = suscripcionBcp.ProximaFechaExpiracion(CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO);
			Assert.Equal(FECHA_DUMMY.AddDays(15), retorno);
		}

		public static TheoryData<List<Suscripcion>, DateTime> ProximaFechaSinSuscripcion => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, FECHA_DUMMY },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, FECHA_DUMMY },
			{ CASO_04_USUARIO_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, FECHA_DUMMY },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, FECHA_DUMMY },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, FECHA_DUMMY },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, FECHA_DUMMY.AddDays(15) },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, FECHA_DUMMY },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, FECHA_DUMMY },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, FECHA_DUMMY },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15)},
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, FECHA_DUMMY },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, FECHA_DUMMY.AddDays(45) },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, FECHA_DUMMY.AddDays(45) },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, FECHA_DUMMY.AddDays(15) },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, FECHA_DUMMY.AddDays(15) },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, FECHA_DUMMY.AddDays(15) }
		};
		[Theory]
		[MemberData(nameof(ProximaFechaSinSuscripcion))]
		public void ProximaFechaSinSuscripcionTest(List<Suscripcion> suscripciones, DateTime expectedResult) {
			DateTime retorno = suscripcionBcp.ProximaFechaSinSuscripcion(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedResult, retorno);
		}

		[Fact]
		public void ProximaFechaSinSuscripcionTest_SinDummy() {
			DateTime retorno = suscripcionBcp.ProximaFechaSinSuscripcion(CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT);
			Assert.Equal(FECHA_DUMMY.AddDays(45), retorno);
		}

		[Fact]
		public async Task ObtenerVigentesPorSubTest() {
			suscripcionDao.ObtenerPorSub(Arg.Any<string>(), true).Returns([
				SuscripcionDummy(id: 1, vigencia: true),
				SuscripcionDummy(id: 2, vigencia: true)
			]);

			List<Suscripcion> retorno = await suscripcionBcp.ObtenerVigentesPorSub("sub-test");
			Assert.Equal(2, retorno.Count);
			await suscripcionDao.Received(1).ObtenerPorSub(Arg.Any<string>(), true);
		}

		[Fact]
		public async Task ObtenerPorFlowSubscriptionIdTest() {
			suscripcionDao.ObtenerPorFlowSubscriptionId(Arg.Any<string>()).Returns(
				SuscripcionDummy(id: 1, vigencia: true)
			);

			Suscripcion? retorno = await suscripcionBcp.ObtenerPorFlowSubscriptionId("flow-subscription-id-test");
			Assert.NotNull(retorno);
			Assert.Equal(1, retorno.Id);
			await suscripcionDao.Received(1).ObtenerPorFlowSubscriptionId(Arg.Any<string>());
		}

		public static TheoryData<List<Suscripcion>, bool> TienePlanEmpresa => new() {
			{ CASO_01_USUARIO_SIN_SUSCRIPCIONES, false },
			{ CASO_02_USUARIO_GRATUITA_ACTIVA, true },
			{ CASO_03_USUARIO_GRATUITA_EXPIRADA, false },
			{ CASO_04_USUARIO_PAGO_ACTIVO, true },
			{ CASO_05_USUARIO_PAGO_EXPIRADA, false },
			{ CASO_06_USUARIO_PAGO_CANCELADA_ACTIVA, true },
			{ CASO_07_USUARIO_PAGO_CANCELADA_EXPIRADA, false },
			{ CASO_08_USUARIO_GRAT_ANT_PAGO_ACTIVO, true },
			{ CASO_09_USUARIO_GRAT_ANT_PAGO_EXPIRADA, false },
			{ CASO_10_USUARIO_GRAT_ANT_PAGO_CANCELADA_ACTIVA, true },
			{ CASO_11_USUARIO_GRAT_ANT_PAGO_CANCELADA_EXPIRADA, false },
			{ CASO_12_USUARIO_PAGO_PEND_SIN_PREVIAS, false },
			{ CASO_13_USUARIO_GRAT_ANT_PAGO_PEND, false },
			{ CASO_14_USUARIO_GRAT_ACT_PAGO_PEND_FUT, true },
			{ CASO_15_USUARIO_PAGO_CANC_ACT_PAGO_PEND_FUT, true },
			{ CASO_16_USUARIO_PAGO_CANC_EXP_PAGO_PEND_FUT, false },
			{ CASO_17_USUARIO_GRAT_ACTIVA_POST_GRAT, true },
			{ CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT, true },
			{ CASO_19_USUARIO_PAGO_CANC_EXP_PAGO_ACTIVO, true },
			{ CASO_20_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_PEND_FUT, true },
			{ CASO_21_USUARIO_GRAT_ANT_PAGO_CANC_EXP_PAGO_CANC_ACT_PAGO_CANC_FUT, true }
		};
		[Theory]
		[MemberData(nameof(TienePlanEmpresa))]
		public void TienePlanEmpresaTest(List<Suscripcion> suscripciones, bool expectedResult) {
			bool retorno = suscripcionBcp.TienePlanEmpresa(suscripciones, FECHA_DUMMY);
			Assert.Equal(expectedResult, retorno);
		}

		[Fact]
		public void TienePlanEmpresaTest_SinDummy() {
			bool retorno = suscripcionBcp.TienePlanEmpresa(CASO_18_USUARIO_PAGO_CANC_ACT_POST_GRAT);
			Assert.True(retorno);
		}

		[Theory]
		[MemberData(nameof(TienePlanEmpresa))]
		public async Task ConsultaTienePlanEmpresaTest(List<Suscripcion> suscripciones, bool expectedResult) {
			suscripcionDao.ObtenerPorSub(Arg.Any<string>(), true).Returns(suscripciones);

			bool retorno = await suscripcionBcp.ConsultaTienePlanEmpresa("sub-test");
			Assert.Equal(expectedResult, retorno);
			await suscripcionDao.Received(1).ObtenerPorSub(Arg.Any<string>(), true);
		}

		[Fact]
		public async Task CancelarTest_Valido() {
			await suscripcionBcp.Cancelar(SuscripcionDummy(id: 10, estado: 1 /* Activa */, flowSubscriptionId: "flow-subscription-id-test-cancel"));
			await flowHelper.Received(1).SubscriptionCancel("flow-subscription-id-test-cancel");
			await suscripcionDao.Received(1).Actualizar(Arg.Is<Suscripcion>(s => s.Id == 10 && s.Estado == 2 && s.FechaCancelacion == FECHA_DUMMY));
		}

		[Fact]
		public async Task CancelarTest_SinFlowSubscriptionId() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionBcp.Cancelar(SuscripcionDummy(id: 10, estado: 1 /* Activa */, flowSubscriptionId: null)));
			Assert.Equal(TipoErrorValidacion.TipoNoValido, ex.TipoErrorValidacion);
			await flowHelper.DidNotReceive().SubscriptionCancel(Arg.Any<string>());
			await suscripcionDao.DidNotReceive().Actualizar(Arg.Any<Suscripcion>());
		}

		[Fact]
		public async Task CancelarTest_EnCreacion() {
			ErrorValidacion ex = await Assert.ThrowsAsync<ErrorValidacion>(() => suscripcionBcp.Cancelar(SuscripcionDummy(id: 10, estado: 5 /* En Creacion */)));
			Assert.Equal(TipoErrorValidacion.EstadoNoValido, ex.TipoErrorValidacion);
			await flowHelper.DidNotReceive().SubscriptionCancel(Arg.Any<string>());
			await suscripcionDao.DidNotReceive().Actualizar(Arg.Any<Suscripcion>());
		}

		[Fact]
		public async Task CancelarTest_YaCancelada() {
			await suscripcionBcp.Cancelar(SuscripcionDummy(id: 10, estado: 2 /* Cancelada */));
			await flowHelper.DidNotReceive().SubscriptionCancel(Arg.Any<string>());
			await suscripcionDao.DidNotReceive().Actualizar(Arg.Any<Suscripcion>());
		}

		[Fact]
		public async Task EliminarTest_Valido() {
			await suscripcionBcp.Eliminar(SuscripcionDummy(id: 10, vigencia: true));
			await suscripcionDao.Received(1).Actualizar(Arg.Is<Suscripcion>(s => s.Id == 10 && !s.Vigencia && s.FechaEliminacion == FECHA_DUMMY));
		}

		[Fact]
		public async Task EliminarTest_YaEliminado() {
			await suscripcionBcp.Eliminar(SuscripcionDummy(id: 10, vigencia: false));
			await suscripcionDao.DidNotReceive().Actualizar(Arg.Any<Suscripcion>());
		}

		[Fact]
		public async Task EliminarCreacionNoConfirmadaTest() {
			await suscripcionBcp.EliminarCreacionNoConfirmada([
				SuscripcionDummy(id: 1, estado: 5 /* En Creación */),
				SuscripcionDummy(id: 2, estado: 5 /* En Creación */),
				SuscripcionDummy(id: 3, estado: 5 /* En Creación */),
				SuscripcionDummy(id: 4, estado: 1 /* Activa */),
			]);
			await suscripcionDao.Received(3).Actualizar(Arg.Is<Suscripcion>(s => !s.Vigencia && s.FechaEliminacion == FECHA_DUMMY));
			await suscripcionDao.DidNotReceive().Actualizar(Arg.Is<Suscripcion>(s => s.Id == 4));
		}

		[Fact]
		public async Task CrearTest() {
			suscripcionDao.Insertar(Arg.Any<Suscripcion>()).Returns(99);
			Suscripcion retorno = await suscripcionBcp.Crear("sub-test-crear", 9, FECHA_DUMMY, FECHA_DUMMY.AddMonths(1), 1 /* Activa */);
			Assert.Equal(99, retorno.Id);
			Assert.Equal("sub-test-crear", retorno.Sub);
			Assert.Equal(9, retorno.IdPlan);
			Assert.Equal(FECHA_DUMMY, retorno.FechaInicio);
			Assert.Equal(FECHA_DUMMY.AddMonths(1), retorno.FechaExpiracion);
			Assert.Equal(1, retorno.Estado);
			Assert.True(retorno.Vigencia);
			Assert.Equal(FECHA_DUMMY, retorno.FechaCreacion);
			await suscripcionDao.Received(1).Insertar(Arg.Any<Suscripcion>());
		}

		[Fact]
		public async Task ModificarTest() {
			await suscripcionBcp.Modificar(SuscripcionDummy());
			await suscripcionDao.Received(1).Actualizar(Arg.Any<Suscripcion>());
		}
	}
}
