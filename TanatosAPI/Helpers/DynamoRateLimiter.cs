using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Helpers {
    public class DynamoRateLimiter(IVariableEntornoHelper variableEntorno, IAmazonDynamoDB dynamo, IDateTimeProvider dateTimeProvider) : IRateLimiter {
        private readonly string TABLE_NAME = variableEntorno.Obtener("DYNAMODB_TABLE_NAME_RATE_LIMITS");
        
        public async Task<RateLimitResult> CheckAsync(string key, int maxRequests, TimeSpan window, RateLimitContext rateLimitContext) {
            DateTimeOffset now = dateTimeProvider.UtcNow;
            DateTimeOffset windowStart = now - window;

            QueryResponse response = await dynamo.QueryAsync(new QueryRequest {
                TableName = TABLE_NAME,
                KeyConditionExpression = "PK = :PK AND SK >= :WINDOW_START",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    [":PK"] = new(key),
                    [":WINDOW_START"] = new($"{windowStart.ToUnixTimeMilliseconds():D15}")
                },
                ScanIndexForward = true,
                Limit = maxRequests,
                ProjectionExpression = "SK"
            });

            int count = response.Items.Count;

            if (count >= maxRequests) {
                long oldest = long.Parse(response.Items[0]["SK"].S.Split('#')[0]);
                DateTimeOffset oldestTime = DateTimeOffset.FromUnixTimeMilliseconds(oldest);
                DateTimeOffset retryAfter = oldestTime + window;
                return new RateLimitResult() {
                    Allowed = false,
                    Remaining = 0,
                    RetryAfter = retryAfter
                };
            }

            int remaing = Math.Max(0, maxRequests - count - 1);

            string sk = $"{now.ToUnixTimeMilliseconds():D15}#{Guid.NewGuid():N}";
            await dynamo.PutItemAsync(new PutItemRequest {
                TableName = TABLE_NAME,
                Item = new Dictionary<string, AttributeValue> {
                    ["PK"] = new(key),
                    ["SK"] = new(sk),
                    ["TTL"] = new() { N = (now + window).ToUnixTimeSeconds().ToString() },
                    ["Path"] = new(rateLimitContext.Path),
                    ["Method"] = new(rateLimitContext.Method),
                    ["IP"] = new(rateLimitContext.IP),
                    ["MaxRequests"] = new(maxRequests.ToString()),
                    ["Remaining"] = new(remaing.ToString())
                }
            });

            return new RateLimitResult() { 
                Allowed = true,
                Remaining = remaing,
                RetryAfter = now
            };
        }
    }
}
