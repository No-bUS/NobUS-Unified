using System;
using System.Collections.Generic;
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

        var nodes = doc.DocumentNode.SelectNodes(
            "//div[contains(@class,'swimbox') or contains(@class,'gymbox')]"
        );

        if (nodes is null || nodes.Count == 0)
        {
            return Array.Empty<Facility>();
        }

        var facilities = new List<Facility>();

        foreach (var node in nodes)
        {
            var rawName = node.SelectSingleNode("./span")?.InnerText;
            var numbers = node.SelectSingleNode("./b")?.InnerText;

            if (string.IsNullOrWhiteSpace(rawName) || string.IsNullOrWhiteSpace(numbers))
            {
                continue;
            }

            var splits = numbers
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (
                splits.Length != 2
                || !int.TryParse(splits[0], out int load)
                || !int.TryParse(splits[1], out int capacity)
            )
            {
                continue;
            }

            string trimmedName = rawName.Split('-')[0].Trim();
            string classes = node.GetAttributeValue("class", string.Empty);

            Type facilityType = classes.Contains("swimbox", StringComparison.OrdinalIgnoreCase)
                ? Type.Pool
                : classes.Contains("gymbox", StringComparison.OrdinalIgnoreCase)
                    ? Type.Gym
                    : rawName.Contains("pool", StringComparison.OrdinalIgnoreCase)
                        ? Type.Pool
                        : rawName.Contains("gym", StringComparison.OrdinalIgnoreCase)
                            ? Type.Gym
                            : Type.Other;

            facilities.Add(new Facility(trimmedName, capacity, load, facilityType));
        }

        return facilities.ToArray();
    }

    public static async Task<Facility[]> GetAllAsync() =>
        Parse(await FetchAsync());
}
