using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public class KCPChannel : Channel,IKcpCallback
    {
        private SimpleSegManager.Kcp kcp { get; set; }
        private readonly KCPService service;
        private readonly byte[] sendCache = new byte[1024];
        private uint lastConnectTime;
        public uint LocalConv
        {
            get { return (uint)Id; }
            set { Id = value; }
        }

        public uint RemoteConv { get;set; }
        public bool IsConnected { get; set; }
        private IPEndPoint remoteAddress;

        public IPEndPoint RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
            set
            {
                this.remoteAddress = value;
            }

        }

        public readonly uint CreateTime;

        public KCPChannel(uint localConn, IPEndPoint remoteEndPoint, KCPService kService):base(localConn,ChannelType.Connect,kService)
        {
            this.service = kService;
            this.LocalConv = localConn;

            Debug.Log($"channel create: {this.LocalConv} {remoteEndPoint} {this.ChannelType}");


            this.RemoteAddress = remoteEndPoint;
            this.CreateTime = kService.TimeNow;

            this.Connect(this.CreateTime);
        }

        // accept
        public KCPChannel(uint localConn, uint remoteConn, IPEndPoint remoteEndPoint, KCPService kService) : base(localConn, ChannelType.Accept,kService)
        {
            service = kService;
            ChannelType = ChannelType.Accept;

            Debug.Log($"channel create: {localConn} {remoteConn} {remoteEndPoint} {this.ChannelType}");
            LocalConv = localConn;
            RemoteConv = remoteConn;
            RemoteAddress = remoteEndPoint;
            kcp = new SimpleSegManager.Kcp(0, this);
            this.InitKcp();

            this.CreateTime = kService.TimeNow;
        }


        private void InitKcp()
        {
            kcp.NoDelay(1, 200, 20, 1);
        }

        public void Connect(uint timeNow)
        {
            try
            {
                if (this.IsConnected)
                {
                    return;
                }

                if(timeNow  > CreateTime + KCPService.ConnectTimeoutTime)
                {
                    Debug.LogError($"KcpChannel connect timeout: localConv:{Id} remoteConv:{RemoteConv} timeNow:{timeNow} createTime:{CreateTime}");
                    OnError(ErrorCode.ERR_KCPConnectTimeout);
                    return;
                }

                byte[] buffer = sendCache;
                buffer.WriteTo(0, KCPProtocalType.SYN);
                buffer.WriteTo(1, this.LocalConv);
                buffer.WriteTo(5, this.RemoteConv);
                this.service.Transporter.Send(buffer, 0, 9, this.RemoteAddress);

                this.lastConnectTime = timeNow;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                OnError(ErrorCode.ERR_KCPSocketSendFail);
            }
        }

        public void HandleConnect()
        {
            if(IsConnected)
            {
                return;
            }

            kcp = new SimpleSegManager.Kcp(0, this);
            Debug.Log($"channel connectd: id:{LocalConv} conv:{RemoteConv}");
            IsConnected = true;
            service.ConnectCallBack(LocalConv);
        }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            try
            {
                var data = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
                byte[] head = new byte[9];
                head.WriteTo(0, KCPProtocalType.MSG);
                head.WriteTo(1, LocalConv);
                head.WriteTo(5, RemoteConv);
                byte[] combinedArray = head.Concat(data).ToArray();

                service.Transporter.Send(combinedArray, 0, avalidLength + 9,RemoteAddress);
            }
            catch (Exception e)
            {
                OnError();
                Debug.LogError(e.Message);
            }
        }

        public void HandleRecv(byte[] date, int offset, int length)
        {
            if (IsDisposed)
            {
                return;
            }
            var result = kcp.Input(date.AsSpan(offset, length));
            if(result != 0)
            {
                Debug.LogError($"Kcp input fail,code:{result}");
                return;
            }

            var (buffer, avalidLength) = kcp.TryRecv();
            if(buffer != null)
            {
                var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
                service.ReceiveCallback(LocalConv, s);
            }

        }

        public void Send(Span<byte> data)
        {
            kcp.Send(data);
        }

        public void Update(uint timeNow)
        {
            if (this.IsDisposed)
            {
                return;
            }

            // 如果还没连接上，发送连接请求
            if (!this.IsConnected && this.ChannelType == ChannelType.Connect)
            {
                this.Connect(timeNow);
                return;
            }

            if (this.kcp == null)
            {
                return;
            }

            try
            {
                this.kcp.Update(DateTimeOffset.UtcNow);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                //this.OnError(ErrorCore.ERR_SocketError);
                return;
            }

            //uint nextUpdateTime = this.kcp.Check(timeNow);
            //this.Service.AddToUpdate(nextUpdateTime, this.Id);
        }

        public void OnError(int error = 0)
        {
            service.Remove(LocalConv, error);
            service.ErrorCallback(LocalConv,error);
        }


        public override void Dispose()
        {
            if(IsDisposed)
            {
                return;
            }

            Debug.Log($"channel dispose: {LocalConv} {RemoteConv} {this.Error}");

            Id = 0;
            service.Remove(Id);

            try
            {
                if (this.Error != ErrorCode.ERR_PeerDisconnect)
                {
                    this.service.Disconnect(LocalConv, RemoteConv, this.Error, this.RemoteAddress);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            this.kcp = null;
        }
    }
}
