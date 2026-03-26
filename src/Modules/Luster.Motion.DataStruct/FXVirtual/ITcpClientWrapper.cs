using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.FXVirtual
{
    // 创建可MOCK的接口和包装类
    public interface ITcpClientWrapper
    {
        bool Connected { get; }
        Stream GetStream();
        void Connect(string host, int port);
        void Close();
    }

    public class TcpClientWrapper : ITcpClientWrapper
    {
        private readonly TcpClient _tcpClient;

        public TcpClientWrapper()
        {
            _tcpClient = new TcpClient();
        }

        public bool Connected => _tcpClient.Connected;

        public Stream GetStream() => _tcpClient.GetStream();

        public void Connect(string host, int port) => _tcpClient.Connect(host, port);

        public void Close() => _tcpClient.Close();
    }
}
