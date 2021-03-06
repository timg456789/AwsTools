﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

namespace AwsTools
{
    public class Conversion<T> where T : IModel, new()
    {

        public static Dictionary<string, AttributeValue> ConvertToDynamoDb(T model)
        {
            var kvp = new Dictionary<string, AttributeValue>();

            foreach (var property in model.GetType().GetProperties())
            {
                object value = property.GetValue(model);
                AttributeValue attributeValue = null;

                if (property.PropertyType == typeof(string))
                {
                    if (!string.IsNullOrWhiteSpace(value?.ToString()))
                    {
                        attributeValue = new AttributeValue {S = value.ToString()};
                    }
                }
                else if (property.PropertyType == typeof(int) ||
                         property.PropertyType == typeof(int?))
                {
                    if (value != null && (int) value > 0)
                    {
                        attributeValue = new AttributeValue {N = value.ToString()};
                    }
                }
                else if (property.PropertyType == typeof(long) ||
                         property.PropertyType == typeof(long?))
                {
                    if (value != null && (long) value > 0)
                    {
                        attributeValue = new AttributeValue {N = value.ToString()};
                    }
                }
                else if (property.PropertyType == typeof(decimal) ||
                    property.PropertyType == typeof(decimal?))
                {
                    if (value != null && (decimal) value > 0)
                    {
                        attributeValue = new AttributeValue {N = value.ToString()};
                    }
                }
                else if (property.PropertyType == typeof(List<string>))
                {
                    if (value != null && ((List<string>) value).Any())
                    {
                        attributeValue = new AttributeValue((List<string>) value);
                    }
                }
                else if (property.PropertyType == typeof(bool) ||
                         property.PropertyType == typeof(bool?))
                {
                    if (value != null)
                    {
                        attributeValue =
                            new AttributeValue
                            {
                                S = value.ToString()
                            }; // I like to put bool into string so the field can be indexed.
                    }
                }
                else
                {
                    throw new Exception(
                        $"Unknown data type \"{property.PropertyType.Name}\" type for property {property.Name}.");
                }

                if (attributeValue != null)
                {
                    var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                    if (attribute != null) // Property may not be marked. Skip it and consider a non-persitent field.
                    {
                        kvp.Add(attribute.PropertyName, attributeValue);
                    }
                }
            }

            return kvp
                .OrderBy(x => x.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static List<T> ConvertToPoco(List<Dictionary<string, AttributeValue>> dynamoDbModels)
        {
            return dynamoDbModels.Select(ConvertToPoco).ToList();
        }

        public static T ConvertToPoco(Dictionary<string, AttributeValue> dynamoDbModel)
        {
            if (dynamoDbModel == default(Dictionary<string, AttributeValue>))
            {
                return default(T);
            }
            var model = new T();

            foreach (string key in dynamoDbModel.Keys)
            {
                var property = model
                    .GetType()
                    .GetProperties()
                    .SingleOrDefault(x =>
                        key.Equals(x.GetCustomAttribute<JsonPropertyAttribute>().PropertyName,
                            StringComparison.Ordinal));
                if (property == null)
                {
                    continue;
                }
                object value;
                if (property.PropertyType == typeof(string))
                {
                    value = dynamoDbModel[key].S;
                }
                else if (property.PropertyType == typeof(int) ||
                         property.PropertyType == typeof(int?))
                {
                    value = Convert.ToInt32(dynamoDbModel[key].N);
                }
                else if (property.PropertyType == typeof(long) ||
                         property.PropertyType == typeof(long?))
                {
                    value = Convert.ToInt64(dynamoDbModel[key].N);
                }
                else if (property.PropertyType == typeof(decimal) ||
                    property.PropertyType == typeof(decimal?))
                {
                    value = Convert.ToDecimal(dynamoDbModel[key].N);
                }
                else if (property.PropertyType == typeof(List<string>))
                {
                    value = dynamoDbModel[key].SS;
                }
                else if (property.PropertyType == typeof(bool))
                {
                    value = Convert.ToBoolean(dynamoDbModel[key].S);
                }
                else
                {
                    throw new Exception(
                        $"Unknown data type \"{property.PropertyType.Name}\" type for property {property.Name}.");
                }

                property.SetValue(model, value);
            }

            return model;
        }

        public static Dictionary<string, List<WriteRequest>> GetBatchInserts(List<T> pocoModels)
        {
            var batchWrite = new Dictionary<string, List<WriteRequest>> {[new T().GetTable()] = new List<WriteRequest>()};

            foreach (var pocoModel in pocoModels)
            {
                var dyamoDbModel = ConvertToDynamoDb(pocoModel);
                var putRequest = new PutRequest(dyamoDbModel);
                var writeRequest = new WriteRequest(putRequest);
                batchWrite[new T().GetTable()].Add(writeRequest);
            }

            return batchWrite;
        }

        public static Dictionary<string, List<WriteRequest>> GetBatchDeletes(List<T> pocoModels, string tableName)
        {
            var batchWrite = new Dictionary<string, List<WriteRequest>> {[tableName] = new List<WriteRequest>()};

            foreach (var data in pocoModels)
            {
                var deleteKey = data.GetKey();
                var putRequest = new DeleteRequest(deleteKey);
                var writeRequest = new WriteRequest(putRequest);
                batchWrite[tableName].Add(writeRequest);
            }

            return batchWrite;
        }
    }

}
