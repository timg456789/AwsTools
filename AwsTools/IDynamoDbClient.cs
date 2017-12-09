using System.Collections.Generic;

namespace AwsTools
{
    public interface IDynamoDbClient<T> where T : IModel, new()
    {
        List<T> Insert(List<T> ads);
    }
}