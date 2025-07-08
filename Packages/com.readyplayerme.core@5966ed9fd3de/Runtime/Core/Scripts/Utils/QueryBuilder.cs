using System.Text;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// Represents a builder for constructing query strings for URLs.
    /// </summary>
    public class QueryBuilder
    {
        private const string PARAMETER_SEPARATOR = "&";
        private readonly StringBuilder query = new StringBuilder();
        public string Query => query.ToString();

        public QueryBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder"/> class with an initial key-value pair.
        /// </summary>
        /// <param name="keyName">The key of the initial query parameter.</param>
        /// <param name="value">The value of the initial query parameter.</param>
        public QueryBuilder(string keyName, string value)
        {
            AddKeyValue(keyName, value);
        }

        /// <summary>
        /// Adds a key-value pair to the query string.
        /// </summary>
        /// <param name="keyName">The key to add.</param>
        /// <param name="value">The value associated with the key.</param>
        public void AddKeyValue(string keyName, string value)
        {
            AddKey(keyName);
            AddValue(value);
        }

        /// <summary>
        /// Adds a key to the query string. It conditionally prepends a parameter separator if necessary.
        /// </summary>
        /// <param name="keyName">The key to add.</param>
        private void AddKey(string keyName)
        {
            //if first key does not add parameter separator
            var separator = Query.Length > 1 ? PARAMETER_SEPARATOR : "";
            AppendQuery($"{separator}{keyName}=");
        }

        /// <summary>
        /// Adds a value to the query string immediately following its key.
        /// </summary>
        /// <param name="value">The value to add.</param>
        private void AddValue(string value)
        {
            AppendQuery(value);
        }

        /// <summary>
        /// Appends a piece of text to the query string.
        /// </summary>
        /// <param name="text">The text to append.</param>
        private void AppendQuery(string text)
        {
            query.Append(text);
        }
    }
}
