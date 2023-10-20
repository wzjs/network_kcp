using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public enum ChannelType
    {
        Connect,
        Accept,
    }
    public abstract class Channel
    {
        public long Id;

        public ChannelType ChannelType { get; protected set; }

        public int Error { get; set; }


        public bool IsDisposed
        {
            get
            {
                return this.Id == 0;
            }

        }
        public abstract void Dispose();

    }
}
