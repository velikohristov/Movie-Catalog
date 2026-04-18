using System.Net;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;

namespace Изпит
{
    [TestFixture]
    public class Tests
    {
        private RestClient client = null!;
        private static string createdMovieId = string.Empty;

        // Смени с твоите валидни login данни
        private const string Email = "user1234@gmail.com";
        private const string Password = "password123456";

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            // 1) Login client (без auth)
            var loginClient = new RestClient("http://144.91.123.158:5000");

            var loginRequest = new RestRequest("/api/User/Authentication", Method.Post);
            loginRequest.AddJsonBody(new
            {
                email = Email,
                password = Password
            });

            var loginResponse = await loginClient.ExecuteAsync<LoginResponseDto>(loginRequest);

            Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(loginResponse.Data, Is.Not.Null);
            Assert.That(loginResponse.Data!.AccessToken, Is.Not.Null.And.Not.Empty);

            // 2) Main client (с Bearer token)
            var options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(loginResponse.Data.AccessToken)
            };

            client = new RestClient(options);
        }

        [Test, Order(1)]
        public async Task CreateMovie_ShouldReturnOk_AndMovieObject()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "My test movie",
                description = "Created for API test",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });

            var response = await client.ExecuteAsync<ApiResponseDto>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Movie, Is.Not.Null);
            Assert.That(response.Data.Movie!.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(response.Data.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = response.Data.Movie.Id;
        }

        [Test, Order(2)]
        public async Task EditCreatedMovie_ShouldReturnOk_AndSuccessMessage()
        {
            Assert.That(createdMovieId, Is.Not.Empty);

            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);
            request.AddJsonBody(new
            {
                title = "Edited movie title",
                description = "Edited movie description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });

            var response = await client.ExecuteAsync<ApiResponseDto>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public async Task GetAllMovies_ShouldReturnOk_AndNonEmptyArray()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);
            var response = await client.ExecuteAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

            using var doc = JsonDocument.Parse(response.Content!);
            Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(doc.RootElement.GetArrayLength(), Is.GreaterThan(0));
        }

        [Test, Order(4)]
        public async Task DeleteCreatedMovie_ShouldReturnOk_AndSuccessMessage()
        {
            Assert.That(createdMovieId, Is.Not.Empty);

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = await client.ExecuteAsync<ApiResponseDto>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public async Task CreateMovieWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "",
                description = ""
            });

            var response = await client.ExecuteAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public async Task EditNonExistingMovie_ShouldReturnBadRequest_AndExpectedMessage()
        {
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", Guid.NewGuid().ToString());
            request.AddJsonBody(new
            {
                title = "Edited movie title",
                description = "Edited movie description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });

            var response = await client.ExecuteAsync<ApiResponseDto>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public async Task DeleteNonExistingMovie_ShouldReturnBadRequest_AndExpectedMessage()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", Guid.NewGuid().ToString());

            var response = await client.ExecuteAsync<ApiResponseDto>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }
    }
}