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

        public string FullUrl => GetFullUrl(_baseUrl, _endpoint);

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

        private static string GetFullUrl(string baseUrl, string endpoint)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return endpoint;

            return $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("The base URL cannot be null or empty.", nameof(baseUrl));

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri parsedBaseUri))
                throw new ArgumentException("The base URL must be an absolute URL.", nameof(baseUrl));

            _baseUrl = parsedBaseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? parsedBaseUri.AbsoluteUri : parsedBaseUri.AbsoluteUri + "/";
        }

        public static RestRequest Get(string endpoint, IDictionary<string, string> headers = null, IDictionary<string, string> queryParams = null)
        {
            return new RestRequest(AppendQueryParams(endpoint, queryParams), RestMethod.Get, headers);
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

        private static string AppendQueryParams(string endpoint, IDictionary<string, string> queryParams)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("The request endpoint cannot be null or empty.", nameof(endpoint));

            if (queryParams == null || queryParams.Count == 0)
                return endpoint;

            var parameters = new List<string>();

            foreach (var parameter in queryParams)
            {
                if (string.IsNullOrWhiteSpace(parameter.Key))
                    throw new ArgumentException("A query parameter name cannot be null or empty.", nameof(queryParams));

                if (parameter.Value == null)
                    continue;

                parameters.Add($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
            }

            if (parameters.Count == 0)
                return endpoint;

            int fragmentIndex = endpoint.IndexOf('#');
            string fragment = fragmentIndex >= 0 ? endpoint.Substring(fragmentIndex) : string.Empty;
            string url = fragmentIndex >= 0 ? endpoint.Substring(0, fragmentIndex) : endpoint;

            string separator = url.Contains("?") ? "&" : "?";

            if (url.EndsWith("?", StringComparison.Ordinal) || url.EndsWith("&", StringComparison.Ordinal))
                separator = string.Empty;

            return url + separator + string.Join("&", parameters) + fragment;
        }
    }
}