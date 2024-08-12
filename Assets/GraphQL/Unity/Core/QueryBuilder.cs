using System.Text;
using System.Collections.Generic;

namespace GraphQL.Unity.Core
{
    /// <summary>
    /// Provides a fluent interface for building GraphQL queries.
    /// </summary>
    public class QueryBuilder
    {
        private StringBuilder _query = new StringBuilder();
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private List<string> _variableDefinitions = new List<string>();
        private int _indent = 0;
        private bool _isFirstField = true;

        /// <summary>
        /// Starts a new GraphQL operation (query, mutation, or subscription).
        /// </summary>
        /// <param name="name">The name of the operation.</param>
        /// <param name="type">The type of the operation (default is "query").</param>
        /// <returns>The QueryBuilder instance for method chaining.</returns>
        public QueryBuilder Operation(string name, string type = "query")
        {
            _query.Append($"{type} {name}");
            return this;
        }

        /// <summary>
        /// Adds a variable to the query.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="type">The GraphQL type of the variable.</param>
        /// <returns>The QueryBuilder instance for method chaining.</returns>
        public QueryBuilder Variable(string name, object value, string type)
        {
            _variables[name] = value;
            _variableDefinitions.Add($"${name}: {type}");
            return this;
        }

        /// <summary>
        /// Begins a new object in the query.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>The QueryBuilder instance for method chaining.</returns>
        public QueryBuilder BeginObject(string name)
        {
            if (_indent == 0)
            {
                // This is the root object, add variable definitions if any
                if (_variableDefinitions.Count > 0)
                {
                    _query.Append($"({string.Join(", ", _variableDefinitions)}) ");
                }
                _query.AppendLine("{");
            }
            else
            {
                if (!_isFirstField)
                {
                    _query.AppendLine();
                }
            }
            _query.AppendLine($"{new string(' ', _indent * 2)}{name} {{");
            _indent++;
            _isFirstField = true;
            return this;
        }

        /// <summary>
        /// Adds a field to the current object in the query.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The QueryBuilder instance for method chaining.</returns>
        public QueryBuilder Field(string name)
        {
            if (!_isFirstField)
            {
                _query.AppendLine();
            }
            _query.Append($"{new string(' ', _indent * 2)}{name}");
            _isFirstField = false;
            return this;
        }

        /// <summary>
        /// Ends the current object in the query.
        /// </summary>
        /// <returns>The QueryBuilder instance for method chaining.</returns>
        public QueryBuilder EndObject()
        {
            _indent--;
            _query.AppendLine();
            _query.Append($"{new string(' ', _indent * 2)}}}");
            _isFirstField = false;
            return this;
        }

        /// <summary>
        /// Builds and returns the final GraphQLRequest object.
        /// </summary>
        /// <returns>A GraphQLRequest object representing the built query.</returns>
        public GraphQLRequest Build()
        {
            // Close any remaining open braces
            while (_indent > 0)
            {
                EndObject();
            }
            // Add the final closing brace
            _query.AppendLine();
            _query.Append("}");

            return new GraphQLRequest(_query.ToString().Trim(), variables: _variables);
        }

    }
}