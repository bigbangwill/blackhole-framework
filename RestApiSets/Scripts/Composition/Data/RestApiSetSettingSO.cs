using System;
using System.Collections.Generic;
using Blackhole.Settings.Contract;
using UnityEngine;
using Blackhole.RestApiSets.Contract;

namespace Blackhole.RestApiSets.Composition
{
    [ProjectSettings("Networking", 10)]
    [CreateAssetMenu(fileName = "RestApiSetSettingSO", menuName = "Blackhole/Modules/REST API Sets/Settings")]
    public class RestApiSetSettingSO : ScriptableObject
    {
        [Serializable]
        public struct UrlInfo
        {
            public RestApiSetServers Server;
            public string Url;
        }

        public List<UrlInfo> Urls = new();
    }
}
