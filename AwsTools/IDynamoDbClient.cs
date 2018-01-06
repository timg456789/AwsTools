using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace AwsTools
{
    public interface IDynamoDbClient<T> where T : IModel, new()
    {
        Task Insert(T model);
        Task<List<T>> Insert(List<T> models);
        Task<T> Get(T model);
        Task<T> Get(Dictionary<string, AttributeValue> key);
    }
}