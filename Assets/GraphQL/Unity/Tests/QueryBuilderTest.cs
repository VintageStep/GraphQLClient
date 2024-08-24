using NUnit.Framework;
using UnityEngine;
using System;

namespace GraphQL.Unity.Tests
{
    public class QueryBuilderTests
    {
        [Test]
        public void Operation_SetsOperationNameCorrectly()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act
            var result = queryBuilder.Operation("TestOperation").Build();

            // Assert
            StringAssert.StartsWith("query TestOperation", result.query);
        }

        [Test]
        public void Variable_AddsVariableCorrectly()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act
            var result = queryBuilder
                .Operation("TestOperation")
                .Variable("testVar", "testValue", "String!")
                .BeginObject("testObject")
                .EndObject()
                .Build();

            // Assert
            StringAssert.Contains("($testVar: String!)", result.query);
            Assert.IsTrue(result.variables.ContainsKey("testVar"));
            Assert.AreEqual("testValue", result.variables["testVar"]);

            // Debug
            TestContext.WriteLine($"Generated Query:\n{result.query}");
            TestContext.WriteLine($"Variables: {JsonHelper.Serialize(result.variables)}");
        }

        [Test]
        public void BeginObject_AddsObjectCorrectly()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act
            var result = queryBuilder
                .Operation("TestOperation")
                .BeginObject("testObject")
                .Field("testField")
                .EndObject()
                .Build();

            // Assert
            StringAssert.Contains("testObject {", result.query);
            StringAssert.Contains("testField", result.query);
        }

        [Test] // IF indentation is changed in expected query, it'll throw an error
        public void Build_GeneratesCorrectQuery()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act
            var result = queryBuilder
                .Operation("TestOperation")
                .Variable("testVar", "testValue", "String!")
                .BeginObject("testObject")
                .Field("testField1")
                .Field("testField2")
                .EndObject()
                .Build();

            // Assert
            var expectedQuery = @"query TestOperation($testVar: String!) {
testObject {
  testField1
  testField2
}
}".Replace("\r\n", "\n").Trim();

            Assert.AreEqual(expectedQuery, result.query.Replace("\r\n", "\n").Trim());
            Assert.IsTrue(result.variables.ContainsKey("testVar"));
            Assert.AreEqual("testValue", result.variables["testVar"]);
        }

        [Test]
        public void Build_ThrowsExceptionForVariablesWithoutObject()
        {
            // Arrange
            var queryBuilder = new QueryBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                queryBuilder
                    .Operation("TestOperation")
                    .Variable("testVar", "testValue", "String!")
                    .Build();
            });
        }
    }
}
