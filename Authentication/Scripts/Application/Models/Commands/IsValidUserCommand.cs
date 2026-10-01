using System.Collections.Generic;

namespace Blackhole.Authentication.Application
{
    public class IsValidUserCommand
    {
        public KeyValuePair<string, string> AccessTokenHeader { get; }

        public IsValidUserCommand(KeyValuePair<string, string> accessTokenHeader)
        {
            AccessTokenHeader = accessTokenHeader;
        }
    }
}