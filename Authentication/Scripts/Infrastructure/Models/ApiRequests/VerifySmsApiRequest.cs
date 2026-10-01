namespace Blackhole.Authentication.Infrastructure
{
    internal readonly struct VerifySmsApiRequest
    {
        internal readonly string VerifyCode;
        internal readonly string MobileNumber;
        internal readonly string IdentityNo;
        internal readonly string CountryId;

        internal VerifySmsApiRequest(string verifyCode, string mobileNumber, string identityNo = "712574", string countryId = "411")
        {
            VerifyCode = verifyCode;
            MobileNumber = mobileNumber;
            IdentityNo = identityNo;
            CountryId = countryId;
        }
    }
}