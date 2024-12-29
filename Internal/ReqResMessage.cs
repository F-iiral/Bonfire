namespace BonfireServer.Internal;
using System.Net;
using System.Text;

public class ReqResMessage(HttpListenerRequest request, HttpListenerResponse response)
{
    public byte[] Data = Encoding.UTF8.GetBytes(string.Empty);
    public readonly HttpListenerResponse Response = response;
    public readonly HttpListenerRequest Request = request;
    public bool IsValid = true;
}