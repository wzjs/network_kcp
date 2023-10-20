using MessagePack;

namespace KcpUnityDemo.Protocol
{
    [Message(MessageId.C2S_TestData)]
    [ResponseType(typeof(S2C_TestData))]
    [MessagePackObject]
    public class C2S_TestData : IRequest
    {
        [Key(0)]
        public string Message { get; set; }
    }

    [Message(MessageId.S2C_TestData)]
    [MessagePackObject]
    public class S2C_TestData : IResponse
    {
        [Key(0)]
        public string Message { get; set; }
        [Key(1)]
        public int Error { get; set; }
    }

    [Message(MessageId.C2S_Login)]
    [ResponseType(typeof(S2C_Login))]
    [MessagePackObject]
    public class C2S_Login : IRequest
    {
        [Key(0)]
        public string Account { get; set; }

        [Key(1)]
        public string Password { get; set; }
    }

    [Message(MessageId.S2C_Login)]
    [MessagePackObject]
    public class S2C_Login : IResponse
    {
        [Key(0)]
        public int Error { get; set; }
    }



    public static class MessageId
    {
        public const ushort C2S_Login = 1001;
        public const ushort S2C_Login = 1002;
        public const ushort C2S_TestData = 1003;
        public const ushort S2C_TestData = 1004;
    }
}
