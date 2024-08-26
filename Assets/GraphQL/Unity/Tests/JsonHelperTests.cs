using NUnit.Framework;
using GraphQL.Unity;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GraphQL.Unity.Tests
{
    [TestFixture]
    public class JsonHelperTests
    {
        [Test]
        public void Serialize_PrimitiveTypes_ReturnsCorrectJson()
        {
            Assert.AreEqual("42", JsonHelper.Serialize(42));
            Assert.AreEqual("3.14", JsonHelper.Serialize(3.14));
            Assert.AreEqual("\"test\"", JsonHelper.Serialize("test"));
            Assert.AreEqual("true", JsonHelper.Serialize(true));
            Assert.AreEqual("false", JsonHelper.Serialize(false));
        }

        [Test]
        public void Serialize_ObjectType_ReturnsCorrectJson()
        {
            var testObject = new { Name = "John", Age = 30 };
            string json = JsonHelper.Serialize(testObject);
            Assert.AreEqual("{\"name\":\"John\",\"age\":30}", json);
        }

        [Test]
        public void Serialize_ArrayType_ReturnsCorrectJson()
        {
            int[] array = { 1, 2, 3 };
            string json = JsonHelper.Serialize(array);
            Assert.AreEqual("[1,2,3]", json);
        }

        [Test]
        public void Deserialize_PrimitiveTypes_ReturnsCorrectValue()
        {
            Assert.AreEqual(42, JsonHelper.Deserialize<int>("42"));
            Assert.AreEqual(3.14, JsonHelper.Deserialize<double>("3.14"));
            Assert.AreEqual("test", JsonHelper.Deserialize<string>("\"test\""));
            Assert.IsTrue(JsonHelper.Deserialize<bool>("true"));
            Assert.IsFalse(JsonHelper.Deserialize<bool>("false"));
        }

        [Test]
        public void Deserialize_ObjectType_ReturnsCorrectObject()
        {
            string json = "{\"name\":\"John\",\"age\":30}";
            var result = JsonHelper.Deserialize<TestObject>(json);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual(30, result.Age);
        }

        [Test]
        public void Deserialize_ArrayType_ReturnsCorrectArray()
        {
            string json = "[1,2,3]";
            int[] result = JsonHelper.Deserialize<int[]>(json);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, result);
        }

        [Test]
        public void Deserialize_InvalidJson_ThrowsJsonException()
        {
            try
            {
                JsonHelper.Deserialize<TestObject>("invalid json");
                Assert.Fail("Expected JsonException was not thrown.");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Assert.IsInstanceOf<Newtonsoft.Json.JsonReaderException>(ex);
                StringAssert.Contains("Unexpected character encountered while parsing value: i", ex.Message);
            }
        }

        [Test]
        public void Deserialize_IncompleteJson_ThrowsJsonException()
        {
            try
            {
                JsonHelper.Deserialize<TestObject>("{\"name\":\"John\"");
                Assert.Fail("Expected JsonException was not thrown.");
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Assert.IsInstanceOf<Newtonsoft.Json.JsonException>(ex);
                StringAssert.Contains("Unexpected end", ex.Message);
            }
        }

        [Test]
        public void Deserialize_MismatchedJsonStructure_LenientBehavior()
        {
            var result = JsonHelper.Deserialize<TestObject>("{\"wrongField\": \"value\"}");

            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.AreEqual(0, result.Age);
        }

        [Test]
        public void Deserialize_MismatchedJsonStructure_StrictBehavior()
        {
            Assert.Throws<JsonSerializationException>(() =>
                JsonHelper.Deserialize<TestObject>("{\"wrongField\": \"value\"}", true)
            );
        }

        private class TestObject
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}