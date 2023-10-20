using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public class TCPTransporter : IKCP
    {
        public int Available()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int Recv(byte[] data, ref EndPoint endPoint)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, int index, int length, EndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
}
