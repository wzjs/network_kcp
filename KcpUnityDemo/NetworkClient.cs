//using KcpUnityDemo.Protocol;
//using KcpUnityDemo.Serialize;
//using MessagePack;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace KcpUnityDemo
//{
//    public class MessageHandle
//    {
//        public ushort Id;
//        public object MsgData;
//        public Action<object> Handle;
//        public Action<int> Error;
//    }
//    public class NetworkClient
//    {
//        private NetService netService;
//        private Dictionary<ushort, MessageHandle> sendMessageHandles;
//        private EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);
//        private uint channelId = 1; //TODO: 需要一定随机性,防止被猜出
//        private bool isConnected = false;
//        public NetworkClient() 
//        {
//            netService = new KCPService(localEndPoint);
//            netService.ReceiveCallback += OnRecive;
//            netService.ConnectCallBack += OnConnect;
//            netService.ErrorCallback += OnError;
//            channelId = (uint)NetworkHelper.IncrementRemoteConv();
//            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 5001);
//            netService.Create(channelId, remoteEndPoint);
//            sendMessageHandles = new Dictionary<ushort, MessageHandle>();

//            Task.Run(async () =>
//            {
//                while (true)
//                {
//                    await Task.Delay(10);
//                    Update();
//                }
//            });

//            Task.Run(async () =>
//            {
//                await Task.Delay(Random.Shared.Next(500, 1000));
//                var random = new Random().Next(0, 2);
//                if (random == 0)
//                {
//                    Send<S2C_Login>(new C2S_Login() { Password = "123456", Account = "admin" }, (obj) =>
//                    {
//                        Debug.Log("error:" + obj.Error);
//                    });
//                }
//                else if (random == 1)
//                {
//                    Send<S2C_TestData>(new C2S_TestData() { Message = "Test Data" }, (obj) =>
//                    {
//                        Debug.Log("error:" + obj.Error + " message:" + obj.Message);
//                    });
//                }
//            });
//        }

//        private void OnError(long id, int error)
//        {
//            Debug.LogError($"Network error, channel id:{id} error:{error}");
//        }



//        private void OnConnect(long id)
//        {
//            if(channelId == id)
//            {
//                isConnected = true;
//            }
//        }

//        private void OnRecive(long id, byte[] buffer)
//        {
//            if(id != channelId)
//            {
//                Debug.LogError($"Network error,client channel id:{id} param id:{id}");
//                return;
//            }
//            var messageId = BitConverter.ToUInt16(buffer, 0);
//            Debug.Log($"Deserialize messageId:" + messageId);
//            if(!sendMessageHandles.ContainsKey(messageId))
//            {
//                Debug.LogError("This shouldn't happen actully, please check the program");
//                return;
//            }
//            var messageHandle = sendMessageHandles[messageId];

//            sendMessageHandles.Remove(messageId);

//            var message = MessageSerializeHelper.Deserialize(buffer);

//            messageHandle.Handle?.Invoke(message);
//        }

//        public void Send<T>(object data,Action<T> onReceive,Action<int> onError = null) where T : IMessage
//        {
//            if (!isConnected)
//            {
//                Debug.LogWarning($"Network connect not finish, localConv:{channelId}");
//                return;
//            }

//            var dataType = data.GetType();
//            if(!typeof(IMessage).IsAssignableFrom(dataType) )
//            {
//                Debug.LogError("NetworkClient::Send ,msg isn't extend from IMessage");
//                return;
//            }

//            var ackId = MessageMapCenter.GetMessageId(typeof(T));
//            if(sendMessageHandles.ContainsKey(ackId))
//            {
//                Debug.LogWarning($"Network already send msg ,type:{data.GetType()}");
//                return;
//            }

//            var messageData = new MessageHandle()
//            {
//                Id = ackId,
//                Error = onError,
//                Handle = (ack)=> { onReceive?.Invoke((T)ack); },
//                MsgData = data
//            };
//            sendMessageHandles.Add(ackId,messageData);
//            var buffer = MessageSerializeHelper.Serialize(data);
//            netService.Send(channelId, buffer);
//        }

//        public void Update()
//        {
//            netService.Update();
//        }
//    }
//}


