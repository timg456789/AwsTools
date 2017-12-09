using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace AwsTools
{
    public interface IModel
    {
        Dictionary<string, AttributeValue> GetKey();
        string GetTable();
    }
}
