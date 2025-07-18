﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using System.Text;

namespace currently_playing;
internal class SpotifyService
{
    private const string SpotifyAuthUrl = "https://accounts.spotify.com/authorize";
    private const string SpotifyTokenUrl = "https://accounts.spotify.com/api/token";
    private static readonly string[] Scopes = { "user-read-private", "user-read-email", "user-read-currently-playing" };
    private readonly HttpClient _httpClient;

    #region Class
    public class RefreshTokenObject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class CurrentlyPlayingObject
    {
        public Item item { get; set; }
    }

    public class Item
    {
        public string id { get; set; }

        public string name { get; set; }

        public List<Artist> artists { get; set; }

        public Album album { get; set; }

        public Item()
        {
            artists = new List<Artist>();
        }
    }

    public class Artist
    {
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class Album
    {
        public string name { get; set; }

        public List<Images> images { get; set; }

        public Album()
        {
            images = new List<Images>();
        }
    }

    public class Images
    {
        public int height { get; set; }

        public string url { get; set; }
    }

    public class Spotify
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public string clientID { get; set; }

        public string clientSecret { get; set; }

        public string idCurrentlySong { get; set; }

        public string newDescription { get; set; }

        public string newUrlAlbumImage { get; set; }

        public bool updateDescription { get; set; }
    }

    public string GetAccessToken()
    {
        return spotify.access_token;
    }

    public string GetRefreshToken()
    {
        return spotify.refresh_token;
    }

    public string GetNewDescription()
    {
        return spotify.newDescription;
    }

    public string GetNewImageAlbumUrl()
    {
        return spotify.newUrlAlbumImage;
    }

    public void SetUpdateDescription(bool value)
    {
        spotify.updateDescription = value;
    }

    public bool ShouldUpdateDescription()
    {
        return spotify.updateDescription;
    }
    
    private class TokenResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }
    #endregion

    Spotify spotify = null;

    public SpotifyService(AppConfig config, HttpClient httpClient)
    {
        _httpClient = httpClient;
        spotify = new Spotify();
        spotify.clientID = config.Spotify.ClientId;
        spotify.clientSecret = config.Spotify.ClientSecret;
        spotify.refresh_token = config.Spotify.RefreshToken;
    }

    public async Task<int> InitiateAuthorizationFlow()
    {
        if (string.IsNullOrEmpty(spotify.clientID) || string.IsNullOrEmpty(spotify.clientSecret))
        {
            MessageService.LogMessage("Error: Spotify Client ID and Client Secret must be configured first.");
            MessageService.LogMessage("Use '-id' to set Client ID and '-secret' to set Client Secret.");
            return Global.RequestCanceled;
        }

        var state = Guid.NewGuid().ToString("N");
        var redirectUri = "http://localhost:5000/callback";

        var authUrl = $"{SpotifyAuthUrl}?response_type=code" +
            $"&client_id={spotify.clientID}" +
            $"&scope={string.Join("%20", Scopes)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&state={state}";

        Console.WriteLine("\nOpen the following URL in your browser to authorize the application:");
        Console.WriteLine(authUrl);
        Console.WriteLine("");

        MessageService.LogMessage("After authorizing, paste the complete redirect URL here:");
        var redirectUrl = Console.ReadLine();

        if (string.IsNullOrEmpty(redirectUrl))
        {
            Console.WriteLine("Invalid redirect URL.");
            return Global.RequestCanceled;
        }

        var queryParams = HttpUtility.ParseQueryString(new Uri(redirectUrl).Query);
        var authCode = queryParams["code"];
        var returnedState = queryParams["state"];

        if (string.IsNullOrEmpty(authCode))
        {
            Console.WriteLine("Authorization code not found in URL.");
            return Global.RequestCanceled;
        }

        if (returnedState != state)
        {
            Console.WriteLine("Invalid state. Possible CSRF attack.");
            return Global.RequestCanceled;
        }

        return await ExchangeCodeForTokenAsync(authCode, redirectUri);
         
    }

    private async Task<int> ExchangeCodeForTokenAsync(string authCode, string redirectUri)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        _httpClient.DefaultRequestHeaders.Clear();
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{spotify.clientID}:{spotify.clientSecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await _httpClient.PostAsync(SpotifyTokenUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);
            if (tokenResponse != null)
            {
                spotify.access_token = tokenResponse.access_token;
                spotify.refresh_token = tokenResponse.refresh_token;
                MessageService.LogMessage("Authorization successful! New tokens received.");
                return Global.RequestComplete;
            }
            else
            {
                Console.WriteLine("Failed to deserialize token response.");
                return Global.RequestCanceled;
            }
        }
        else
        {
            Console.WriteLine($"Token exchange failed: {response.StatusCode} - {responseContent}");
            return Global.RequestCanceled;
        }
    }
    
    public async Task RefreshToken()
    {
        try
        {
            var endPoint = "https://accounts.spotify.com/api/token";
            var form = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", spotify.refresh_token }
            };

            _httpClient.DefaultRequestHeaders.Clear();

            var CredenciaisAcesso = spotify.clientID + ":" + spotify.clientSecret;
            byte[] encodedByte = System.Text.ASCIIEncoding.ASCII.GetBytes(CredenciaisAcesso);
            var BasicAuth = Convert.ToBase64String(encodedByte);

            _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + BasicAuth);

            var response = await _httpClient.PostAsync(endPoint, new FormUrlEncodedContent(form));
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenObject = JsonSerializer.Deserialize<RefreshTokenObject>(content);
                if (tokenObject != null)
                {
                    spotify.access_token = tokenObject.access_token;
                    MessageService.LogMessage("Access token refreshed successfully.");
                }
                else
                {
                    MessageService.LogMessage("Error refreshing token: Response object is null.");
                }
            }
            else
            {
                MessageService.LogMessage($"Error refreshing token: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            MessageService.LogMessage($"Exception during token refresh: {ex.Message}");
        }
    }

    public async Task<int> CurrentlyPlaying()
    {
        int result = Global.RequestComplete;

        try
        {
            var endPoint = "https://api.spotify.com/v1/me/player/currently-playing";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", spotify.access_token);

            var response = await _httpClient.GetAsync(endPoint);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    MessageService.LogMessage("No music playing at the moment.");
                }
                else
                {
                    CurrentlyPlayingObject songObject = JsonSerializer.Deserialize<CurrentlyPlayingObject>(content);
                    if (songObject != null)
                    {
                        if (spotify.idCurrentlySong != songObject.item.id)
                        {
                            spotify.idCurrentlySong = songObject.item.id;

                            var artistas = string.Empty;
                            foreach (var artis in songObject.item.artists)
                            {
                                artistas += artis.name + ", ";
                            }

                            var image = songObject.item.album.images.OrderByDescending(img => img.height).FirstOrDefault();
                            if (image != null)
                                spotify.newUrlAlbumImage = image.url;

                            spotify.newDescription = $"{songObject.item.name} - {artistas.Remove(artistas.Length - 2, 2)}.";
                            spotify.updateDescription = true;

                            MessageService.LogMessage($"Now playing: {spotify.newDescription}");
                        }
                    }
                    else
                    {
                        MessageService.LogMessage("Error retrieving currently playing song: Response object is null.");
                        result = Global.RequestCanceled;
                    }
                }
            }
            else
            {
                MessageService.LogMessage($"Error retrieving currently playing song: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
                
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    spotify.access_token = string.Empty;

                result = Global.RequestCanceled;
            }
        }
        catch (Exception ex)
        {
            MessageService.LogMessage($"Exception during currently playing request: {ex.Message}");
            result = Global.RequestCanceled;
        }

        return result;
    }
}
