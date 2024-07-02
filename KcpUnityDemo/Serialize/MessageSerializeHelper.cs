using KcpUnityDemo.Protocol;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Serialize
{
    public static class MessageSerializeHelper
    {
        public const byte MessageIdSize = 2;

        //public static byte[] ToProtobuf<T>(T message) where T : IMessage
        //{
        //    byte[] buffer = MessagePackSerializer.Serialize(message);
        //    var result = new byte[buffer.Length + MessageIdSize];
        //    buffer.CopyTo(result, 2);
        //    var messageId = MessageMapCenter.GetMessageId(typeof(T));
        //    if (messageId != 0)
        //    {
        //        result.WriteTo(0, messageId);
        //    }
        //    return result;
        //}

        public static T Deserialize<T>(byte[] message) where T : IMessage
        {
            return MessagePackSerializer.Deserialize<T>(message.AsMemory().Slice(2).ToArray());
        }

        public static byte[] Serialize<T>(T message) where T : IMessage
        {
            //byte[] buffer = MessagePackSerializer.Serialize(message);
            byte[] buffer = MessagePackSerializer.Serialize(message);
            var result = new byte[buffer.Length + MessageIdSize];
            buffer.CopyTo(result, 2);
            var messageId = MessageMapCenter.GetMessageId(typeof(T));
            if (messageId != 0)
            {
                result.WriteTo(0, messageId);
            }
            return result;
        }

        public static byte[] Serialize(object message)
        {
            //byte[] buffer = MessagePackSerializer.Serialize(message);
            var dataType = message.GetType();
            byte[] buffer = MessagePackSerializer.Serialize(dataType,message);
            var result = new byte[buffer.Length + MessageIdSize];
            buffer.CopyTo(result, MessageIdSize);
            var messageId = MessageMapCenter.GetMessageId(dataType);
            if (messageId != 0)
            {
                result.WriteTo(0, messageId);
            }
            return result;
        }

        public static object Deserialize(byte[] message)
        {
            ushort messageId = (ushort)BitConverter.ToInt16(message.AsSpan(0, 2));
            var responseType = MessageMapCenter.GetTypeById(messageId);
            return MessagePackSerializer.Deserialize(responseType, message.AsMemory().Slice(2).ToArray());
        }
    }
}
