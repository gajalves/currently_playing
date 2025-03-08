﻿using System.Text.Json;

namespace currently_playing;
internal static class ConfigManager
{
    private static readonly string ConfigFilePath = "config.json";

    public static AppConfig LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
        {
            Console.WriteLine("Configuration file not found. Creating a new one...");
            return new AppConfig();
        }

        try
        {
            string json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return null;
        }
    }

    public static void SaveConfig(AppConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
            Console.WriteLine("Configuration saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }

    public static string GetSpotifyClientId()
    {
        var config = LoadConfig();
        return config?.Spotify?.ClientId;
    }

    public static string GetSpotifyClientSecret()
    {
        var config = LoadConfig();
        return config?.Spotify?.ClientSecret;
    }

    public static string GetSpotifyAccessToken()
    {
        var config = LoadConfig();
        return config?.Spotify?.AccessToken;
    }

    public static string GetSpotifyRefreshToken()
    {
        var config = LoadConfig();
        return config?.Spotify?.RefreshToken;
    }

    public static string GetSpotifyRedirectUri()
    {
        return "http://localhost:5000/callback";
    }

    public static async Task UpdateSpotifyTokens(string accessToken, string refreshToken)
    {
        var config = LoadConfig() ?? new AppConfig();
        config.Spotify ??= new SpotifyConfig();
        config.Spotify.AccessToken = accessToken;
        config.Spotify.RefreshToken = refreshToken;
        SaveConfig(config);
    }
}
