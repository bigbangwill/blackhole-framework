using System;
using System.Collections.Generic;

namespace Blackhole.RestApi.Contract
{
    public sealed class RestRequest
    {
        private string _baseUrl;
        private readonly string _endpoint;

        public RestMethod Method { get; }

        public Dictionary<string, string> Headers { get; }

        public string JsonBody { get; }

        public bool HasBody => !string.IsNullOrWhiteSpace(JsonBody);

        public string FullUrl => _baseUrl + _endpoint;

        public RestRequest(string endpoint, RestMethod method, IDictionary<string, string> headers = null, string jsonBody = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("The request endpoint cannot be null or empty.", nameof(endpoint));

            _endpoint = endpoint;
            Method = method;
            JsonBody = jsonBody;

            var copiedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (string.IsNullOrWhiteSpace(header.Key))
                        throw new ArgumentException("A request header name cannot be null or empty.", nameof(headers));

                    if (header.Value == null)
                        throw new ArgumentException($"The value of header '{header.Key}' cannot be null.", nameof(headers));

                    copiedHeaders.Add(header.Key, header.Value);
                }
            }
            Headers = new Dictionary<string, string>(copiedHeaders);
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("The base URL cannot be null or empty.", nameof(baseUrl));

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri parsedBaseUri))
                throw new ArgumentException("The base URL must be an absolute URL.", nameof(baseUrl));

            _baseUrl = parsedBaseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? parsedBaseUri.AbsoluteUri : parsedBaseUri.AbsoluteUri + "/";
        }

        public static RestRequest Get(string endpoint, IDictionary<string, string> headers = null)
        {
            return new RestRequest(endpoint, RestMethod.Get, headers);
        }

        public static RestRequest Post(string endpoint, string jsonBody = null, IDictionary<string, string> headers = null)
        {
            return new RestRequest(endpoint, RestMethod.Post, headers, jsonBody);
        }

        public static RestRequest Put(string endpoint, string jsonBody, IDictionary<string, string> headers = null)
        {
            return new RestRequest(endpoint, RestMethod.Put, headers, jsonBody);
        }

        public static RestRequest Patch(string endpoint, string jsonBody, IDictionary<string, string> headers = null)
        {
            return new RestRequest(endpoint, RestMethod.Patch, headers, jsonBody);
        }

        public static RestRequest Delete(string endpoint, IDictionary<string, string> headers = null)
        {
            return new RestRequest(endpoint, RestMethod.Delete, headers);
        }
    }
}