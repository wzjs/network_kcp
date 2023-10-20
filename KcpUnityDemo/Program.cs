//using KcpUnityDemo.Protocol;
//using KcpUnityDemo.Serialize;
//using Microsoft.VisualBasic;
//using System.Net;
//using System.Text;
//using System.Text.Unicode;

//namespace KcpUnityDemo
//{
//    internal class Program
//    {
//        private static NetworkClient networkClient;
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Start client");
//            networkClient = new NetworkClient();
//            Task.Run(async () =>
//            {
//                while (true)
//                {
//                    await Task.Delay(10);
//                    networkClient.Update();
//                }
//            });

//            while (true)
//            {
//                Console.WriteLine("Input F to send");
//                var key = Console.ReadKey();
//                if (key.Key == ConsoleKey.F)
//                {
//                    networkClient.Send<S2C_Login>(new C2S_Login() { Password = "123456", Account = "admin" }, (obj) =>
//                    {
//                        Debug.Log("error:" + obj.Error);
//                    });
//                }else if(key.Key == ConsoleKey.D)
//                {
//                    networkClient.Send<S2C_TestData>(new C2S_TestData() { Message = "Test Data" }, (obj) =>
//                    {
//                        Debug.Log("error:" + obj.Error + " message:" + obj.Message);
//                    });
//                }
//            }
//        }


//    }
//}

using KcpUnityDemo.Protocol;
using KcpUnityDemo.Serialize;
using Microsoft.VisualBasic;
using System.Net;
using System.Text;
using System.Text.Unicode;

namespace KcpUnityDemo
{
    internal class Program
    {
        private static NetworkClient networkClient;
        static void Main(string[] args)
        {
            Console.WriteLine("Start client");
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    NetworkClient networkClient = new NetworkClient();
                    Debug.Log("client numbern :" + i);
                    Task.Delay(100);
                }
            });
            Console.ReadLine();

        }


    }
}