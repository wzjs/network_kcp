using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Protocol
{
    public static class MessageMapCenter
    {
        private static readonly Dictionary<Type, ushort> typeMessageId = new Dictionary<Type, ushort>();
        private static readonly Dictionary<ushort, Type> messageIdType = new Dictionary<ushort, Type>();

        private static readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();

        static MessageMapCenter()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                var messageAtt = type.GetCustomAttribute<MessageAttribute>();
                if (messageAtt != null)
                {
                    typeMessageId.Add(type, messageAtt.MessageId);
                    messageIdType.Add(messageAtt.MessageId, type);
                }

                if (typeof(IRequest).IsAssignableFrom(type))
                {
                    var responseAtt = type.GetCustomAttribute<ResponseTypeAttribute>();
                    if(responseAtt != null)
                    {
                        requestResponse.Add(type, responseAtt.ResponseType);
                    }
                }
            }
        }

        public static ushort GetMessageId(Type type)
        {
            if(typeMessageId.TryGetValue(type, out ushort messageId))
            {
                return messageId;
            }
            Debug.LogError($"GetMessageId is not exist ,type:{type.Name}");
            return 0;
        }

        public static Type GetTypeById(ushort messageId)
        {
            if (messageIdType.TryGetValue(messageId, out Type type))
            {
                return type;
            }
            Debug.LogError($"GetTypeById is not exist ,messageId:{messageId}");
            return null;
        }

        public static Type GetResponseType(Type request)
        {
            if (requestResponse.TryGetValue(request, out Type type))
            {
                return type;
            }
            Debug.LogError($"GetResponseType is not exist ,request:{request.Name}");
            return null;
        }
    }
}
