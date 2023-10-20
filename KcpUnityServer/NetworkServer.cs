using KcpUnityDemo;
using KcpUnityDemo.Protocol;
using KcpUnityDemo.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityServer
{
    public class NetworkServer
    {
        private static EndPoint localPoint = new IPEndPoint(IPAddress.Any, 5001);
        private KCPService service;
        private MessageHandle messageHandle;
        public NetworkServer()
        {
            messageHandle = new MessageHandle(this);
            StartServer();
        }

        public void StartServer()
        {
            service = new KCPService(localPoint);
            service.ReadCallback = (id, data) =>
            {
                var recv = Recv(data);
                messageHandle.ProcessHandle(recv, id);
                
            };
            Task.Run(async () => {
                while (true)
                {
                    await Task.Delay(10);
                    service.Update();
                }
            });
            Console.ReadLine();
        }

        public void Send<T>(T data,long channelId) where T : IMessage
        {
            byte[] buffer = MessageSerializeHelper.Serialize(data);
            service.Send(channelId, buffer);
        }

        public object Recv(byte[] data)
        {
            var obj = MessageSerializeHelper.Deserialize(data);
            return obj;
        }
    }
}
