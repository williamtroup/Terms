using System.Net.NetworkInformation;

namespace Terms.Tools.Extensions
{
    public static class PingExtensions
    {
        public static PingReply ToResult(this Ping ping, string address)
        {
            PingReply pingReply = null;

            try
            {
                pingReply = ping.Send(address);
            }
            catch
            {
                // TODO: Logging will need to be added.
            }

            return pingReply;
        }
    }
}