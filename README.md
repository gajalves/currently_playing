## currently_playing

A C# console application that automatically updates your GitHub profile with the song you're currently listening to on Spotify.

## Features

- Real-time monitoring of your Spotify current playing track
- Automatic GitHub profile README updates
- Displays album cover and current song information
- Maintains your GitHub stats in the README

## Prerequisites

- .NET 6.0 or higher
- Spotify Account
- GitHub Account
- Visual Studio 2022 or VS Code

## Spotify Setup

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Login with your Spotify account
3. Click "Create an App"
4. Fill in:
   - App name: (your choice)
   - App description: (your choice)
5. After creation, go to "Edit Settings"
6. In "Redirect URIs", add: `http://localhost:5000/callback`
7. Save changes
8. Save your "Client ID" and "Client Secret"

## GitHub Setup

1. Go to [GitHub Settings > Developer settings](https://github.com/settings/developer)
2. Navigate to "Personal access tokens" > "Tokens (classic)"
3. Generate new token with:
   - `repo` permission (full repository access)
4. Copy and save the generated token

## Usage

1. Clone the repository:
```bash
git clone https://github.com/your-username/currently-playing.git
```

2. Configure Spotify:
```bash
> -s spotify
> -id your_spotify_client_id
> -secret your_spotify_client_secret
> -save
> -auth
```

3. Configure GitHub:
```bash
> -s github
> -user your_github_username
> -token your_github_token
> -save
```

4. Run the application:
```bash
> -r
```

## Available Commands

- `-s <service>` - Enter configuration mode (spotify or github)
- `-id` - Set Spotify Client ID
- `-secret` - Set Spotify Client Secret
- `-auth` - Start Spotify authorization flow
- `-user` - Set GitHub username
- `-token` - Set GitHub token
- `-save` - Save current configuration
- `-r` - Run application
- `exit` - Exit application

## Project Structure

- `Program.cs` - Entry point and main loop
- `SpotifyService.cs` - Spotify integration
- `GithubService.cs` - GitHub integration
- `ConfigManager.cs` - Configuration management
- `AppConfig.cs` - Configuration classes
- `Banner.cs` - User interface
- `Global.cs` - Global constants
- `MessageService.cs` - Logging service

## How It Works

1. Application monitors your Spotify account every 10 seconds
2. When it detects a new song, updates your GitHub README
3. README is updated with:
   - Your GitHub stats
   - Currently playing song
   - Album cover
   - Artist name

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
