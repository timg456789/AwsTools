using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Xunit;

namespace AwsToolsTests
{
    public class ConversionTests
    {
        [Fact]
        public void Null_Dictionary_Gets_Null_Poco()
        {
            Assert.Null(Conversion<ImageClassification>.ConvertToPoco(default(Dictionary<string, AttributeValue>)));
        }

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
            var random = new Random();

            var model = new ImageClassification
            {
                Source = Guid.NewGuid().ToString(),
                PageId = random.Next(),
                Artist = Guid.NewGuid().ToString(),
                ImageId = random.Next(),
                Name = Guid.NewGuid().ToString(),
                OriginalArtist = Guid.NewGuid().ToString(),
                Date = "1866",
                S3Path = "tgonzalez/something.jpg",
                HasFlag = true,
                Labels = new List<string> {"abc"}
            };

            var awsToolsConversion = Conversion<ImageClassification>.ConvertToDynamoDb(model);
            Assert.Equal(model.Source, awsToolsConversion["source"].S);
            Assert.Equal(model.PageId.ToString(), awsToolsConversion["pageId"].N);
            Assert.Equal("True", awsToolsConversion["hasFlag"].S);
            Assert.Equal(awsToolsConversion["labels"].SS.Single(), model.Labels.Single());
            Assert.Equal("False", Conversion<ImageClassification>.ConvertToDynamoDb(new ImageClassification())["hasFlag"].S);
        }
    }
}
