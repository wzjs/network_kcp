
using KcpUnityDemo;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

public class TCPChannel : Channel
{
    private sealed class ConnectState
    {
        public Action<Exception> Callback;
    }

    private sealed class SendState
    {
        public Action<Exception, int> Callback;
        public ArraySegment<byte> Buffer;
    }
    private sealed class ReceiveState
    {
        public ArraySegment<byte> Buffer;
        public Action<BinaryReader, int, bool> DateReaderHandler;
        public Action<Exception> FailHandler;
        public int BytesLeft;
    }

    private Socket _socket;
    private PacketTool _pactetTool;
    public int MessageMinLenght { get; set; }
    public int MessageMaxLength { get; set; }
    public bool IsConnected => _socket != null && _socket.Connected;
    public TCPChannel(long id,NetService service,int msgMinSize,int msgMaxSize) : base(id,ChannelType.Connect, service)
    {
        MessageMinLenght = msgMinSize;
        MessageMaxLength = msgMaxSize;
        _pactetTool = new PacketTool(msgMinSize, msgMaxSize);
    }

    public void SetSocket(Socket socket)
    {
        _socket = socket;
    }

    public void Connect(IPEndPoint point)
    {
        try
        {
            _socket = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(point, OnConnect, null);
        }
        catch (Exception e)
        {
            OnError(e);
        }

    }

    private void OnConnect(IAsyncResult ar)
    {
        try
        {
            _socket.EndConnect(ar);
            NetService.ConnectCallBack(Id);
        }
        catch (Exception e)
        {
            OnError(e);
        }
    }

    public void SendAsync(Action<BinaryWriter> dataWriter, Action<Exception, int> callBack)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            _pactetTool.Packet(stream,dataWriter);
            byte[] data = stream.ToArray();
            SendState state = new SendState() { Buffer = new ArraySegment<byte>(data,0,data.Length)};
            StartSend(state);
        }
    }

    private void StartSend(SendState state)
    {
        try
        {
            _socket.BeginSend(state.Buffer.Array, state.Buffer.Offset, state.Buffer.Count, SocketFlags.None, OnSend, state);
        }
        catch (Exception e)
        {
            OnError(e);
        }
    }

    private void OnSend(IAsyncResult ar)
    {
        SendState state = (SendState)ar.AsyncState;
        try
        {
            SocketError error;
            int bytesSend = _socket.EndSend(ar,out error);
            if(error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }
            if(bytesSend <= 0)
            {
                throw new SocketException((int)SocketError.Shutdown);
            }else if(bytesSend == state.Buffer.Count)
            {
                
            }
            else
            {
                state.Buffer = new ArraySegment<byte>(state.Buffer.Array, state.Buffer.Offset + bytesSend, state.Buffer.Count - bytesSend);
                StartSend(state);
            }
        }
        catch (Exception e)
        {
            OnError(e);
        }
    }

    public void ReceiveAsync(Action<BinaryReader,int,bool> dataReaderHandler,Action<Exception> failHandler)
    {
        try
        {
            byte[] buffer = new byte[MessageMaxLength];
            var state = new ReceiveState() { 
                Buffer = buffer,
                DateReaderHandler = dataReaderHandler ,
                FailHandler = failHandler,
            };
            StartReceive(state);
        }
        catch (Exception e)
        {
            OnError(e);
        }
    }

    private void StartReceive(ReceiveState state)
    {
        try
        {
            _socket.BeginReceive(state.Buffer.Array, state.Buffer.Offset, state.Buffer.Count - state.Buffer.Offset, SocketFlags.None, OnReceive, state);
        }
        catch (Exception e)
        {
            OnError(e);
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        ReceiveState state = (ReceiveState)ar.AsyncState;
        try
        {
            SocketError error;
            int byresRead = _socket.EndReceive(ar, out error);
            if(error != SocketError.Success)
            {
                throw new SocketException((int)error);
            }

            if(byresRead <= 0)
            {
                throw new SocketException((int)SocketError.Shutdown);
            }
            else
            {
                state.BytesLeft += byresRead;
                int bytesHandled = 0;
                using(MemoryStream stream = new MemoryStream(state.Buffer.Array,0,state.BytesLeft))
                {
                    _pactetTool.Unpacked(stream, state.DateReaderHandler);
                    bytesHandled = (int)stream.Position;
                }

                if(bytesHandled < 0 || bytesHandled > state.BytesLeft)
                {
                    throw new Exception("Invalid bytes handled");
                }

                state.BytesLeft -= bytesHandled;
                if(state.Buffer.Array.Length == state.BytesLeft)
                {
                    throw new Exception("Unexpect Array length == state.bytesLeft");
                }

                if(bytesHandled > 0 && state.BytesLeft > 0)
                {
                    Buffer.BlockCopy(state.Buffer.Array, bytesHandled, state.Buffer.Array, 0, state.BytesLeft);
                }

                state.Buffer = new ArraySegment<byte>(state.Buffer.Array, state.BytesLeft, state.Buffer.Count - state.BytesLeft);
                StartReceive(state);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            OnError(e);
        }
    }

    public override void Close()
    {
        base.Close();
        _socket.Close();
        _socket = null;
    }

    private void OnError(Exception e)
    {
        Close();
        NetService.ErrorCallback?.Invoke(Id, 0);
    }
    public override void Dispose()
    {
        throw new NotImplementedException();
    }

    private sealed class PacketTool
    {
        private const int kLenghtTitleCount = sizeof(int);
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public PacketTool(int minSize,int maxSize)
        {
            MinLength = minSize;
            MaxLength = maxSize;
        }

        public void Packet(Stream stream,Action<BinaryWriter> dataWriterAction)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            int length = 0;
            binaryWriter.Write(length);
            dataWriterAction(binaryWriter);
            length = (int)binaryWriter.BaseStream.Position;
            binaryWriter.Seek(0, SeekOrigin.Begin);
            binaryWriter.Write(length);
            binaryWriter.BaseStream.Position = length;
            binaryWriter.Flush();
        }

        public void Unpacked(Stream stream,Action<BinaryReader,int,bool> dataReaderAction)
        {
            long lengthOfStream = stream.Length;
            BinaryReader binary = new BinaryReader(stream);
            while (true)
            {
                long startPosition = stream.Position;
                long remainLength = lengthOfStream - startPosition;
                if(remainLength >= kLenghtTitleCount)
                {
                    int length = binary.ReadInt32();
                    bool isEncrypt = ((length & int.MinValue) != 0);
                    length &= int.MaxValue;

                    if(length < MinLength || length > MaxLength)
                    {
                        throw new Exception(string.Format("msg's length[{0}] is error", length));
                    }

                    if(remainLength >= length) //全包
                    {
                        int remainDateLength = length - kLenghtTitleCount;
                        dataReaderAction(binary, remainDateLength, isEncrypt);
                    }
                    else //半包
                    {
                        stream.Seek(-kLenghtTitleCount, SeekOrigin.Current);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

}