using KcpUnityDemo.Protocol;
using KcpUnityDemo.Serialize;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public class MessageHandle
    {
        public ushort Id;
        public object MsgData;
        public Action<object> Handle;
        public Action<int> Error;
    }
    public class NetworkClient
    {
        private TCPService netService;
        private Dictionary<ushort, MessageHandle> sendMessageHandles;
        private uint channelId = 1; //TODO: 需要一定随机性,防止被猜出
        private bool isConnected = false;
        public NetworkClient()
        {
            sendMessageHandles = new Dictionary<ushort, MessageHandle>();

            netService = new TCPService(false);
            netService.ReceiveCallback += OnRecive;
            netService.ConnectCallBack += OnConnect;
            netService.ErrorCallback += OnError;
            channelId = (uint)NetworkHelper.IncrementRemoteConv();
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 5001);
            netService.Create(channelId, remoteEndPoint);
            
            Debug.Log("Create Client id:" + channelId);

            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        //await Task.Delay(10);
            //        //Update();

            //        if (Console.ReadKey().Key == ConsoleKey.F)
            //        {
                       
            //        }
            //    }
            //});

            
            
            //Task.Run(async () =>
            //{
            //    await Task.Delay(Random.Shared.Next(500, 1000));
            //    var random = new Random().Next(0, 2);
            //    if (random == 0)
            //    {
            //        Send<S2C_Login>(new C2S_Login() { Password = "123456", Account = "admin" }, (obj) =>
            //        {
            //            Debug.Log("error:" + obj.Error);
            //        });
            //    }
            //    else if (random == 1)
            //    {
            //        Send<S2C_TestData>(new C2S_TestData() { Message = "Test Data" }, (obj) =>
            //        {
            //            Debug.Log("error:" + obj.Error + " message:" + obj.Message);
            //        });
            //    }
            //});
        }

        private void OnError(long id, int error)
        {
            Debug.LogError($"Network error, channel id:{id} error:{error}");
        }



        private void OnConnect(long id)
        {
            if (channelId == id)
            {
                Debug.Log("Connect success to server id:" + id);
                isConnected = true;
                //netService.Receive(id,(b,l,e)=> { OnReceive(b,l,e); });
                netService.Receive(id, OnReceive);

                Send<S2C_Login>(new C2S_Login() { Password = "123456", Account = "admin" }, (obj) =>
                {
                    Debug.Log("error:" + obj.Error);

                });
            }
        }

        private void OnReceive(BinaryReader binaryReader, int length,bool isEncrypt)
        {
            if (isEncrypt)
            {
            }
            else
            {
                ushort msgId = binaryReader.ReadUInt16();
                Debug.Log($"Deserialize messageId:" + msgId);
                if (!sendMessageHandles.ContainsKey(msgId))
                {
                    Debug.LogError($"MsgId:{msgId} not exist in handles");
                    return;
                }
                var messageHandle = sendMessageHandles[msgId];

                sendMessageHandles.Remove(msgId);
                binaryReader.BaseStream.Seek(-2, SeekOrigin.Current);
                byte[] data = binaryReader.ReadBytes(length);
                var message = MessageSerializeHelper.Deserialize(data);
                messageHandle.Handle?.Invoke(message);
            }
        }

        private void OnRecive(long id, byte[] buffer)
        {
            if (id != channelId)
            {
                Debug.LogError($"Network error,client channel id:{id} param id:{id}");
                return;
            }
            var messageId = BitConverter.ToUInt16(buffer, 0);
            Debug.Log($"Deserialize messageId:" + messageId);
            if (!sendMessageHandles.ContainsKey(messageId))
            {
                Debug.LogError("This shouldn't happen actully, please check the program");
                return;
            }
            var messageHandle = sendMessageHandles[messageId];

            sendMessageHandles.Remove(messageId);

            var message = MessageSerializeHelper.Deserialize(buffer);

            messageHandle.Handle?.Invoke(message);
        }

        public void Send<T>(object data, Action<T> onReceive, Action<int> onError = null) where T : IMessage
        {
            if (!isConnected)
            {
                Debug.LogWarning($"Network connect not finish, localConv:{channelId}");
                return;
            }

            var dataType = data.GetType();
            if (!typeof(IMessage).IsAssignableFrom(dataType))
            {
                Debug.LogError("NetworkClient::Send ,msg isn't extend from IMessage");
                return;
            }

            var ackId = MessageMapCenter.GetMessageId(typeof(T));
            if (sendMessageHandles.ContainsKey(ackId))
            {
                Debug.LogWarning($"Network already send msg ,type:{data.GetType()}");
                return;
            }
            Debug.Log("Send message to server, type:" + typeof(T));
            var messageData = new MessageHandle()
            {
                Id = ackId,
                Error = onError,
                Handle = (ack) => { onReceive?.Invoke((T)ack); },
                MsgData = data
            };
            sendMessageHandles.Add(ackId, messageData);
            var buffer = MessageSerializeHelper.Serialize(data);
            netService.Send(channelId, (writer) => 
            {
                writer.Write(buffer);
            });
        }

        public void Update()
        {
            netService.Update();
        }
    }
}
