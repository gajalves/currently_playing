﻿namespace currently_playing;
public class SpotifyConfig
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RefreshToken { get; set; }
    public string AccessToken { get; set; }
}

public class GitHubConfig
{
    public string User { get; set; }
    public string Token { get; set; }
}

public class AppConfig
{
    public SpotifyConfig Spotify { get; set; }
    public GitHubConfig GitHub { get; set; }
}
