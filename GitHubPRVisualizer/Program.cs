using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace GitHubPRVisualizer;

internal static class Program {
    private const int DEFAULT_STUCK_PRS_DAYS = 7;
        
    private static async Task<int> Main(string[] args) {
        bool runStuckPrs = false;

        var weekStatsOption = new Option<bool>("--week-stats", "Show the number of PRs opened/closed in the last week");
        
        var stuckPrsOption = new Option<int?>("--stuck-prs",
            description: "Show PRs stuck in review for more than a specified number of days (defaults to 7 days)") {
            Arity = ArgumentArity.ZeroOrOne
        };
        
        stuckPrsOption.AddValidator(result => {
            try {
                runStuckPrs = true;
                if (result.Tokens.Count == 1 && result.GetValueOrDefault<int>() < 0) {
                    result.ErrorMessage = $"The value for stuck-prs cannot be negative, received {result.GetValueOrDefault<int>()}";
                }
            }
            catch (Exception ex) {
                result.ErrorMessage = ex.Message;
            }
        });

        var rootCommand = new RootCommand {
            new Argument<string>("repository", "GitHub repository in the format 'owner/repo'"),
            new Argument<string>("token", "GitHub Personal Access Token"),
            weekStatsOption,
            stuckPrsOption
        };

        rootCommand.Description = "GitHub Pull Request Visualizer";
        
        rootCommand.AddValidator(result => {
            if (result.FindResultFor(weekStatsOption) == null && result.FindResultFor(stuckPrsOption) == null) {
                result.ErrorMessage = "No commands given.  Expected one or both: --week-stats --stuck-prs (days)";
            }
        });

        rootCommand.Handler = CommandHandler.Create<string, string, bool, int?>(async (repository, token, weekStats, stuckPrs) => {
            try {
                var prVisualizer = new PRVisualizer(repository, token);

                if (weekStats) {
                    await prVisualizer.ShowWeekStats();
                }

                if (runStuckPrs) {
                    await prVisualizer.ShowStuckPRs(stuckPrs ?? DEFAULT_STUCK_PRS_DAYS);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });

        return await rootCommand.InvokeAsync(args);
    }
}