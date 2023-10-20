using KcpUnityDemo.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Serialize
{
    public enum NetSerializeType
    {
        protobuf,
    }
    public class NetSerializeService
    {
        public const byte MessageIdSize = 2;
        private INetSerializer serializer;
        public NetSerializeService(NetSerializeType type) 
        {
            switch (type)
            {
                case NetSerializeType.protobuf:
                    serializer = new ProtobufSerializer();
                    break;
                default:
                    break;
            }
        }

        public byte[] Serialize(object data)
        {
            var buffer = serializer.Serialize(data);
            var result = new byte[buffer.Length + MessageIdSize];
            buffer.CopyTo(result, 2);
            var messageId = MessageMapCenter.GetMessageId(data.GetType());
            if(messageId != 0)
            {
                result.WriteTo(0,messageId);
            }
            return result;
        }

        public object Deserialize(byte[] buffer)
        {
            var messageId = BitConverter.ToUInt16(buffer, 0);
            Debug.Log($"Deserialize messageId:" + messageId);
            var obj = serializer.Deserialize(buffer.AsMemory().Slice(2).ToArray());
            return obj;
        }
    }
}
