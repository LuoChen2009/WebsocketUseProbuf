using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.WebSocket;
using System;
using System.Collections.Concurrent;

namespace WS.Service
{
    public class SuperWebSocketServer
    {
        private static readonly object _lockObj = new object();

        private static SuperWebSocketServer _superWebSocketServer;

        private WebSocketServer _websocketServer;

        private ConcurrentDictionary<string, WebSocketSession> _clientPools = new ConcurrentDictionary<string, WebSocketSession>();

        /// <summary>
        /// 接收字符串消息
        /// </summary>
        public event Action<WebSocketSession, string> OnReceiveMessage;
        /// <summary>
        /// 接收byte数组消息
        /// </summary>
        public event Action<WebSocketSession, byte[]> OnReceiveBytes;
        /// <summary>
        /// 新客户端连接
        /// </summary>
        public event Action<WebSocketSession> OnNewConnection;
        /// <summary>
        /// 关闭连接
        /// </summary>
        public event Action<WebSocketSession, CloseReason> OnClosed;

        public WebSocketSession GetSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return default(WebSocketSession);

            return _clientPools[sessionId];
        }

        private SuperWebSocketServer()
        {
            _websocketServer = new WebSocketServer();

            _websocketServer.NewSessionConnected += NewSessionConnected;
            _websocketServer.SessionClosed += SessionClosed;
            _websocketServer.NewMessageReceived += NewMessageReceived;
            _websocketServer.NewDataReceived += NewDataReceived;
        }

        public static SuperWebSocketServer GetIntance()
        {
            if (_superWebSocketServer == null)
            {
                lock (_lockObj)
                {
                    if (_superWebSocketServer == null)
                    {
                        _superWebSocketServer = new SuperWebSocketServer();
                    }
                }
            }

            return _superWebSocketServer;
        }

        public bool Start(WebSocketServerConfig config)
        {

            ServerConfig serverConfig = new ServerConfig
            {
                Name = config.ServerName,
                MaxConnectionNumber = 10000, //最大允许的客户端连接数目，默认为100。
                Mode = SocketMode.Tcp,
                Port = config.Port, //服务器监听的端口。
                ClearIdleSession = false,   //true或者false， 是否清除空闲会话，默认为false。
                ClearIdleSessionInterval = 120,//清除空闲会话的时间间隔，默认为120，单位为秒。
                ListenBacklog = 10,
                ReceiveBufferSize = 64 * 1024, //用于接收数据的缓冲区大小，默认为2048。
                SendBufferSize = 64 * 1024,   //用户发送数据的缓冲区大小，默认为2048。
                                              //ReceiveBufferSize = 64 * 1024, //用于接收数据的缓冲区大小，默认为2048。
                                              //SendBufferSize = 64 * 1024,   //用户发送数据的缓冲区大小，默认为2048。
                KeepAliveInterval = 1,     //keep alive消息发送时间间隔。单位为秒。
                KeepAliveTime = 60,    //keep alive失败重试的时间间隔。单位为秒。
                SyncSend = false
            };

            if (config.Ip != null && !string.IsNullOrWhiteSpace(config.Ip.ToString()))
            {
                serverConfig.Ip = config.Ip.ToString();
            }

            if (!_websocketServer.Setup(serverConfig))
                return false;

            return _websocketServer.Start();
        }

        private void NewDataReceived(WebSocketSession session, byte[] value)
        {
            var temp = OnReceiveBytes;
            if (temp != null)
            {
                temp.Invoke(session, value);
            }
        }

        private void NewMessageReceived(WebSocketSession session, string value)
        {
            var temp = OnReceiveMessage;
            if (temp != null)
            {
                temp.Invoke(session, value);
            }
        }

        private void SessionClosed(WebSocketSession session, CloseReason value)
        {
            var temp = OnClosed;
            if (temp != null)
            {
                temp.Invoke(session, value);
            }
        }

        private void NewSessionConnected(WebSocketSession session)
        {
            if (_clientPools.ContainsKey(session.SessionID))
            {
                return;
            }

            _clientPools.TryAdd(session.SessionID, session);
        }
    }
}
