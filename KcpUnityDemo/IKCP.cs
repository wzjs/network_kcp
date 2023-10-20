using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public interface IKCP : IDisposable
    {
        void Send(byte[] data,int index,int length, EndPoint endPoint);
        int Recv(byte[] data,ref EndPoint endPoint);
        int Available();
    }


}
