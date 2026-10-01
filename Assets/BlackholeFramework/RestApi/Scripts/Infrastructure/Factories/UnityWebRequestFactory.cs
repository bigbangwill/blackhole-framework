using System;
using System.Text;
using Blackhole.RestApi.Contract;
using UnityEngine.Networking;

namespace Blackhole.RestApi.Infrastructure
{
    public static class UnityWebRequestFactory
    {
        public static UnityWebRequest Create(RestRequest restRequest, string resolvedUrl)
        {
            if (restRequest == null)
                throw new ArgumentNullException(nameof(restRequest));

            if (string.IsNullOrWhiteSpace(resolvedUrl))
                throw new ArgumentException("The resolved URL cannot be null or empty.", nameof(resolvedUrl));

            var webRequest = new UnityWebRequest(resolvedUrl, GetHttpMethod(restRequest.Method));
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            if (restRequest.HasBody)
            {
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(restRequest.JsonBody));
                webRequest.uploadHandler.contentType = "application/json";
            }

            foreach (var header in restRequest.Headers)
                webRequest.SetRequestHeader(header.Key, header.Value);

            return webRequest;
        }

        private static string GetHttpMethod(RestMethod method)
        {
            return method switch
            {
                RestMethod.Get => UnityWebRequest.kHttpVerbGET,
                RestMethod.Post => UnityWebRequest.kHttpVerbPOST,
                RestMethod.Put => UnityWebRequest.kHttpVerbPUT,
                RestMethod.Patch => "PATCH",
                RestMethod.Delete => UnityWebRequest.kHttpVerbDELETE,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The REST method is not supported.")
            };
        }
    }
}