using System.Buffers;
using System.Net;
using System.Net.Sockets.Kcp;

namespace KcpUnityDemo
{
    public static class KCPProtocalType
    {
        public const byte SYN = 1;
        public const byte ACK = 2;
        public const byte FIN = 3;
        public const byte MSG = 4;
        public const byte ReconnectSYN = 5;
        public const byte ReconnectACK = 6;
        //心跳?
    }

    public class KCPService : NetService
    {
        public const int ConnectTimeoutTime = 10 * 1000;
        private EndPoint endPoint = new IPEndPoint(IPAddress.Any,4001);

        private readonly Dictionary<long, KCPChannel> localConvChannels = new();
        private readonly Dictionary<long, KCPChannel> waitAcceptChannels = new();

        private IKCP transporter;
        private byte[] cache = new byte[2048];
        private readonly long startTime;

        
        public uint TimeNow
        {
            get
            {
                return (uint)(Time.NowUTC() - this.startTime);
            }
        }

        public IKCP Transporter
        {
            get { return transporter; }
        }

        public KCPService(EndPoint localEndPoint)
        {
            transporter = new UDPTransporter(localEndPoint);
            this.startTime = Time.NowUTC();
        }


        public void Recv()
        {
            while (this.transporter.Available() > 0)
            {
                int len = transporter.Recv(this.cache, ref endPoint);

                if(len <= 1)
                {
                    Debug.LogError("KCPServer:Recv raw msg len must > 1");
                    return;
                    //continue;
                }
                KCPChannel kChannel;

                var flag = cache[0];
                {
                    IPEndPoint ipPoint = endPoint as IPEndPoint;
                    Debug.Log($"KCPServer:Recv data len:{len},KCPMsgType={flag} from ip:{ipPoint.Address} port:{ipPoint.Port} ");
                }

                uint localConv;
                uint remoteConv;
                switch (flag)
                {
                    case KCPProtocalType.SYN:
                        if(len < 9)
                        {
                            Debug.LogError("KCPServer:Recv KCPType=SYN,msg len must > 9, len:" + len);
                            break;
                        }
                        remoteConv = BitConverter.ToUInt32(cache, 1);
                        localConv = BitConverter.ToUInt32(cache, 5);

                        waitAcceptChannels.TryGetValue(remoteConv, out kChannel);
                        if(kChannel == null)
                        {
                            //temp
                            localConv = NetworkHelper.IncrementRemoteConv();

                            if (localConvChannels.ContainsKey(localConv))
                            {
                                break;
                            }
                            IPEndPoint ip = (IPEndPoint)endPoint;
                            IPEndPoint ipEndPoint = new IPEndPoint(ip.Address, ip.Port); 
                            kChannel = new KCPChannel(localConv, remoteConv, ipEndPoint ,this);
                            this.waitAcceptChannels.Add(kChannel.RemoteConv, kChannel); // 连接上了或者超时后会删除
                            this.localConvChannels.Add(kChannel.LocalConv, kChannel);

                            //kChannel.RealAddress = realAddress;

                            //this.AcceptCallback(kChannel.Id, realEndPoint);
                        }

                        if(kChannel.RemoteConv != remoteConv)
                        {
                            break;
                        }

                        try
                        {
                            byte[] buffer = cache;
                            buffer.WriteTo(0,KCPProtocalType.ACK);
                            buffer.WriteTo(1, kChannel.LocalConv);
                            buffer.WriteTo(5, kChannel.RemoteConv);
                            Debug.Log($"KCPServer:Send,Type=ACK,localConv:{localConv} remoteConv:{remoteConv}");
                            Transporter.Send(buffer, 0, 9,kChannel.RemoteAddress);
                        }
                        catch(Exception ex)
                        {
                            Debug.LogError(ex.Message);
                        }

                        break;
                    case KCPProtocalType.ACK:
                        if (len != 9)
                        {
                            Debug.LogError("KCPServer:Recv KCPType=ACK,msg len must > 9, len:" + len);
                            break;
                        }

                        remoteConv = BitConverter.ToUInt32(this.cache, 1);
                        localConv = BitConverter.ToUInt32(this.cache, 5);
                        kChannel = this.Get(localConv);
                        if (kChannel != null)
                        {
                            Debug.Log($"KCPServer:Recv, KCPType=Ack localConv:{localConv}  remoteConv:{remoteConv}");
                            kChannel.RemoteConv = remoteConv;
                            kChannel.HandleConnect();
                        }
                        break;
                    case KCPProtocalType.FIN:

                        if (len != 13)
                        {
                            Debug.LogError("KCPServer:Recv KCPType=FIN,msg len must > 13, len:" + len);
                            break;
                        }

                        remoteConv = BitConverter.ToUInt32(this.cache, 1);
                        localConv = BitConverter.ToUInt32(this.cache, 5);
                        int error = BitConverter.ToInt32(this.cache, 9);

                        // 处理chanel
                        kChannel = this.Get(localConv);
                        if (kChannel == null)
                        {
                            break;
                        }

                        if (kChannel.RemoteConv != remoteConv)
                        {
                            break;
                        }

                        Debug.Log($"KCPServer:Recv KCPType=FIN: localConv:{localConv} remoteConv:{remoteConv} error:{error}");
                        kChannel.OnError(ErrorCode.ERR_PeerDisconnect);

                        break;
                    case KCPProtocalType.MSG:
                        if(len < 9)
                        {
                            Debug.LogError("KCPServer:Recv KCPType=MSG,msg len must > 9, len:" + len);
                            break;
                        }

                        remoteConv = BitConverter.ToUInt32(cache, 1);
                        localConv = BitConverter.ToUInt32(this.cache, 5);

                        kChannel = this.Get(localConv);
                        if (kChannel.RemoteConv != remoteConv)
                        {
                            break;
                        }
                        
                        if (!kChannel.IsConnected)
                        {
                            kChannel.IsConnected = true;
                            this.waitAcceptChannels.Remove(kChannel.RemoteConv);
                        }
                        kChannel.HandleRecv(this.cache, 9, len - 9);
                        break;
                    case KCPProtocalType.ReconnectSYN:
                        if(len != 9)
                        {
                            Debug.LogError("KCPServer:Recv KCPType=ReconnectSYN,msg len must > 9, len:" + len);
                            break;
                        }
                        remoteConv = BitConverter.ToUInt32(cache, 1);
                        localConv = BitConverter.ToUInt32(cache, 5);
                        
                        kChannel = Get(localConv);
                        if(kChannel == null)
                        {
                            Debug.LogWarning($"kcpChannel reconnect not found channel : {localConv} {remoteConv}");
                            break;
                        }

                        if(localConv != kChannel.LocalConv)
                        {
                            Debug.LogWarning($"kcpChannel reconnect localConv error:{localConv} {remoteConv} {kChannel.LocalConv}");
                            break;
                        }

                        if(remoteConv != kChannel.RemoteConv)
                        {
                            Debug.LogWarning($"kcpChannel reconnect remoteConv error:{localConv} {remoteConv} {kChannel.RemoteConv}");
                            break;
                        }

                        try
                        {
                            byte[] data = cache;
                            data.WriteTo(0, KCPProtocalType.ReconnectACK);
                            data.WriteTo(1, kChannel.LocalConv);
                            data.WriteTo(5,kChannel.RemoteConv);
                            transporter.Send(data, 0, 9, endPoint);
                            //Todo:服务端将该channel进队列,等待发送重连NTF 
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                            kChannel.OnError();
                        }
                        break;
                    case KCPProtocalType.ReconnectACK:
                        remoteConv = BitConverter.ToUInt32(this.cache, 1);
                        localConv = BitConverter.ToUInt32(this.cache, 5);
                        //重连客户端收到ACK,等待接受NTF
                        
                        break;
                }
            }
        }

