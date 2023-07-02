using System.Net.Http.Headers;
using System.Text;
using NobUS.DataContract.Reader.OfficialAPI.Client;
namespace NobUS.DataContract.Reader.OfficialAPI
{
  internal static class Utility
  {
    public static HttpClient GetHttpClientWithAuth()
    {
      var httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes("NUSnextbus:13dL?zY,3feWR^\"T")));
      return httpClient;
    }
  }
}
