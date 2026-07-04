using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.RateLimit;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Test.Helpers {
	public class DynamoRateLimiterTest {
		private readonly IVariableEntornoHelper variableEntorno = Substitute.For<IVariableEntornoHelper>();
		private readonly IAmazonDynamoDB dynamo = Substitute.For<IAmazonDynamoDB>();
		private readonly IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

		private readonly DynamoRateLimiter dynamoRateLimiter;

		private static readonly DateTime FECHA_DUMMY = new(2026, 1, 15, 14, 0, 0, DateTimeKind.Utc);
		private static readonly string TABLE_NAME_DUMMY = "NOMBRE_TABLA_DYNAMO_TEST";

		public DynamoRateLimiterTest() {
			dateTimeProvider.UtcNow.Returns(FECHA_DUMMY);
			variableEntorno.Obtener("DYNAMODB_TABLE_NAME_RATE_LIMITS").Returns(TABLE_NAME_DUMMY);

			dynamoRateLimiter = new(variableEntorno, dynamo, dateTimeProvider);
		}

		[Fact]
		public async Task CheckAsync_Valido() {
			dynamo.QueryAsync(Arg.Any<QueryRequest>()).Returns(new QueryResponse {
				Items = []
			});

			RateLimitResult retorno = await dynamoRateLimiter.CheckAsync("KEY_TEST", 100, TimeSpan.FromMinutes(1), new RateLimitContext {
				Sub = "sub-test-1",
				Path = "TEST",
				Method = "/Test",
			});

			Assert.True(retorno.Allowed);
			Assert.Equal(99, retorno.Remaining);
			Assert.Equal(FECHA_DUMMY, retorno.RetryAfter);
			await dynamo.Received(1).QueryAsync(Arg.Any<QueryRequest>());
			await dynamo.Received(1).PutItemAsync(Arg.Any<PutItemRequest>());
		}

		[Fact]
		public async Task CheckAsync_NotAllowed() {
			dynamo.QueryAsync(Arg.Any<QueryRequest>()).Returns(new QueryResponse {
				Items = [
					new Dictionary<string, AttributeValue>() {
						["SK"] = new AttributeValue($"{(new DateTimeOffset(FECHA_DUMMY)).ToUnixTimeMilliseconds():D15}#{Guid.NewGuid():N}")
					},
					[],
					[],
					[],
					[],
				]
			});

			RateLimitResult retorno = await dynamoRateLimiter.CheckAsync("KEY_TEST", 5, TimeSpan.FromMinutes(1), new RateLimitContext {
				Sub = "sub-test-1",
				Path = "TEST",
				Method = "/Test",
			});

			Assert.False(retorno.Allowed);
			Assert.Equal(0, retorno.Remaining);
			Assert.Equal(FECHA_DUMMY.AddMinutes(1), retorno.RetryAfter);

			await dynamo.Received(1).QueryAsync(Arg.Any<QueryRequest>());
			await dynamo.DidNotReceive().PutItemAsync(Arg.Any<PutItemRequest>());
		}
	}
}
