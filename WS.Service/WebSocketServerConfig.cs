using System.Net;

namespace WS.Service
{
    /// <summary>
    /// 配置项
    /// </summary>
    public class WebSocketServerConfig
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public string ServerName { get; set; }
        public bool IsUseCertificate { get; set; }
        public string CertificateStoreName { get; set; }
        public string Security { get; set; }
        public string CertificateThumbprint { get; set; }
        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; }
    }
}
