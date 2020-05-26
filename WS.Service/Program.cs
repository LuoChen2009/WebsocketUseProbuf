using log4net;
using SuperSocket.WebSocket;
using System;
using WS.Common;
using Newtonsoft.Json;

namespace WS.Service
{
    class Program
    {
        private static SuperWebSocketServer _server;
        private static ILog _log = log4net.LogManager.GetLogger("TestLog");

        static void Main(string[] args)
        {
            
            _server = SuperWebSocketServer.GetIntance();

            _server.OnReceiveMessage += Server_OnReceiveMessage;
            _server.OnReceiveBytes += Server_OnReceiveBytes;
            _server.OnClosed += Server_OnClosed;

            WebSocketServerConfig config = new WebSocketServerConfig
            {
                ServerName = "WebSocketServerDemo",
                Port = 58090,
            };

            Console.WriteLine("Starting...");

            var isSuccess = _server.Start(config);
            if (isSuccess)
            {
                Console.WriteLine("Starting Successful.");
            }
            else
            {
                Console.WriteLine("Starting Fail.");
            }

            Console.ReadKey();
        }

        private static void Server_OnReceiveBytes(WebSocketSession arg1, byte[] arg2)
        {
            try
            {
                var receivedData = RequestMessageAny.Parser.ParseFrom(arg2);
                var requestBody = receivedData.RequestBody;
                switch (receivedData.ReqeustType)
                {
                    case EnumRequestType.QueryFillReq:
                        var queryAccount = requestBody.Unpack<QueryFill>();
                        break;
                    case EnumRequestType.QueryOrderReq:
                        var queryFill = requestBody.Unpack<QueryAccountOrder>();
                        break;
                }

                _log.Info(JsonConvert.SerializeObject(receivedData));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:{0},Stack:{1}", ex.Message, ex.StackTrace);
            }

            
            //Console.WriteLine("Receive Bytes Data Length:{0}", arg2.Length);
        }

        private static void Server_OnReceiveMessage(WebSocketSession arg1, string arg2)
        {
            Console.WriteLine("Receive Message Length:{0},Message:{1}", arg2.Length, arg2);
        }

        private static void Server_OnClosed(WebSocketSession arg1, SuperSocket.SocketBase.CloseReason arg2)
        {
            Console.WriteLine("Closed:{0},{1}", arg1.SessionID, arg2.ToString());
        }
    }
}
