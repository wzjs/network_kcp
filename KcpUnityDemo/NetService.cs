using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public abstract class NetService : IDisposable
    {
        public Action<long> ConnectCallBack;
        public Action<long, byte[]> ReadCallback;
        public Action<long, int> ErrorCallback;

        public abstract void Send(long channelId, byte[] data);
        public abstract void Create(long channelId, IPEndPoint remoteEndPoint);
        public abstract void Update();
        public abstract void Dispose();
    }
}
