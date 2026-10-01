using Blackhole.Settings.Contract;
using UnityEngine;

namespace Blackhole.Authentication.Infrastructure
{
    [ProjectSettings("Networking", 0)]
    [CreateAssetMenu(fileName = "AuthenticationSettingSO", menuName = "Blackhole/Modules/Authentication/Settings")]
    public class AuthenticationSettingSO : ScriptableObject
    {
        [SerializeField] private string _baseUrl = "https://api.example.com";

        public string BaseUrl => _baseUrl;
    }
}
