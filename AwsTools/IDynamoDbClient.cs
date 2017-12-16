using System.Collections.Generic;
using System.Threading.Tasks;

namespace AwsTools
{
    public interface IDynamoDbClient<T> where T : IModel, new()
    {
        Task<List<T>> Insert(List<T> ads);
    }
}