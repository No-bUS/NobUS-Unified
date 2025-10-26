using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace NobUS.Daemon.SportsWatcher;

class Program
{
    static Program()
    {
        RootCommand.SetAction(HandleCommandAsync);
    }

    static async Task<int> Main(string[] args)
    {
        var parseResult = RootCommand.Parse(args, new ParserConfiguration());
        return await parseResult.InvokeAsync(new InvocationConfiguration());
    }

    private static async Task<int> HandleCommandAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken
    )
    {
        var outputFile = parseResult.GetValue(OutputFile)!;
        var interval = parseResult.GetValue(Interval);

        while (!cancellationToken.IsCancellationRequested)
        {
            var sb = new StringBuilder(200);
            var table = EmptyTable;

            var facilities = await Extra
                .Campus.Facility.Sports.Parser.GetAllAsync()
                .ConfigureAwait(false);
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

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return 0;
    }

    private static string GetTimeString() => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    private static Table EmptyTable =>
        new Table()
            .AddColumn("Report Time")
            .AddColumn("Location")
            .AddColumn("Type")
            .AddColumn("Load")
            .AddColumn("Occupancy");

    private static readonly Option<FileInfo> OutputFile = new("--output", ["-o"])
    {
        Description = "The file to write the output to.",
        DefaultValueFactory = _ => new FileInfo($"Sports-{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv"),
    };

    private static readonly Option<double> Interval = new("--interval", ["-i"])
    {
        Description = "The interval in seconds to check the capacity.",
        DefaultValueFactory = _ => 60d,
    };

    private static readonly RootCommand RootCommand = new("Logs NUS sport facility capacity info.")
    {
        OutputFile,
        Interval,
    };
}
