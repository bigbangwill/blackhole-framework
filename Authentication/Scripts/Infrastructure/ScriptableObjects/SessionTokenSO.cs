using UnityEngine;

namespace Blackhole.Authentication.Infrastructure
{
    [CreateAssetMenu(fileName = "SessionTokenSO", menuName = "Blackhole/Modules/Authentication/Session Token")]
    public class SessionTokenSO : ScriptableObject
    {
        public string AccessToken;
        public string RefreshToken;
        public int ExpiresIn;
        public string TokenType;
    }
}
