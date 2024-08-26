using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GraphQL.Unity.Tests
{
    public class GraphQLPerformanceTests
    {
        private GraphQLClientBehaviour clientBehaviour;
        private GameObject testGameObject;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject();
            clientBehaviour = testGameObject.AddComponent<GraphQLClientBehaviour>();
            // Set up a mock web request factory that simulates a successful response
            var mockFactory = new MockWebRequestFactory();
            clientBehaviour.SetWebRequestFactory(mockFactory);
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
        public IEnumerator MeasureQueryExecutionTime()
        {
            LogAssert.ignoreFailingMessages = true;

            var query = new QueryBuilder()
                .Operation("GetHero")
                .BeginObject("hero")
                    .Field("name")
                    .Field("age")
                .EndObject()
                .Build();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool queryComplete = false;
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                queryComplete = true;
                if (error != null)
                {
                    UnityEngine.Debug.LogError($"Query failed: {error.Message}");
                }
            });

            yield return new WaitUntil(() => queryComplete);

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Query execution time: {stopwatch.ElapsedMilliseconds}ms");

            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, "Query execution took longer than 1 second");
        }

        [UnityTest]
        public IEnumerator MeasureMemoryUsage()
        {
            LogAssert.ignoreFailingMessages = true;

            var query = new QueryBuilder()
                .Operation("GetHero")
                .BeginObject("hero")
                    .Field("name")
                    .Field("age")
                .EndObject()
                .Build();

            long memoryBefore = System.GC.GetTotalMemory(true);

            bool queryComplete = false;
            clientBehaviour.SendQuery<HeroResponse>(query, (response, error) =>
            {
                queryComplete = true;
                if (error != null)
                {
                    UnityEngine.Debug.LogError($"Query failed: {error.Message}");
                }
            });

            yield return new WaitUntil(() => queryComplete);

            long memoryAfter = System.GC.GetTotalMemory(true);
            long memoryUsed = memoryAfter - memoryBefore;

            UnityEngine.Debug.Log($"Memory before: {memoryBefore} bytes");
            UnityEngine.Debug.Log($"Memory after: {memoryAfter} bytes");
            UnityEngine.Debug.Log($"Memory used: {memoryUsed} bytes");


            Assert.Less(memoryUsed, 10240 * 10240, "Query used more than 1MB of memory");
        }

        private class HeroResponse
        {
            public Hero Hero { get; set; }
        }

        private class Hero
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }

    // Mock classes for testing
    public class MockWebRequestFactory : IWebRequestFactory
    {
        public IWebRequest CreateWebRequest(string url, string method)
        {
            return new MockWebRequest();
        }
    }

    public class MockWebRequest : IWebRequest
    {
        public void SetRequestHeader(string name, string value) { }

        public void SetRequestBody(string body) { }

        public Task<string> SendWebRequestAsync()
        {
            // Simulate a successful response
            return Task.FromResult("{\"data\":{\"hero\":{\"name\":\"Luke Skywalker\",\"age\":23}}}");
        }
    }
}