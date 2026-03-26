using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.Helpers
{
    public class commonHelpers
    {
        //  Ping 方法，支持检测IP和端口
        public static bool Ping(string ip, string port)
        {
            try
            {
                int portNum;
                if (!int.TryParse(port, out portNum))
                    return false;

                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var result = client.BeginConnect(ip, portNum, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                    if (!success)
                        return false;
                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
