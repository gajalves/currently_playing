using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace currently_playing;
internal class GithubService
{
    private readonly HttpClient _httpClient;

    public string shaFile { get; set; }
    public const string VictoryHand = ":v:";
    public const string MusicalNote = ":musical_note:";
    public string BaseUrl = $"https://api.github.com/repos/";

    #region Class
    public class GitHubRepositoryFiles
    {
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string git_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
    }

    public class UpdateRepoFIleContentBody
    {
        public string message { get; set; }
        public Committer committer { get; set; }
        public string content { get; set; }
        public string sha { get; set; }
    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class GitHub
    {
        public string token { get; set; }
        public string user { get; set; }
    }
    #endregion

    GitHub gitHub = null;

    public GithubService(AppConfig config, HttpClient httpClient)
    {
        _httpClient = httpClient;
        gitHub = new GitHub();
        gitHub.token = config.GitHub.Token;
        gitHub.user = config.GitHub.User;
    }

    public async Task<int> GetRepoFiles()
    {
        int result = Global.RequestComplete;
        try
        {
            var endPoint = BaseUrl + $"{gitHub.user}/{gitHub.user}/contents/";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gitHub.token);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await _httpClient.GetAsync(endPoint);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var fileList = JsonSerializer.Deserialize<List<GitHubRepositoryFiles>>(content);
                if (fileList != null && fileList.Count > 0)
                {
                    shaFile = fileList[0].sha;
                }
                else
                {
                    MessageService.LogMessage($"{nameof(GetRepoFiles)} - Deserialization returned null or empty list.");
                    result = Global.RequestCanceled;
                }
            }
            else
            {
                MessageService.LogMessage($"{nameof(GetRepoFiles)} - Request failed with code {(int)response.StatusCode}");
                result = Global.RequestCanceled;
            }
        }
        catch (Exception ex)
        {
            MessageService.LogMessage($"{nameof(GetRepoFiles)} - Exception: {ex.Message}");
            result = Global.RequestCanceled;
        }

        return result;
    }

    public async Task<int> UpdateRepoFile(string description, string urlImage)
    {
        int result = Global.RequestComplete;
        try
        {
            var endPoint = BaseUrl + $"{gitHub.user}/{gitHub.user}/contents/README.md";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gitHub.token);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var body = CreateContent(description, urlImage);
            var bodyContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endPoint, bodyContent);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                MessageService.LogMessage($"{nameof(UpdateRepoFile)} - Description updated successfully.");
            }
            else
            {
                MessageService.LogMessage($"{nameof(UpdateRepoFile)} - Request failed with code {(int)response.StatusCode}");
                result = Global.RequestCanceled;
            }
        }
        catch (Exception ex)
        {
            MessageService.LogMessage($"{nameof(UpdateRepoFile)} - Exception: {ex.Message}");
            result = Global.RequestCanceled;
        }

        return result;
    }

    private UpdateRepoFIleContentBody CreateContent(string description, string urlImage)
    {
        return new UpdateRepoFIleContentBody
        {
            message = "bot_commit: update currently playing",
            committer = new Committer
            {
                name = "bot",
                email = $"{gitHub.user}@mail.com"
            },
            content = EncodeContent(description, urlImage),
            sha = shaFile
        };
    }

    private string EncodeContent(string description, string urlImage)
    {
        string content = $"## Hi there 👋 \n" +
                         $"[![{gitHub.user}'s github stats](https://github-readme-stats.vercel.app/api?username={gitHub.user}&show_icons=true&theme=dark)](https://github.com/anuraghazra/github-readme-stats)\n" +
                         $"[![Top Langs](https://github-readme-stats.vercel.app/api/top-langs/?username={gitHub.user}&layout=compact&theme=dark)](https://github.com/anuraghazra/github-readme-stats)\n" +
                         $"## Currently Playing 🎵 \n" +
                         $"<p align=\"center\">" +
                         $"<img width=\"200\" src=\"{urlImage}\"></p><p align=\"center\"> {description} </p>\n" +
                         "\n---";

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
    }
}
