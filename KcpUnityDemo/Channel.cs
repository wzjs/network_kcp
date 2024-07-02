using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        protected readonly NetService NetService;
        public int Error { get; set; }
        //public abstract bool IsConnected { get;}

        public bool IsDisposed
        {
            get
            {
                return this.Id == 0;
            }

        }
        public Channel(long id,ChannelType type,NetService netService)
        {
            Id = id;
            ChannelType = type;
            NetService = netService;
        }

        public virtual void Close()
        {

        }
        public abstract void Dispose();

    }
}
