using Google.Protobuf;
using System.IO;
namespace WS.Common
{
    public static class DataConvertUtility
    {
        public static byte[] ToByteArray<T>(this T obj) where T : IMessage
        {
            if (obj == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                obj.WriteTo(ms);
                return ms.ToArray();
            }
        }
    }
}
