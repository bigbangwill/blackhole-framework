namespace Blackhole.Authentication.Contract
{
    public class VerifySmsRequestDto
    {
        public string MobileNumber { get; set; }
        public string VerifyCode { get; set; }
    }
}