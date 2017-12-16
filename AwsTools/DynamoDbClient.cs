
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

        public async void Insert(T model)
        {
            var converted = Conversion<T>.ConvertToDynamoDb(model);
            await Client.PutItemAsync(new PutItemRequest(model.GetTable(), converted)).ConfigureAwait(false);
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

    }
}
