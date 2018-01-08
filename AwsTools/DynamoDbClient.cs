
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AwsTools
{
    public class DynamoDbClient<T> : IDynamoDbClient<T> where T : IModel, new()
    {
        private IAmazonDynamoDB Client { get; }
        private ILogging Logging { get; }

        public DynamoDbClient(IAmazonDynamoDB client, ILogging logging)
        {
            Client = client;
            Logging = logging;
        }

        public async Task Insert(T model)
        {
            var converted = Conversion<T>.ConvertToDynamoDb(model);
            await Client
                .PutItemAsync(new PutItemRequest(model.GetTable(), converted))
                .ConfigureAwait(false);
        }

        public async Task<List<T>> Insert(List<T> models)
        {
            if (!models.Any())
            {
                return new List<T>();
            }

            var batches = Conversion<T>.GetBatchInserts(models);

            var unprocessed = new List<T>();
            var response = await Client.BatchWriteItemAsync(batches).ConfigureAwait(false);
            var unprocessedBatch = response
                .UnprocessedItems
                .SelectMany(y => y.Value.Select(x => Conversion<T>.ConvertToPoco(x.PutRequest.Item)));
            unprocessed.AddRange(unprocessedBatch);

            return unprocessed;
        }

        public async Task<T> Get(T model)
        {
            return await Get(model.GetKey()).ConfigureAwait(false);
        }

        public async Task<T> Get(Dictionary<string, AttributeValue> key)
        {
            var response = await Client.GetItemAsync(new T().GetTable(), key).ConfigureAwait(false);
            return Conversion<T>.ConvertToPoco(response.Item);
        }
        
        public async Task<T> Get(string index, Dictionary<string, AttributeValue> indexKeys)
        {
            var request = new QueryRequest(new T().GetTable())
            {
                IndexName = index,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(),
                ExpressionAttributeNames = new Dictionary<string, string>(),
                KeyConditionExpression = string.Empty,
                Limit = 1
            };

            foreach (var indexKey in indexKeys.Keys)
            {
                var safeKey = "#source" + indexKey;
                var safeValue = ":" + indexKey;

                request.ExpressionAttributeNames.Add(safeKey, indexKey);
                request.ExpressionAttributeValues.Add(safeValue, indexKeys[indexKey]);

                if (!string.IsNullOrWhiteSpace(request.KeyConditionExpression))
                {
                    request.KeyConditionExpression += " and ";
                }

                request.KeyConditionExpression += $"{safeKey} = {safeValue}";
            }

            var response = await Client.QueryAsync(request).ConfigureAwait(false);
            return Conversion<T>.ConvertToPoco(response.Items.FirstOrDefault());
        }

        protected virtual async Task<List<T>> GetBatchOf100(List<Dictionary<string, AttributeValue>> adKeyBatch)
        {
            var table = new T().GetTable();
            var fullAds = new List<T>();
            var fullAdBatchKeys = new Dictionary<string, KeysAndAttributes>
            {
                {
                    table,
                    new KeysAndAttributes { Keys = adKeyBatch }
                }
            };
            
            const int MAX_ATTEMPTS = 4;
            int currentAttempt = 0;
            BatchGetItemResponse response;
            do
            {
                currentAttempt += 1;
                var responseTask = Client.BatchGetItemAsync(fullAdBatchKeys).ConfigureAwait(false);
                response = await responseTask;
                fullAds.AddRange(response
                    .Responses[table]
                    .Select(Conversion<T>.ConvertToPoco));

                if (response.UnprocessedKeys.Any())
                {
                    var waitTime = ExponentialBackoff.GetWaitTime(currentAttempt, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
                    Logging.Log($"Read capacity exceeded. Failed to get {response.UnprocessedKeys[table].Keys.Count} records. " +
                                $"Attempt {currentAttempt} of {MAX_ATTEMPTS}. Retrying in {waitTime.TotalSeconds} seconds.");
                    new Sleeper(Logging).Sleep(waitTime);
                }
            } while (response.UnprocessedKeys.Any() && currentAttempt < MAX_ATTEMPTS);
            
            return fullAds;
        }

        public async Task<List<T>> Get(List<T> models)
        {
            var adKeyBatches = Batcher.Batch(100, models.Select(x => x.GetKey()).ToList());
            var fullAds = new List<T>();

            foreach (var adKeyBatch in adKeyBatches)
            {
                var batchResponse = GetBatchOf100(adKeyBatch).ConfigureAwait(false);
                fullAds.AddRange(await batchResponse);
            }
            return fullAds;
        }

    }
}
