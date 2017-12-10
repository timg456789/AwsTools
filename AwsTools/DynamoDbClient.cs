
using System.Collections.Generic;
using System.Linq;
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

        public void Insert(T model)
        {
            var converted = Conversion<T>.ConvertToDynamoDb(model);
            var response = Client.PutItemAsync(new PutItemRequest(model.GetTable(), converted)).Result;
        }

        public List<T> Insert(List<T> models)
        {
            if (!models.Any())
            {
                return new List<T>();
            }

            var batches = Conversion<T>.GetBatchInserts(models);

            var unprocessed = new List<T>();
            var response = Client.BatchWriteItemAsync(batches).Result;
            var unprocessedBatch = response
                .UnprocessedItems
                .SelectMany(y => y.Value.Select(x => Conversion<T>.ConvertToPoco(x.PutRequest.Item)));
            unprocessed.AddRange(unprocessedBatch);

            return unprocessed;
        }

    }
}
