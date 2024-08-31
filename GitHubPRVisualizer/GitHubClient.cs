using System.Net.Http.Headers;

namespace GitHubPRVisualizer;

public class GitHubClient : IDisposable {
    private readonly HttpClient _httpClient;

    public GitHubClient(string token) {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubPRVisualizer", "1.0.0"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
    }

    public async Task<string> GetAsync(string url) {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) {
            throw new Exception($"GitHub API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose() { _httpClient.Dispose(); }
}