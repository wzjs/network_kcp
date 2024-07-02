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
        private TCPService service;
        private MessageHandle messageHandle;
        public NetworkServer()
        {
            messageHandle = new MessageHandle(this);
            StartServer();
        }

        public void StartServer()
        {
            service = new TCPService(true);
            service.ConnectCallBack = OnConnect;
            service.Accept();
            Task.Run(async () => {
                while (true)
                {
                    await Task.Delay(10);
                    service.Update();
                }
            });
            Console.ReadLine();
        }

        private void OnConnect(long id)
        {
            Debug.Log("Receive one connect id:" + id);
            service.Receive(id, (b, l, e) =>
                {
                    OnReceive(b, l, e, id);
                }
            );
        }

        public void Send<T>(T data,long channelId) where T : IMessage
        {
            byte[] buffer = MessageSerializeHelper.Serialize(data);
            service.Send(channelId, (writer) =>
            {
                writer.Write(buffer);
            });
        }

        private void OnReceive(BinaryReader binaryReader, int length, bool isEncrypt,long channelId)
        {
            if (isEncrypt)
            {
            }
            else
            {
                byte[] data = binaryReader.ReadBytes(length);
                var message = MessageSerializeHelper.Deserialize(data);
                messageHandle.ProcessHandle(message, channelId);
            }
        }
    }
}
