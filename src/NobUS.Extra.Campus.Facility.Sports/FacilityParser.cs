using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

public interface IFacilityParser
{
    Task<IReadOnlyList<Facility>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class FacilityParser(HttpClient httpClient) : IFacilityParser
{
    private static readonly Uri Url = new(
        "https://reboks.nus.edu.sg/nus_public_web/public/index.php/facilities/capacity"
    );

    public async Task<IReadOnlyList<Facility>> GetAllAsync(
        CancellationToken cancellationToken = default
    ) => Parse(await FetchAsync(cancellationToken).ConfigureAwait(false));

    private async Task<string> FetchAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Url);
        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"
        );

        using var response = await httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<Facility> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var swimmingPools = doc.DocumentNode.SelectSingleNode(
            "/html/div[2]/div/div/div[3]/section/div/div/div[1]"
        );
        var gyms = doc.DocumentNode.SelectSingleNode(
            "/html/div[2]/div/div/div[3]/section/div/div/div[2]"
        );

        var facilities = (
            swimmingPools?.SelectNodes("./div[@class=\"swimbox\"]") ?? Enumerable.Empty<HtmlNode>()
        )
            .Concat(gyms?.SelectNodes("./div[@class=\"gymbox\"]") ?? Enumerable.Empty<HtmlNode>())
            .Select(node =>
            {
                var counts = node.SelectSingleNode("./b")
                    .InnerText.Split('/')
                    .Select(int.Parse)
                    .ToArray();
                var rawName = node.SelectSingleNode("./span").InnerText;
                var type = rawName.ToLowerInvariant() switch
                {
                    var name when name.Contains("swimming pool") => Type.Pool,
                    var name when name.Contains("gym") => Type.Gym,
                    _ => Type.Other,
                };

                return new Facility(rawName.Split('-')[0], counts[1], counts[0], type);
            })
            .ToArray();

        return facilities;
    }
}
