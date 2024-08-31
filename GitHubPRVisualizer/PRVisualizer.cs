using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitHubPRVisualizer;

public class PRVisualizer {
    private const int LAST_WEEK = -7;
    
    private readonly string _owner;
    private readonly string _repo;
    private readonly GitHubClient _gitHubClient;

    public PRVisualizer(string repository, string token) {
        string[] parts = repository.Split('/');
        if (parts.Length != 2) {
            throw new ArgumentException("Repository must be in the format 'owner/repo'.");
        }

        _owner = parts[0];
        _repo = parts[1];
        _gitHubClient = new GitHubClient(token);
    }

    public async Task ShowWeekStats() {
        Console.Write("Fetching PRs opened in the last week...");
        var openedPRs = await PrintDotsWhileWaiting(GetPRs(createdAfter: DateTime.UtcNow.AddDays(LAST_WEEK)));
        Console.WriteLine($"PRs opened in the last week: {openedPRs.Count}");

        Console.Write("Fetching PRs closed in the last week...");
        var closedPRs = await PrintDotsWhileWaiting(GetPRs(state: "closed", closedAfter: DateTime.UtcNow.AddDays(LAST_WEEK)));
        Console.WriteLine($"PRs closed in the last week: {closedPRs.Count}");
    }

    public async Task ShowStuckPRs(int thresholdDays = 7) {
        Console.Write($"Fetching open PRs stuck for more than {thresholdDays} days...");
        var openPRs = await PrintDotsWhileWaiting(GetPRs(state: "open"));
        var stuckPRs = openPRs.Where(pr => (DateTime.UtcNow - pr.UpdatedAt).TotalDays > thresholdDays).ToList();

        Console.WriteLine($"PRs stuck for more than {thresholdDays} days: {stuckPRs.Count}");
        foreach (var pr in stuckPRs) {
            Console.WriteLine($"- #{pr.Number} {pr.Title} (Last updated: {pr.UpdatedAt.ToShortDateString()})");
        }
    }

    private async Task<List<PullRequest>> GetPRs(string state = "all", DateTime? createdAfter = null, DateTime? closedAfter = null) {
        var prs = new List<PullRequest>();
        int page = 1;
        const int perPage = 100;
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        do {
            string url = $"https://api.github.com/repos/{_owner}/{_repo}/pulls?state={state}&per_page={perPage}&page={page}";
            string response = await _gitHubClient.GetAsync(url);
            var pagePRs = JsonSerializer.Deserialize<List<PullRequest>>(response, jsonSerializerOptions);

            if (pagePRs!.Count == 0) {
                break;
            }

            prs.AddRange(pagePRs);
            if (pagePRs.Count < perPage) {
                break;
            }

            page++;
        } while (true);

        // Filter based on creation or closing date
        if (createdAfter.HasValue) {
            prs = prs.Where(pr => pr.CreatedAt >= createdAfter.Value).ToList();
        }

        if (closedAfter.HasValue) {
            prs = prs.Where(pr => pr.ClosedAt.HasValue && pr.ClosedAt.Value >= closedAfter.Value).ToList();
        }

        return prs;
    }

    private static async Task<T> PrintDotsWhileWaiting<T>(Task<T> taskToWaitFor) {
        // CancellationTokenSource to cancel dot printing if the task completes
        using var cts = new CancellationTokenSource();
        var printTask = Task.Run(async () => {
            while (!taskToWaitFor.IsCompleted) {
                Console.Write('.');
                try {
                    await Task.Delay(3000, cts.Token); // Wait for 3 seconds
                }
                catch (TaskCanceledException) {
                    // Ignored
                }
            }
        }, cts.Token);

        // Await the task we're waiting for
        var result = await taskToWaitFor;

        // Cancel the printing task when the main task is done
        await cts.CancelAsync();
        await printTask; // Ensure the printing task completes
        
        Console.WriteLine(); // Pretty end line character
        
        return result;
    }
}

internal class PullRequest {
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}