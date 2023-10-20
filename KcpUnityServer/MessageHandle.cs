using KcpUnityDemo;
using KcpUnityDemo.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityServer
{
    public class MessageHandle
    {
        private Dictionary<Type,Action<object, long>> protoHandle = new Dictionary<Type, Action<object, long>> ();
        private NetworkServer server;

        public MessageHandle(NetworkServer netServer)
        {
            server = netServer;
            RegisterHandles();
        }

        public void RegisterHandles()
        {
            protoHandle[typeof(C2S_Login)] = C2S_Login_Handle;
            protoHandle[typeof(C2S_TestData)] = C2S_TestData_Handle;
        }

        private void C2S_TestData_Handle(object msg, long channelId)
        {
            if (!MsgVaild(msg, typeof(C2S_TestData)))
            {
                Debug.LogError($"MessageHandle::Error Msg type {msg.GetType()} isn't match this method");
                return;
            }
            var data = msg as C2S_TestData;
            Debug.Log($"Receive C2S_TestData data , message:{data.Message}");
            server.Send(new S2C_TestData() { Message = "Response by server" }, channelId);
        }

        public void ProcessHandle(object msg,long channelId)
        {
            var type = msg.GetType();
            if(!protoHandle.ContainsKey(type))
            {
                Debug.LogError($"MessageHandle::ProcessHandle type:{type} isn't exist in protoHandle");
                return;
            }

            protoHandle[type].Invoke(msg, channelId);
        }

        public void C2S_Login_Handle(object msg, long channelId)
        {
            if (!MsgVaild(msg, typeof(C2S_Login)))
            {
                Debug.LogError($"MessageHandle::Error Msg type {msg.GetType()} isn't match this method");
                return;
            }
            var data = msg as C2S_Login;
            Debug.Log($"Receive C2S_Login data , account:{data.Account} password:{data.Password}");
            server.Send(new S2C_Login() { Error = 0 }, channelId);

        }

        private bool MsgVaild(object msg,Type type)
        {
            return msg.GetType() == type;
        }
    }
}
