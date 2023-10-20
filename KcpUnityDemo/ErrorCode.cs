using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public static class ErrorCode
    {
        public const int ERR_KCPConnectTimeout = 100001;
        public const int ERR_KCPSocketSendFail = 100002;
        public const int ERR_PeerDisconnect = 100003;
    }
}
