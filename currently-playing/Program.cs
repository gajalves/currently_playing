﻿﻿﻿﻿﻿using currently_playing;

class Program
{
    private enum Context { Main, Spotify, GitHub }

    static async Task Main(string[] args)
    {
        Console.SetWindowSize(Console.WindowWidth, 45);

        Banner.Print();
        Banner.Help();

        var config = ConfigManager.LoadConfig() ?? new AppConfig();
        var currentContext = Context.Main;

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var inputArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (currentContext)
            {
                case Context.Main:
                    currentContext = await HandleMainContext(inputArgs, config, currentContext);
                    break;

                case Context.Spotify:
                    currentContext = await HandleSpotifyContext(inputArgs, config, currentContext);
                    break;

                case Context.GitHub:
                    currentContext = HandleGitHubContext(inputArgs, config, currentContext);
                    break;
            }
        }
    }

    private static async Task<Context> HandleMainContext(string[] inputArgs, AppConfig config, Context currentContext)
    {
        switch (inputArgs[0])
        {
            case "-r":
                Console.WriteLine("Running application...");
                await RunProcessingLoop(config);
                return currentContext;

            case "-s":
                if (inputArgs.Length > 1)
                {
                    if (inputArgs[1] == "spotify")
                    {
                        Console.WriteLine("Entering Spotify configuration mode. Type 'exit' to return.");
                        return Context.Spotify;
                    }
                    else if (inputArgs[1] == "github")
                    {
                        Console.WriteLine("Entering GitHub configuration mode. Type 'exit' to return.");
                        return Context.GitHub;
                    }
                    else
                    {
                        Console.WriteLine("Invalid setup option. Use 'spotify' or 'github'.");
                    }
                }
                else
                {
                    Console.WriteLine("Missing setup option. Use 'spotify' or 'github'.");
                }
                return currentContext;

            case "exit":
                Console.WriteLine("Exiting application...");
                Environment.Exit(0);
                return currentContext;

            default:
                Console.WriteLine($"Unknown command: {inputArgs[0]}");
                Banner.Help();
                return currentContext;
        }
    }

    private static async Task<Context> HandleSpotifyContext(string[] inputArgs, AppConfig config, Context currentContext)
    {
        switch (inputArgs[0])
        {
            case "-id":
                if (inputArgs.Length > 1)
                {
                    config.Spotify ??= new SpotifyConfig();
                    config.Spotify.ClientId = inputArgs[1];
                    Console.WriteLine($"Spotify Client ID: {config.Spotify.ClientId}");
                }
                else
                {
                    Console.WriteLine("Missing Spotify Client ID.");
                }
                return currentContext;

            case "-secret":
                if (inputArgs.Length > 1)
                {
                    config.Spotify ??= new SpotifyConfig();
                    config.Spotify.ClientSecret = inputArgs[1];
                    Console.WriteLine($"Spotify Client Secret: {config.Spotify.ClientSecret}");
                }
                else
                {
                    Console.WriteLine("Missing Spotify Client Secret.");
                }
                return currentContext;

            case "-save":
                ConfigManager.SaveConfig(config);
                Console.WriteLine("Spotify configuration saved.");
                return currentContext;

            case "exit":
                Console.WriteLine("Exiting Spotify configuration mode.");
                return Context.Main;

            case "-auth":
                try
                {
                    var httpClient = new HttpClient();
                    var spotifyService = new SpotifyService(config, httpClient);
                    var result = await spotifyService.InitiateAuthorizationFlow();
                    if (result == Global.RequestCanceled)
                        return currentContext;

                    config.Spotify.AccessToken = spotifyService.GetAccessToken();
                    config.Spotify.RefreshToken = spotifyService.GetRefreshToken();
                    ConfigManager.SaveConfig(config);
                    Console.WriteLine("Spotify authorization completed and configuration saved.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during authorization: {ex.Message}");
                }
                return currentContext;

            default:
                Console.WriteLine($"Unknown command: {inputArgs[0]}");
                Console.WriteLine("Valid commands: -id, -secret, -auth, -save, exit");
                return currentContext;
        }
    }

    private static Context HandleGitHubContext(string[] inputArgs, AppConfig config, Context currentContext)
    {
        switch (inputArgs[0])
        {
            case "-user":
                if (inputArgs.Length > 1)
                {
                    config.GitHub ??= new GitHubConfig();
                    config.GitHub.User = inputArgs[1];
                    Console.WriteLine($"GitHub User: {config.GitHub.User}");
                }
                else
                {
                    Console.WriteLine("Missing GitHub User.");
                }
                return currentContext;

            case "-token":
                if (inputArgs.Length > 1)
                {
                    config.GitHub ??= new GitHubConfig();
                    config.GitHub.Token = inputArgs[1];
                    Console.WriteLine($"GitHub Token: {config.GitHub.Token}");
                }
                else
                {
                    Console.WriteLine("Missing GitHub Token.");
                }
                return currentContext;

            case "-save":
                ConfigManager.SaveConfig(config);
                Console.WriteLine("GitHub configuration saved.");
                return currentContext;

            case "exit":
                Console.WriteLine("Exiting GitHub configuration mode.");
                return Context.Main;

            default:
                Console.WriteLine($"Unknown command: {inputArgs[0]}");
                Console.WriteLine("Valid commands: -user, -token, -save, exit");
                return currentContext;
        }
    }

    private static async Task RunProcessingLoop(AppConfig config)
    {
        var httpClient = new HttpClient();
        var spotifyService = new SpotifyService(config, httpClient);
        var githubService = new GithubService(config, httpClient);

        while (true)
        {
            await HandleSpotifyTokenRefresh(spotifyService);
            await HandleCurrentlyPlaying(spotifyService, githubService);
            await Task.Delay(10000); 
        }
    }

    private static async Task HandleSpotifyTokenRefresh(SpotifyService spotifyService)
    {
        if (string.IsNullOrEmpty(spotifyService.GetAccessToken()))
        {
            await spotifyService.RefreshToken();
        }
    }

    private static async Task HandleCurrentlyPlaying(SpotifyService spotifyService, GithubService githubService)
    {
        int result = await spotifyService.CurrentlyPlaying();
        if (result == Global.RequestComplete)
        {
            await HandleGithubUpdate(spotifyService, githubService);
        }
    }

    private static async Task HandleGithubUpdate(SpotifyService spotifyService, GithubService githubService)
    {
        int result = await githubService.GetRepoFiles();
        if (result == Global.RequestComplete && spotifyService.ShouldUpdateDescription())
        {
            result = await githubService.UpdateRepoFile(
                spotifyService.GetNewDescription(),
                spotifyService.GetNewImageAlbumUrl()
            );

            if (result == Global.RequestComplete)
            {
                spotifyService.SetUpdateDescription(false);
            }
        }
    }
}
