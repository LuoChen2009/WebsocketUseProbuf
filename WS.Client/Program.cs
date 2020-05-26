using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using WS.Common;
using Google.Protobuf.WellKnownTypes;

namespace WS.Client
{
    class Program
    {
        static void Main(string[] args)
        {

            ClientWebSocket clientWebSocket = new ClientWebSocket();


            Console.WriteLine("Starting to Connect Server");
            string userName = "lei.sun";
            string pwd = "1234";
            string url = string.Format("{0}?username={1}&password={2}", "ws://localhost:58090", userName, pwd);
            clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None).Wait();
            Console.WriteLine("Connecting Server Successful.");

            Task.Run(() => StartReceiving(clientWebSocket));

            var requestBody = Any.Pack(new QueryAccountOrder { AccountId = "test1", OrderId = "123" });
            var request = new RequestMessageAny { ReqeustType = EnumRequestType.QueryOrderReq, RequestBody = requestBody };

            var bytes = request.ToByteArray();
            clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, CancellationToken.None);

            Console.ReadKey();
        }

        static async Task StartReceiving(ClientWebSocket client)
        {
            try
            {
                int recCount = 0;
                int count = 0;
                MemoryStream ms = new MemoryStream();
                while (true)
                {
                    var array = new byte[64 * 1024];//64k
                    var buffer = new ArraySegment<byte>(array);
                    var result = await client.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                    {
                        //字符串
                        string msg = Encoding.UTF8.GetString(array, 0, result.Count);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Receive:{0}", msg);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        //byte
                        //Thread.Sleep(100);
                        count++;
                        Console.WriteLine("Receive Count:{0}", count);
                        byte[] receBt = new byte[result.Count];
                        Array.Copy(array, receBt, result.Count);
                        Console.WriteLine("ReceByteCount:{0},IsEndOfMessage:{1}", result.Count, result.EndOfMessage);
                        string showMsg = string.Empty;

                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                        if (!result.EndOfMessage)
                        {
                            continue;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive Data From Service Error:{0}", e.Message);
            }

        }
    }
}
