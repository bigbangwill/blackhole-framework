namespace Blackhole.Authentication.Application
{
    public class SendSmsCommand
    {
        public string MobileNumber { get; }

        public SendSmsCommand(string mobileNumber)
        {
            MobileNumber = mobileNumber;
        }
    }
}