namespace Blackhole.Authentication.Application
{
    public class VerifySmsCommand
    {
        public string MobileNumber { get; }
        public string VerifyCode { get; }

        public VerifySmsCommand(string mobileNumber, string verifyCode)
        {
            MobileNumber = mobileNumber;
            VerifyCode = verifyCode;
        }
    }
}