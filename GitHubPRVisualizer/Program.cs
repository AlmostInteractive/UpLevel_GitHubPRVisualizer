using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace GitHubPRVisualizer;

internal static class Program {
    private static async Task<int> Main(string[] args) {
        ;
        var rootCommand = new RootCommand {
            new Argument<string>("repository", "GitHub repository in the format 'owner/repo'"),
            new Argument<string>("token", "GitHub Personal Access Token (PAT)"),
            new Option<bool>("--week-stats", "Show the number of PRs opened/closed in the last week"),
            new Option<int>("--stuck-prs", "Show PRs stuck in review for more than a specified number of days)")
        };

        rootCommand.Description = "GitHub Pull Request Visualizer";

        rootCommand.Handler = CommandHandler.Create<string, string, bool, int>(async (repository, token, weekStats, stuckPrs) => {
            try {
                var prVisualizer = new PRVisualizer(repository, token);

                if (weekStats) {
                    await prVisualizer.ShowWeekStats();
                }

                if (stuckPrs >= 0) {
                    await prVisualizer.ShowStuckPRs(stuckPrs);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });

        return await rootCommand.InvokeAsync(args);
    }
}