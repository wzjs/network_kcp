using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Protocol
{
    public class MessageAttribute : Attribute
    {
        public ushort MessageId { get; set; }
        public MessageAttribute(ushort messageId)
        {
            MessageId = messageId;
        }
    }

    public class ResponseTypeAttribute : Attribute
    {
        public Type ResponseType { get; set; }
        public ResponseTypeAttribute(Type responseType)
        {
            ResponseType = responseType;
        }
    }
}
