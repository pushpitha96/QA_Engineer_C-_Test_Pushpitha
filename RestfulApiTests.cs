using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using RestSharp;
using Xunit;
using System.Text.Json;

namespace QA_Code_test
{
    public class RestfulApiTests : IClassFixture<TestFixture>
    {
        private const string BaseUrl = "https://api.restful-api.dev/";
        private readonly TestFixture _fixture;

        public RestfulApiTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAllObjects_ShouldReturnListOfObjects()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("objects", Method.Get);
            var response = await client.ExecuteAsync(request);

            // Debugging statement
            Console.WriteLine("GetAllObjects response content: " + response.Content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNullOrEmpty();
            response.Content.Should().Contain("\"id\"");
            response.Content.Should().Contain("\"name\"");
        }

        [Fact]
        public async Task AddObject_ShouldReturnSuccess()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("objects", Method.Post);
            request.AddJsonBody(new
            {
                name = "NewObject",
                data = new { Generation = "4th", Price = "519.99", Capacity = "256 GB" }
            });
            var response = await client.ExecuteAsync(request);

            // Debugging statement
            Console.WriteLine("AddObject response content: " + response.Content);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
            response.Content.Should().Contain("NewObject");

            _fixture.AddedObjectId = ExtractIdFromResponse(response.Content);
        }

        [Fact]
        public async Task GetSingleObject_ShouldReturnCorrectObject()
        {
            var client = new RestClient(BaseUrl);
            var getRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Get);
            var getResponse = await client.ExecuteAsync(getRequest);

            // Debugging statements
            Console.WriteLine("GetSingleObject request URI: " + getRequest.Resource);
            Console.WriteLine("GetSingleObject response content: " + getResponse.Content);

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            getResponse.Content.Should().Contain("NewObject");
        }

        [Fact]
        public async Task UpdateObject_ShouldReturnUpdatedObject()
        {
            var client = new RestClient(BaseUrl);
            var updateRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Put);
            updateRequest.AddHeader("Content-Type", "application/json");
            updateRequest.AddHeader("User-Agent", "Mozilla/5.0");
            updateRequest.AddJsonBody(new
            {
                name = "UpdatedObject",
                data = new
                {
                    color = "Yellow",
                    capacity = "256 GB"
                }
            });
            var updateResponse = await client.ExecuteAsync(updateRequest);

            // Debugging statements
            Console.WriteLine("UpdateObject request URI: " + updateRequest.Resource);
            Console.WriteLine("UpdateObject response content: " + updateResponse.Content);

            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            updateResponse.Content.Should().Contain("UpdatedObject");
        }

        [Fact]
        public async Task DeleteObject_ShouldReturnSuccess()
        {
            var client = new RestClient(BaseUrl);
            var deleteRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Delete);
            var deleteResponse = await client.ExecuteAsync(deleteRequest);

            // Debugging statements
            Console.WriteLine("DeleteObject delete request URI: " + deleteRequest.Resource);
            Console.WriteLine("DeleteObject delete response content: " + deleteResponse.Content);

            deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

            var getRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Get);
            var getResponse = await client.ExecuteAsync(getRequest);

            // Debugging statements
            Console.WriteLine("DeleteObject get request URI: " + getRequest.Resource);
            Console.WriteLine("DeleteObject get response content: " + getResponse.Content);

            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private string ExtractIdFromResponse(string responseContent)
        {
            var json = JsonDocument.Parse(responseContent);
            return json.RootElement.GetProperty("id").GetString();
        }
    }

    public class TestFixture
    {
        public string AddedObjectId { get; set; }
    }
}