using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace GitHubPRVisualizer;

internal static class Program {
    private static async Task<int> Main(string[] args) {
        // we'll use this one in the validator below
        Option stuckPrsOption = new Option<int>("--stuck-prs",
            description: "Show PRs stuck in review for more than a specified number of days)");
        
        var rootCommand = new RootCommand {
            new Argument<string>("repository", "GitHub repository in the format 'owner/repo'"),
            new Argument<string>("token", "GitHub Personal Access Token"),
            new Option<bool>("--week-stats", "Show the number of PRs opened/closed in the last week"),
            stuckPrsOption
        };

        rootCommand.Description = "GitHub Pull Request Visualizer";

        rootCommand.AddValidator(result => {
            try {
                var optionResult = result.FindResultFor(stuckPrsOption);
                if (optionResult != null && optionResult.GetValueOrDefault<int>() < 0) {
                    result.ErrorMessage = $"The value for stuck-prs cannot be negative, received {optionResult.GetValueOrDefault<int>()}";
                }
            }
            catch (Exception _) {
                // prevents a crash but it's unnecessary to handle the exception, it's already caught and handled nicely 
            }
        });

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