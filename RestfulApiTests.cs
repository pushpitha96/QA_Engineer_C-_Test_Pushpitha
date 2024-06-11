using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using RestSharp;
using Xunit;
using System.Text.Json;

namespace QA_Code_test
{
    public class RestfulApiTests
    {
        private const string BaseUrl = "https://api.restful-api.dev/";

        /// <summary>
        /// Test to get the list of all objects from the API.
        /// </summary>
        [Fact]
        public async Task GetAllObjects_ShouldReturnListOfObjects()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("objects", Method.Get);
            var response = await client.ExecuteAsync(request);

            // Verify the response status code is OK (200)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // Verify the response content is not empty
            response.Content.Should().NotBeNullOrEmpty();
            // Additional verification to ensure response contains a list of objects
            response.Content.Should().Contain("\"id\"");
            response.Content.Should().Contain("\"name\"");
        }

        /// <summary>
        /// Test to add a new object using POST.
        /// </summary>
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

            // Verify the response status code is either Created (201) or OK (200)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
            // Verify the response content contains the added object's name
            response.Content.Should().Contain("NewObject");
        }


        /// <summary>
        /// Test to get a single object using the ID of the object added in the previous test.
        /// </summary>
        [Fact]
        public async Task GetSingleObject_ShouldReturnCorrectObject()
        {
            var client = new RestClient(BaseUrl);
            var addRequest = new RestRequest("objects", Method.Post);
            addRequest.AddHeader("Content-Type", "application/json");
            addRequest.AddHeader("User-Agent", "Mozilla/5.0");
            addRequest.AddJsonBody(new
            {
                name = "ObjectToGet",
                data = new
                {
                    color = "Red",
                    capacity = "128 GB"
                }
            });
            var addResponse = await client.ExecuteAsync(addRequest);
            var addedObjectId = ExtractIdFromResponse(addResponse.Content);

            var getRequest = new RestRequest($"objects/{addedObjectId}", Method.Get);
            var getResponse = await client.ExecuteAsync(getRequest);

            // Verify the response status code is OK (200)
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            // Verify the response content contains the object's name
            getResponse.Content.Should().Contain("ObjectToGet");
        }

        /// <summary>
        /// Test to update the object added in the previous test using PUT.
        /// </summary>
        [Fact]
        public async Task UpdateObject_ShouldReturnUpdatedObject()
        {
            var client = new RestClient(BaseUrl);
            var addRequest = new RestRequest("objects", Method.Post);
            addRequest.AddHeader("Content-Type", "application/json");
            addRequest.AddHeader("User-Agent", "Mozilla/5.0");
            addRequest.AddJsonBody(new
            {
                name = "ObjectToUpdate",
                data = new
                {
                    color = "Green",
                    capacity = "64 GB"
                }
            });
            var addResponse = await client.ExecuteAsync(addRequest);
            var addedObjectId = ExtractIdFromResponse(addResponse.Content);

            var updateRequest = new RestRequest($"objects/{addedObjectId}", Method.Put);
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

            // Verify the response status code is OK (200)
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            // Verify the response content contains the updated object's name
            updateResponse.Content.Should().Contain("UpdatedObject");
        }

        /// <summary>
        /// Test to delete the object using DELETE.
        /// </summary>
        [Fact]
        public async Task DeleteObject_ShouldReturnSuccess()
        {
            var client = new RestClient(BaseUrl);
            var addRequest = new RestRequest("objects", Method.Post);
            addRequest.AddJsonBody(new { name = "ObjectToDelete", data = new { Generation = "4th", Price = "519.99", Capacity = "256 GB" } });
            var addResponse = await client.ExecuteAsync(addRequest);
            var addedObjectId = ExtractIdFromResponse(addResponse.Content);

            var deleteRequest = new RestRequest($"objects/{addedObjectId}", Method.Delete);
            var deleteResponse = await client.ExecuteAsync(deleteRequest);

            // Verify the response status code is either NoContent (204) or OK (200)
            deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);

            var getRequest = new RestRequest($"objects/{addedObjectId}", Method.Get);
            var getResponse = await client.ExecuteAsync(getRequest);

            // Verify the response status code is NotFound (404)
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Helper method to extract the ID from the response content.
        /// </summary>
        /// <param name="responseContent">The response content in JSON format.</param>
        /// <returns>The extracted ID as a string.</returns>
        private string ExtractIdFromResponse(string responseContent)
        {
            // Parse the JSON response content to extract the ID
            var json = JsonDocument.Parse(responseContent);
            return json.RootElement.GetProperty("id").GetString();
        }
    }
}