        public KCPChannel Get(long id)
        {
            KCPChannel channel;
            this.localConvChannels.TryGetValue(id, out channel);
            return channel;
        }

        public override void Create(long id,IPEndPoint address)
        {
            if (this.localConvChannels.TryGetValue(id, out KCPChannel kChannel))
            {
                return;
            }

            try
            {
                uint localConn = (uint)id;
                kChannel = new KCPChannel(localConn, address, this);
                this.localConvChannels.Add(kChannel.LocalConv, kChannel);
            }
            catch (Exception e)
            {
                Debug.LogError($"kservice get error: {id}\n{e}");
            }
        }

        public void Remove(long id,int error = 0)
        {
            if (!this.localConvChannels.TryGetValue(id, out KCPChannel kChannel))
            {
                return;
            }

            kChannel.Error = error;
            Debug.Log($"kservice remove channel: {id} {kChannel.LocalConv} {kChannel.RemoteConv} error:{error}");
            this.localConvChannels.Remove(kChannel.LocalConv);
            if (this.waitAcceptChannels.TryGetValue(kChannel.RemoteConv, out KCPChannel waitChannel))
            {
                if (waitChannel.LocalConv == kChannel.LocalConv)
                {
                    this.waitAcceptChannels.Remove(kChannel.RemoteConv);
                }
            }

            kChannel.Dispose();
        }

        public void Disconnect(uint localConn, uint remoteConn, int error, EndPoint address, int times = 1)
        {
            try
            {
                if (this.Transporter == null)
                {
                    throw new NullReferenceException();
                }
                
                byte[] buffer = this.cache;
                buffer.WriteTo(0, KCPProtocalType.FIN);
                buffer.WriteTo(1, localConn);
                buffer.WriteTo(5, remoteConn);
                buffer.WriteTo(9, (uint)error);
                for (int i = 0; i < times; ++i)
                {
                    this.Transporter.Send(buffer, 0, 13, address);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Disconnect error {localConn} {remoteConn} {error} {address} {e}");
            }

            Debug.Log($"channel send fin: {localConn} {remoteConn} {address} {error}");
        }

        public override void Dispose()
        {
            foreach (long channelId in this.localConvChannels.Keys.ToArray())
            {
                this.Remove(channelId);
            }

            Transporter.Dispose();
            transporter = null;
        }

        public override void Send(long channelId, byte[] data)
        {
            var kChannel =  Get(channelId);
            if(kChannel != null)
            {
                kChannel.Send(data);
            }
        }

        public override void Update()
        {
            this.Recv();
            // Todo: 按需更新,提高服务端性能
            foreach (var item in localConvChannels)
            {
                item.Value.Update(TimeNow);
            }

        }
    }
}
