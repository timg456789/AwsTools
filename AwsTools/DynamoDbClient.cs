
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;

namespace AwsTools
{
    public class DynamoDbClient<T> where T : IModel, new()
    {
        private IAmazonDynamoDB Client { get; }
        private ILogging Logging { get; }

        public DynamoDbClient(IAmazonDynamoDB client, ILogging logging)
        {
            Client = client;
            Logging = logging;
        }
        
        public List<T> Insert(List<T> ads)
        {
            var adBatchInsert = Conversion<T>.GetBatchInserts(ads);

            var unprocessed = new List<T>();
            if (adBatchInsert[new T().GetTable()].Any())
            {
                var response = Client.BatchWriteItemAsync(adBatchInsert).Result;
                var unprocessedBatch = response
                    .UnprocessedItems
                    .SelectMany(y => y.Value.Select(x => Conversion<T>.ConvertToPoco(x.PutRequest.Item)));
                unprocessed.AddRange(unprocessedBatch);
            }

            return unprocessed;
        }
    }
}
