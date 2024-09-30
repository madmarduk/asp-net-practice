using AspNetPractice;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace AspNetPractice.Tests
{
    [TestFixture]
    public class StringProcessorControllerTests : WebApplicationFactory<Program>
    {
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = CreateClient();
        }

        [Test]
        public async Task ProcessString_ReturnsCorrectString()
        {
            // Test unit for task1
            // Arrange
            var requestUrl = "/api/StringProcessor?input=abcdef&sortAlgorithm=quick";

            // Act
            var response = await _client.GetAsync(requestUrl);

            // Assert
            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString.Contains("cbafed"), Is.True);
        }

        [Test]
        public async Task ProcessString_ReturnsBadRequest_WhenInputIsInvalid()
        {
            // Test unit for task2 (for alphabet check)
            // Arrange
            var requestUrl = "/api/StringProcessor?input=avDD&sortAlgorithm=quick";

            // Act
            var response = await _client.GetAsync(requestUrl);

            // Assert
            Assert.That((int)response.StatusCode, Is.EqualTo(400));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString.Contains("Input string should only contain lower case latin letters"), Is.True);
        }
        
       

        [Test]
        public async Task ProcessString_ReturnsSortedResult()
        {
            // Arrange
            var requestUrl = "/api/StringProcessor?input=dcba&sortAlgorithm=quick";

            // Act
            var response = await _client.GetAsync(requestUrl);

            // Assert
            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.That(responseString.Contains("abcd"), Is.True);     
        }
    }
}
