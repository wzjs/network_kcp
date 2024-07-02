using KcpUnityDemo;
using System.Net;
using System.Net.Sockets;

public class TCPService : NetService
{
    private Dictionary<long, TCPChannel> _channels = new Dictionary<long, TCPChannel>();
    private readonly bool _isServer;
    private readonly Socket _acceptSocket;
    public TCPService(bool isServer)
    {
        _isServer = isServer;
        if (_isServer)
        {
            _acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _acceptSocket.Bind(new IPEndPoint(IPAddress.Any, 5001));
            _acceptSocket.Listen();
        }
    }

    public void Accept()
    {
        while (true)
        {
            var connect = _acceptSocket.Accept();
            var tcpChannel = new TCPChannel((int)NetworkHelper.IncrementRemoteConv(), this, 5, 4096);
            tcpChannel.SetSocket(connect);
            _channels.Add((int)tcpChannel.Id, tcpChannel);
            ConnectCallBack?.Invoke(tcpChannel.Id);
        }
    }

    public void Receive(long channelId, Action<BinaryReader, int, bool> dataReaderHandler)
    {
        if (_channels.TryGetValue(channelId, out TCPChannel channel))
        {
            channel.ReceiveAsync(dataReaderHandler, null);
        }
    }
    public override void Create(long channelId, IPEndPoint remoteEndPoint)
    {
        var channel = new TCPChannel(channelId, this, 5, 4096);
        _channels.Add(channelId,channel);
        channel.Connect(remoteEndPoint);
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Send(long channelId, Action<BinaryWriter> action)
    {
        if (_channels.TryGetValue(channelId, out TCPChannel channel))
        {
            channel.SendAsync(action, null);
        }
    }

    public override void Update()
    {
        
    }

    public override void Send(long channelId, byte[] data)
    {
        throw new NotImplementedException();
    }
}

