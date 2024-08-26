using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using System;

namespace GraphQL.Unity.Tests
{
    [TestFixture]
    public class GraphQLUnityClientTests
    {
        private GraphQLUnityClient _client;
        private IWebRequestFactory _mockWebRequestFactory;
        private IWebRequest _mockWebRequest;

        [SetUp]
        public void Setup()
        {
            _mockWebRequestFactory = Substitute.For<IWebRequestFactory>();
            _mockWebRequest = Substitute.For<IWebRequest>();
            _mockWebRequestFactory.CreateWebRequest(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockWebRequest);
            _client = new GraphQLUnityClient("https://api.example.com/graphql", null, _mockWebRequestFactory);
        }

        [UnityTest]
        public IEnumerator SendQueryAsync_SuccessfulQuery_ReturnsCorrectResponse()
        {
            // Arrange
            var query = "query { hero { name } }";
            var request = new GraphQLRequest(query, operationName: null, variables: null, useCache: false);
            var expectedResponse = "{\"data\":{\"hero\":{\"name\":\"Luke Skywalker\"}}}";
            _mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            Task<GraphQLResponse<TestResponse>> responseTask = null;
            yield return RunAsync(() =>
            {
                responseTask = _client.SendQueryAsync<TestResponse>(request);
                return responseTask;
            });

            // Assert
            Assert.IsNotNull(responseTask);
            Assert.IsTrue(responseTask.IsCompleted);
            var response = responseTask.Result;
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Hero);
            Assert.AreEqual("Luke Skywalker", response.Data.Hero.Name);

            // Verify that the web request was created and sent
            _mockWebRequestFactory.Received(1).CreateWebRequest(Arg.Any<string>(), Arg.Is<string>(m => m == "POST"));
            _mockWebRequest.Received(1).SendWebRequestAsync();
        }

        [UnityTest]
        public IEnumerator SendQueryAsync_WithGraphQLError_ThrowsGraphQLException()
        {
            // Arrange
            var query = "query { hero { name } }";
            var request = new GraphQLRequest(query, operationName: null, variables: null, useCache: false);
            var errorResponse = "{\"errors\":[{\"message\":\"Field 'hero' doesn't exist on type 'Query'\"}]}";
            _mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(errorResponse));

            // Act
            GraphQLException caughtException = null;
            yield return RunAsync(async () =>
            {
                try
                {
                    await _client.SendQueryAsync<TestResponse>(request);
                }
                catch (GraphQLException ex)
                {
                    caughtException = ex;
                }
            });

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsTrue(caughtException.Message.Contains("Field 'hero' doesn't exist on type 'Query'"));

            // Verify that the web request was created and sent
            _mockWebRequestFactory.Received(1).CreateWebRequest(Arg.Any<string>(), Arg.Is<string>(m => m == "POST"));
            _mockWebRequest.Received(1).SendWebRequestAsync();
        }

        [UnityTest]
        public IEnumerator SendQueryAsync_WithNetworkError_ThrowsGraphQLException()
        {
            // Arrange
            var query = "query { hero { name } }";
            var request = new GraphQLRequest(query, operationName: null, variables: null, useCache: false);
            _mockWebRequest.SendWebRequestAsync().Returns(Task.FromException<string>(new System.Exception("Network error")));

            // Act
            GraphQLException caughtException = null;
            yield return RunAsync(async () =>
            {
                try
                {
                    await _client.SendQueryAsync<TestResponse>(request);
                }
                catch (GraphQLException ex)
                {
                    caughtException = ex;
                }
            });

            // Assert
            Assert.IsNotNull(caughtException);
            Assert.IsTrue(caughtException.Message.Contains("Network error"));

            // Verify that the web request was created and sent
            _mockWebRequestFactory.Received(1).CreateWebRequest(Arg.Any<string>(), Arg.Is<string>(m => m == "POST"));
            _mockWebRequest.Received(1).SendWebRequestAsync();
        }

        private class TestResponse
        {
            public Hero Hero { get; set; }
        }

        private class Hero
        {
            public string Name { get; set; }
        }

        // Helper method to run async tasks in coroutines
        private static IEnumerator RunAsync(Func<Task> action)
        {
            var task = action();
            while (!task.IsCompleted)
            {
                yield return null;
            }
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
}