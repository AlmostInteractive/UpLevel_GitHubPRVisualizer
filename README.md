# GitHub PR Visualizer

## Usage

Usage: `GitHubPRVisualizer <repository> <token> [options]`
 
```
Arguments:
<repository>  GitHub repository in the format 'owner/repo'  
<token>       GitHub Personal Access Token```

Options:
--week-stats             Show the number of PRs opened/closed in the last week
--stuck-prs <stuck-prs>  Show PRs stuck in review for more than a specified number of days)
--version                Show version information
-?, -h, --help           Show help and usage information
```

## Obtaining a GitHub Personal Access Token (PAT)

- Go to GitHub Settings.
- Click on "Generate new token".
- Select the necessary scopes: `repo: To access repository data.`
- Generate the token and copy it immediately. You won't be able to see it again.