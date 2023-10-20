using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Protocol
{
    public interface IMessage
    {
    }

    public interface IRequest : IMessage
    {

    }

    public interface IResponse : IMessage
    {
        public int Error { get; set; }
    }
}
