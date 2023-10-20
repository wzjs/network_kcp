using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo.Serialize
{
    public interface INetSerializer
    {
        public byte[] Serialize(object data);
        public object Deserialize(byte[] data);
    }
}
