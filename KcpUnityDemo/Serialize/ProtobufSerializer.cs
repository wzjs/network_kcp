using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Serialize
{
    public class ProtobufSerializer : INetSerializer
    {
        public object Deserialize(byte[] data)
        {
            return MessagePackSerializer.Deserialize<object>(data);
        }

        public byte[] Serialize(object data)
        {
            byte[] bytes = MessagePackSerializer.Serialize(data);
            return bytes;
        }
    }
}
