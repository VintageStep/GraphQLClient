using NUnit.Framework;
using NSubstitute;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;

namespace GraphQL.Unity.Tests
{
    [TestFixture]
    public class GraphQLIntegrationTests
    {
        private GameObject testGameObject;
        private GraphQLClientBehaviour clientBehaviour;
        private IWebRequestFactory mockWebRequestFactory;
        private IWebRequest mockWebRequest;

        [SetUp]
        public void Setup()
        {
            mockWebRequestFactory = Substitute.For<IWebRequestFactory>();
            mockWebRequest = Substitute.For<IWebRequest>();
            mockWebRequestFactory.CreateWebRequest(Arg.Any<string>(), Arg.Any<string>()).Returns(mockWebRequest);

            testGameObject = new GameObject();
            clientBehaviour = testGameObject.AddComponent<GraphQLClientBehaviour>();
            clientBehaviour.SetWebRequestFactory(mockWebRequestFactory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Application.isPlaying)
            {
                Object.Destroy(testGameObject);
            }
            else
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [UnityTest]
        public IEnumerator SendQuery_WithQueryBuilder_ReturnsExpectedResult()
        {
            // Arrange
            var query = new QueryBuilder()
                .Operation("GetHero")
                .BeginObject("hero")
                    .Field("name")
                    .Field("age")
                .EndObject()
                .Build();

            string expectedResponse = "{\"data\":{\"hero\":{\"name\":\"Luke Skywalker\",\"age\":23}}}";
            mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(expectedResponse));

            bool testComplete = false;

            // Act
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                // Assert
                Assert.IsNull(error);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("Luke Skywalker", response.Data.Hero.Name);
                Assert.AreEqual(23, response.Data.Hero.Age);
                testComplete = true;
            });

            // Wait for the async operation to complete
            yield return new WaitUntil(() => testComplete);
        }

        [UnityTest]
        public IEnumerator SendMutation_ReturnsExpectedResult()
        {
            // Arrange
            var mutation = new QueryBuilder()
                .Operation("CreateHero", "mutation")
                .BeginObject("createHero")
                    .Field("name")
                    .Field("id")
                .EndObject()
                .Build();

            string expectedResponse = "{\"data\":{\"createHero\":{\"name\":\"Han Solo\",\"id\":\"1234\"}}}";
            mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(expectedResponse));

            bool testComplete = false;

            // Act
            clientBehaviour.SendQuery<CreateHeroResponse>(mutation, (response, error) =>
            {
                // Assert
                Assert.IsNull(error);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("Han Solo", response.Data.CreateHero.Name);
                Assert.AreEqual("1234", response.Data.CreateHero.Id);
                testComplete = true;
            });

            // Wait for the async operation to complete
            yield return new WaitUntil(() => testComplete);
        }

        [UnityTest]
        public IEnumerator SendQuery_WithError_ReturnsErrorMessage()
        {
            // Arrange
            var query = new QueryBuilder()
                .Operation("GetHero")
                .BeginObject("hero")
                    .Field("name")
                .EndObject()
                .Build();

            string errorResponse = "{\"errors\":[{\"message\":\"Hero not found\"}]}";
            mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(errorResponse));

            bool testComplete = false;

            // Expect both error logs
            LogAssert.Expect(LogType.Error, "GraphQL error: GraphQL errors: Hero not found");
            LogAssert.Expect(LogType.Error, "- Hero not found");

            // Act
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                // Assert
                Assert.IsNull(response);
                Assert.IsNotNull(error);
                Assert.IsTrue(error is GraphQLException);
                Assert.AreEqual("GraphQL errors: Hero not found", error.Message);
                testComplete = true;
            });

            // Wait for the async operation to complete
            yield return new WaitUntil(() => testComplete);
        }

        [UnityTest]
        public IEnumerator SendQuery_WithVariables_ReturnsExpectedResult()
        {
            // Arrange
            var query = new QueryBuilder()
                .Operation("GetHero")
                .Variable("id", "1234", "ID!")
                .BeginObject("hero(id: $id)")
                    .Field("name")
                    .Field("age")
                .EndObject()
                .Build();

            string expectedResponse = "{\"data\":{\"hero\":{\"name\":\"Luke Skywalker\",\"age\":23}}}";
            mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(expectedResponse));

            bool testComplete = false;

            // Act
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                // Assert
                Assert.IsNull(error);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("Luke Skywalker", response.Data.Hero.Name);
                Assert.AreEqual(23, response.Data.Hero.Age);
                testComplete = true;
            });

            // Wait for the async operation to complete
            yield return new WaitUntil(() => testComplete);
        }

        [UnityTest]
        public IEnumerator SendQuery_WithCaching_ReturnsCachedResult()
        {
            // Arrange
            var query = new QueryBuilder()
                .Operation("GetHero")
                .BeginObject("hero")
                    .Field("name")
                    .Field("age")
                .EndObject()
                .Build();

            query.UseCache = true;

            string expectedResponse = "{\"data\":{\"hero\":{\"name\":\"Luke Skywalker\",\"age\":23}}}";
            mockWebRequest.SendWebRequestAsync().Returns(Task.FromResult(expectedResponse));

            bool firstQueryComplete = false;
            bool secondQueryComplete = false;

            // Act
            // First query
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                Assert.IsNull(error);
                Assert.IsNotNull(response);
                firstQueryComplete = true;
            });

            yield return new WaitUntil(() => firstQueryComplete);

            // Second query (should use cache)
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                // Assert
                Assert.IsNull(error);
                Assert.IsNotNull(response);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("Luke Skywalker", response.Data.Hero.Name);
                Assert.AreEqual(23, response.Data.Hero.Age);
                secondQueryComplete = true;
            });

            yield return new WaitUntil(() => secondQueryComplete);

            // Verify that SendWebRequestAsync was called only once
            mockWebRequest.Received(1).SendWebRequestAsync();
        }

        private class CreateHeroResponse
        {
            public Hero CreateHero { get; set; }
        }

        private class Hero
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private class HeroResponse
        {
            public Hero Hero { get; set; }
        }
    }
}
