using System;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NobUS.Extra.Campus.Facility.Sports;
using Spectre.Console;

namespace NobUS.Daemon.SportsWatcher;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCmd = RootCommand;
        rootCmd.SetHandler(
            async (FileInfo outputFile, double interval) =>
            {
                while (true)
                {
                    var sb = new StringBuilder(200);
                    var table = EmptyTable;

                    var facilities = await Parser.GetAllAsync();
                    var timeString = GetTimeString();

                    foreach (var facility in facilities)
                    {
                        sb.AppendLine(
                            $"{timeString}, {facility.Type}, {facility.Name}, "
                                + $"{facility.Load}, {facility.Occupancy:P}"
                        );
                        table.AddRow(
                            timeString,
                            facility.Name,
                            facility.Type.ToString(),
                            facility.Load.ToString(),
                            facility.Occupancy.ToString()
                        );
                    }
                    File.AppendAllText(outputFile.FullName, sb.ToString());
                    AnsiConsole.Write(table);

                    GC.Collect(2, GCCollectionMode.Aggressive, true, true);
                    await Task.Delay((int)(interval * 1000));
                }
            },
            OutputFile,
            Interval
        );
        return await rootCmd.InvokeAsync(args);
    }

    private static string GetTimeString() => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    private static Table EmptyTable =>
        new Table()
            .AddColumn("Report Time")
            .AddColumn("Location")
            .AddColumn("Type")
            .AddColumn("Load")
            .AddColumn("Occupancy");

    private static readonly Option<FileInfo> OutputFile =
        new(
            aliases: ["--output", "-o"],
            description: "The file to write the output to.",
            getDefaultValue: () => new FileInfo($"Sports-{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv")
        );

    private static readonly Option<double> Interval =
        new(
            aliases: ["--interval", "-i"],
            description: "The interval in seconds to check the capacity.",
            getDefaultValue: () => 60
        );

    private static readonly RootCommand RootCommand =
        new("Logs NUS sport facility capacity info.") { OutputFile, Interval };
}
