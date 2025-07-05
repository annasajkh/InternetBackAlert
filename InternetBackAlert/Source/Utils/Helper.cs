using System.Globalization;
using System.Net.NetworkInformation;

namespace InternetBackAlert.Source.Utils;


internal static class Helper
{

    internal static bool IsConnectedToTheInternet()
    {
        try
        {
            Ping ping = new Ping();

            string url = CultureInfo.InstalledUICulture switch
            {
                { Name: var name } when name.StartsWith("fa") =>
                    "aparat.com",
                { Name: var name } when name.StartsWith("zh") =>
                    "baidu.com",
                _ =>
                    "google.com",
            };

            PingReply reply = ping.Send(url, timeout: 1000);

            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
