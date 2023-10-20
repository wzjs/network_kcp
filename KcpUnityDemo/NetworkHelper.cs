using KcpUnityDemo.Protocol;
using KcpUnityDemo.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public static class NetworkHelper
    {
        private static uint remoteId = 1;
        public static void Send<T>(NetService netService,long channelId, T data) where T : IMessage
        {
            byte[] buffer = MessageSerializeHelper.Serialize(data);
            netService.Send(channelId,buffer);
        }

        public static T Recv<T>(byte[] data) where T : IMessage
        {
            var obj = MessageSerializeHelper.Deserialize<T>(data);
            return obj;
        }

        public static uint IncrementRemoteConv()
        {
            return Interlocked.Increment(ref remoteId);
        }
    }
}
