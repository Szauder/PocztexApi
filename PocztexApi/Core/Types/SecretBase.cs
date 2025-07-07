using System.Text;

namespace PocztexApi.Core.Types;

public abstract record SecretBase(byte[] Bytes)
{
    public string AsUtf8String => Encoding.UTF8.GetString(Bytes);
    public string AsBase64String => Convert.ToBase64String(Bytes);

    public static byte[] FromUtf8(string utf8) => Encoding.UTF8.GetBytes(utf8);
    public static byte[] FromBase64(string base64) => Convert.FromBase64String(base64);
}