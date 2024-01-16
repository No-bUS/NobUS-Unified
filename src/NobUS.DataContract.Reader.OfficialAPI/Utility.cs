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
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes("NUSnextbus:13dL?zY,3feWR^\"T"))
            );
            return httpClient;
        }

        public static IEnumerable<_etas> GetEtasFromShuttles(IEnumerable<Shuttles> shuttles) =>
            shuttles
                .Where(ss => ss != null)
                .Where(ss => ss._etas != null)
                .Where(ss => ss.Busstopcode[^2..] != "-E")
                .Where(ss => ss._etas.Count != 0)
                .SelectMany(ss => ss._etas)
                .Where(x => x != null)
                .Distinct();

        public static IEnumerable<(
            string RouteName,
            ICollection<_etas> _etas
        )> GetRouteNameAndEtasFromShuttles(IEnumerable<Shuttles> shuttles) =>
            shuttles
                .Where(ss => ss != null)
                .Where(ss => ss._etas != null)
                .Where(ss => ss.Busstopcode[^2..] != "-E")
                .Where(ss => ss._etas.Count != 0)
                .Select(ss => (ss.Name, ss._etas));
    }
}
