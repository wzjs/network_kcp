using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public class UDPTransporter : ITransporter
    {
        private readonly Socket socket;

        public UDPTransporter(EndPoint ipEndPoint) 
        { 
            socket = new Socket(ipEndPoint.AddressFamily,SocketType.Dgram, ProtocolType.Udp);
            
            try
            {
                this.socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }
        }

        public int Available()
        {
            return socket.Available;
        }

        public void Dispose()
        {
            socket.Dispose();
        }

        public int Recv(byte[] data,ref EndPoint endPoint)
        {
            return socket.ReceiveFrom(data, ref endPoint);
        }

        public void Send(byte[] data, int index, int length, EndPoint endPoint)
        {
            socket.SendTo(data, index, length, SocketFlags.None, endPoint);
        }
    }
}
