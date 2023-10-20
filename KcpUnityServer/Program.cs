using KcpUnityDemo;
using KcpUnityDemo.Protocol;
using KcpUnityDemo.Serialize;
using System.Net;
using System.Text;

namespace KcpUnityServer
{
    internal class Program
    {
        private static EndPoint localPoint = new IPEndPoint(IPAddress.Any, 5001);
        static void Main(string[] args)
        {
            Console.WriteLine("Start server");
            StartServer();
        }


        public static void StartServer()
        {
            var server = new NetworkServer();
            server.StartServer();
            //var server = new KCPService(localPoint);
            //var serializer = new NetSerializeService(NetSerializeType.protobuf);
            //server.ReadCallback = (id, data) =>
            //{
            //    var recv = NetworkHelper.Recv<C2S_Login>(data);
            //    Debug.Log($"From client data ,{recv} password:{recv.Password}  account:{recv.Account}");
            //    NetworkHelper.Send(server ,id, new S2C_Login() { Error = 0 });
            //};
            //Task.Run(async () => {
            //    while (true)
            //    {
            //        await Task.Delay(10);
            //        server.Update();
            //    }
            //});
        }
    }
}