using System;
using System.Collections.Generic;
using Blackhole.RestApi.Contract;
using NUnit.Framework;

namespace Blackhole.Development.Tests
{
    public sealed class RestRequestTests
    {
        [TestCase("items", "items?name=a%20b%26c#details")]
        [TestCase("items?sort=id", "items?sort=id&name=a%20b%26c#details")]
        [TestCase("items?", "items?name=a%20b%26c#details")]
        [TestCase("items?sort=id&", "items?sort=id&name=a%20b%26c#details")]
        public void QueryParametersPreserveExistingQueryAndFragment(string endpoint, string expected)
        {
            var query = new Dictionary<string, string> { ["name"] = "a b&c", ["ignored"] = null };
            RestRequest request = RestRequest.Get(endpoint + "#details", queryParams: query);
            Assert.That(request.FullUrl, Is.EqualTo(expected));
        }

        [Test]
        public void EmptyQueryLeavesEndpointAndHeadersIntact()
        {
            var headers = new Dictionary<string, string> { ["Accept"] = "application/json" };
            RestRequest request = RestRequest.Get("items#details", headers, new Dictionary<string, string>());
            Assert.That(request.FullUrl, Is.EqualTo("items#details"));
            Assert.That(request.Headers["Accept"], Is.EqualTo("application/json"));
            Assert.Throws<ArgumentException>(() => RestRequest.Get("items", queryParams: new Dictionary<string, string> { [" "] = "value" }));
        }
    }
}
