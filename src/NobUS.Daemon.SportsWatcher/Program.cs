using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NobUS.Extra.Campus.Facility.Sports;
using Spectre.Console;

namespace NobUS.Daemon.SportsWatcher;

internal static class Program
{
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

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<FacilityParser>();
        services.AddSingleton<IFacilityParser>(provider =>
            provider.GetRequiredService<FacilityParser>()
        );
        return services.BuildServiceProvider();
    }

    private static Table CreateTable() =>
        new Table()
            .AddColumn("Report Time")
            .AddColumn("Location")
            .AddColumn("Type")
            .AddColumn("Load")
            .AddColumn("Occupancy");

    private static string GetTimeString() => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    private static async Task RunAsync(
        IServiceProvider provider,
        FileInfo outputFile,
        double interval,
        CancellationToken cancellationToken
    )
    {
        var parser = provider.GetRequiredService<IFacilityParser>();
        await using var stream = new FileStream(
            outputFile.FullName,
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read
        );
        await using var writer = new StreamWriter(stream, Encoding.UTF8);

        await WriteSnapshotAsync(parser, writer, cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(interval));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await WriteSnapshotAsync(parser, writer, cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            timer.Dispose();
        }
    }

    private static async Task WriteSnapshotAsync(
        IFacilityParser parser,
        StreamWriter writer,
        CancellationToken cancellationToken
    )
    {
        var facilities = await parser.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var timeString = GetTimeString();
        var table = CreateTable();

        foreach (var facility in facilities)
        {
            await writer
                .WriteLineAsync(
                    $"{timeString}, {facility.Type}, {facility.Name}, {facility.Load}, {facility.Occupancy:P}"
                )
                .ConfigureAwait(false);
            table.AddRow(
                timeString,
                facility.Name,
                facility.Type.ToString(),
                facility.Load.ToString(),
                facility.Occupancy.ToString("P")
            );
        }

        AnsiConsole.Write(table);
    }

    public static async Task<int> Main(string[] args)
    {
        using var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        using var provider = ConfigureServices();
        try
        {
            var parseResult = RootCommand.Parse(args);

            if (
                args.Any(arg =>
                    string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                AnsiConsole.MarkupLine($"[bold]{RootCommand.Description}[/]");
                AnsiConsole.MarkupLine("[bold]Options:[/]");
                AnsiConsole.MarkupLine(
                    "  [green]-o[/], [green]--output[/] <file>   The file to write the output to."
                );
                AnsiConsole.MarkupLine(
                    "  [green]-i[/], [green]--interval[/] <sec>  The interval in seconds to check the capacity."
                );
                return 0;
            }

            if (parseResult.Errors.Count > 0)
            {
                foreach (var error in parseResult.Errors)
                {
                    AnsiConsole.MarkupLine($"[red]{error.Message}[/]");
                }
                return 1;
            }

            var outputFile = parseResult.GetValue(OutputFile)!;
            var interval = parseResult.GetValue(Interval);
            await RunAsync(provider, outputFile, interval, cancellationSource.Token)
                .ConfigureAwait(false);
            return 0;
        }
        finally
        {
            cancellationSource.Cancel();
        }
    }
}
