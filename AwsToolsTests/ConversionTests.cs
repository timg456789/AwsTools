using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Xunit;

namespace AwsToolsTests
{
    public class ConversionTests
    {
        [Fact]
        public void Check_Poco()
        {
            var item = new Dictionary<string, AttributeValue>
            {
                {Guid.NewGuid().ToString(), new AttributeValue {S = "deprecated field"}},
                {"source", new AttributeValue {S = "http://url.com"}},
                {"labels", new AttributeValue {SS = new List<string> {"abc", "123"}}},
                {"hasFlag", new AttributeValue {S = "True"} }
            };
            var model = Conversion<ImageClassification>.ConvertToPoco(item);
            Assert.Equal("http://url.com", model.Source);
            Assert.Equal("abc", model.Labels[0]);
            Assert.Equal("123", model.Labels[1]);
            Assert.True(model.HasFlag);
        }
        
        [Fact]
        public void Check_DynamoDB_Model()
        {
            var model = new ImageClassification();

            var random = new Random();
            model.Source = Guid.NewGuid().ToString();
            model.PageId = random.Next();
            model.Artist = Guid.NewGuid().ToString();
            model.ImageId = random.Next();
            model.Name = Guid.NewGuid().ToString();
            model.OriginalArtist = Guid.NewGuid().ToString();
            model.Date = "1866";
            model.S3Path = "tgonzalez/something.jpg";

            var awsToolsConversion = Conversion<ImageClassification>.ConvertToDynamoDb(model);

            Assert.Equal(
                $@"{{""artist"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.Artist}"",""SS"":[]}},""date"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.Date}"",""SS"":[]}},""imageId"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":""{model.ImageId}"",""NS"":[],""NULL"":false,""S"":null,""SS"":[]}},""name"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.Name}"",""SS"":[]}},""originalArtist"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.OriginalArtist}"",""SS"":[]}},""pageId"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":""{model.PageId}"",""NS"":[],""NULL"":false,""S"":null,""SS"":[]}},""s3Path"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.S3Path}"",""SS"":[]}},""source"":{{""B"":null,""BOOL"":false,""IsBOOLSet"":false,""BS"":[],""L"":[],""IsLSet"":false,""M"":{{}},""IsMSet"":false,""N"":null,""NS"":[],""NULL"":false,""S"":""{model.Source}"",""SS"":[]}}}}",
                JsonConvert.SerializeObject(awsToolsConversion));
        }
    }
}
