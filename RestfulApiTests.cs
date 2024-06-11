using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using RestSharp;
using Xunit;
using System.Text.Json;

namespace QA_Code_test
{
    // This class contains tests for the Restful API and implements IDisposable to clean up after tests
    public class RestfulApiTests : IClassFixture<TestFixture>, IDisposable
    {
        // Base URL for the API
        private const string BaseUrl = "https://api.restful-api.dev/";
        private readonly TestFixture _fixture;
        private readonly RestClient _client;

        // Constructor to initialize the fixture and RestClient
        public RestfulApiTests(TestFixture fixture)
        {
            _fixture = fixture;
            _client = new RestClient(BaseUrl);
        }

        // Test to get all objects and verify the response contains expected data
        [Fact]
        public async Task GetAllObjects_ShouldReturnListOfObjects()
        {
            var request = new RestRequest("objects", Method.Get); // Create a GET request
            var response = await _client.ExecuteAsync(request); // Execute the request

            // Debugging statement to output the response content
            Console.WriteLine("GetAllObjects response content: " + response.Content);

            // Assert that the response status is OK
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // Assert that the response content is not null or empty
            response.Content.Should().NotBeNullOrEmpty();
            // Assert that the response contains specific fields
            response.Content.Should().Contain("\"id\"");
            response.Content.Should().Contain("\"name\"");
        }

        // Test to add a new object and verify the response contains the expected data
        [Fact]
        public async Task AddObject_ShouldReturnSuccess()
        {
            var request = new RestRequest("objects", Method.Post); // Create a POST request
            request.AddJsonBody(new
            {
                name = "NewObject",
                data = new { Generation = "4th", Price = "519.99", Capacity = "256 GB" }
            }); // Add JSON body with the new object data

            var response = await _client.ExecuteAsync(request); // Execute the request

            // Debugging statement to output the response content
            Console.WriteLine("AddObject response content: " + response.Content);

            // Assert that the response status is either Created or OK
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
            
            // Parse the response and verify the "name" field contains "NewObject"
            var responseObject = JsonDocument.Parse(response.Content);
            var addedObject = responseObject.RootElement.GetProperty("name").GetString();
            addedObject.Should().Be("NewObject");

            // Extract the ID of the added object and store it in the fixture
            _fixture.AddedObjectId = ExtractIdFromResponse(response.Content);
        }

        // Test to get a single object by ID and verify the response contains the expected data
        [Fact]
        public async Task GetSingleObject_ShouldReturnCorrectObject()
        {
            // Ensure the object is added before attempting to get it
            await AddObject_ShouldReturnSuccess();

            var getRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Get); // Create a GET request with the object's ID
            var getResponse = await _client.ExecuteAsync(getRequest); // Execute the request

            // Debugging statements to output the request URI and response content
            Console.WriteLine("GetSingleObject request URI: " + getRequest.Resource);
            Console.WriteLine("GetSingleObject response content: " + getResponse.Content);

            // Assert that the response status is OK
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Parse the response and verify the "name" field contains "NewObject"
            var responseObject = JsonDocument.Parse(getResponse.Content);
            var retrievedObject = responseObject.RootElement.GetProperty("name").GetString();
            retrievedObject.Should().Be("NewObject");
        }

        // Test to update an object and verify the response contains the updated data
        [Fact]
        public async Task UpdateObject_ShouldReturnUpdatedObject()
        {
            // Ensure the object is added before attempting to update it
            await AddObject_ShouldReturnSuccess();

            var updateRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Put); // Create a PUT request with the object's ID
            updateRequest.AddHeader("Content-Type", "application/json");
            updateRequest.AddHeader("User-Agent", "Mozilla/5.0"); // Add necessary headers
            updateRequest.AddJsonBody(new
            {
                name = "UpdatedObject",
                data = new
                {
                    color = "Yellow",
                    capacity = "256 GB"
                }
            }); // Add JSON body with the updated object data

            var updateResponse = await _client.ExecuteAsync(updateRequest); // Execute the request

            // Debugging statements to output the request URI and response content
            Console.WriteLine("UpdateObject request URI: " + updateRequest.Resource);
            Console.WriteLine("UpdateObject response content: " + updateResponse.Content);

            // Assert that the response status is OK
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Parse the response and verify the "name" field contains "UpdatedObject"
            var responseObject = JsonDocument.Parse(updateResponse.Content);
            var updatedObject = responseObject.RootElement.GetProperty("name").GetString();
            updatedObject.Should().Be("UpdatedObject");
        }

        // Test to delete an object and verify it no longer exists
        [Fact]
        public async Task DeleteObject_ShouldReturnSuccess()
        {
            // Ensure the object is added before attempting to delete it
            await AddObject_ShouldReturnSuccess();

            var deleteRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Delete); // Create a DELETE request with the object's ID
            var deleteResponse = await _client.ExecuteAsync(deleteRequest); // Execute the request

            // Debugging statements to output the request URI and response content
            Console.WriteLine("DeleteObject delete request URI: " + deleteRequest.Resource);
            Console.WriteLine("DeleteObject delete response content: " + deleteResponse.Content);

            // Assert that the response status is either NoContent or OK
            deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

            var getRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Get); // Create a GET request to verify deletion
            var getResponse = await _client.ExecuteAsync(getRequest); // Execute the request

            // Debugging statements to output the request URI and response content
            Console.WriteLine("DeleteObject get request URI: " + getRequest.Resource);
            Console.WriteLine("DeleteObject get response content: " + getResponse.Content);

            // Assert that the response status is NotFound, indicating the object was successfully deleted
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Helper method to extract the ID from the response content
        private string ExtractIdFromResponse(string responseContent)
        {
            var json = JsonDocument.Parse(responseContent);
            return json.RootElement.GetProperty("id").GetString();
        }

        // Cleanup method to ensure each test starts with a clean state
        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_fixture.AddedObjectId))
            {
                var deleteRequest = new RestRequest($"objects/{_fixture.AddedObjectId}", Method.Delete);
                var deleteResponse = _client.ExecuteAsync(deleteRequest).Result;
                // Optionally handle deleteResponse here
                _fixture.AddedObjectId = null; // Reset the AddedObjectId
            }
        }
    }

    // Class to store shared test data
    public class TestFixture
    {
        public string AddedObjectId { get; set; }
    }
}