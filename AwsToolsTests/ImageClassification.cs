using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace AwsToolsTests
{
    class ImageClassification : IModel
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("pageId")]
        public int PageId { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("imageId")]
        public int ImageId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("originalArtist")]
        public string OriginalArtist { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("s3Path")]
        public string S3Path { get; set; }

        [JsonProperty("labels")]
        public List<string> Labels { get; set; }

        [JsonProperty("Labels")]
        public List<string> labels { get; set; }

        [JsonProperty("hasFlag")]
        public bool HasFlag { get; set; }

        [JsonProperty("intNullable")]
        public int? IntNullable { get; set; }

        [JsonProperty("decimal")]
        public decimal Decimal { get; set; }

        [JsonProperty("decimalNullable")]
        public decimal? DecimalNullable { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            throw new System.NotImplementedException();
        }

        public string GetTable()
        {
            throw new System.NotImplementedException();
        }
    }
}
