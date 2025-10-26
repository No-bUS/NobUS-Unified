using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NobUS.Extra.Campus.Facility.Sports;

public record Facility(string Name, int Capacity, int Load, Type Type)
{
    public double Occupancy => (double)Load / Capacity;
}

public enum Type
{
    Gym,
    Pool,
    Other,
}

public static class Parser
{
    private static readonly string URL =
        "https://reboks.nus.edu.sg/nus_public_web/public/index.php/facilities/capacity";

    private static async Task<string> FetchAsync()
    {
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"
        );
        return await httpClient.GetStringAsync(URL);
    }

    private static Facility[] Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var swimmingPools = doc.DocumentNode.SelectSingleNode(
            "/html/div[2]/div/div/div[3]/section/div/div/div[1]"
        );
        var gyms = doc.DocumentNode.SelectSingleNode(
            "/html/div[2]/div/div/div[3]/section/div/div/div[2]"
        );
        var s = swimmingPools
            .SelectNodes("./div[@class=\"swimbox\"]")
            .Concat(gyms.SelectNodes("./div[@class=\"gymbox\"]"))
            .Select(e =>
            {
                var numString = e.SelectSingleNode("./b")
                    .InnerText.Split('/')
                    .Select(s => int.Parse(s))
                    .ToList();
                var rawName = e.SelectSingleNode("./span").InnerText;
                var type = rawName switch
                {
                    string s when s.ToLower().Contains("swimming pool") => Type.Pool,
                    string s when s.ToLower().Contains("gym") => Type.Gym,
                    _ => Type.Other,
                };
                return new Facility(rawName.Split('-')[0], numString[1], numString[0], type);
            });

        return s.ToArray();
    }

    public static async Task<Facility[]> GetAllAsync() => Parse(await FetchAsync());
}
