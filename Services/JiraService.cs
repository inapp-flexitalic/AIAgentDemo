using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class JiraService
{
    private readonly HttpClient _httpClient;
    private readonly string _jiraUrl;

   
    public JiraService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _jiraUrl = configuration["JiraSettings:BaseUrl"]!;
        var email = configuration["JiraSettings:Email"]!;
        var apiToken = configuration["JiraSettings:ApiToken"]!;

        var auth = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{email}:{apiToken}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);
    }

    public async Task<string> CreateTicketAsync(
        string summary,
        string description)
    {
        var payload = new
        {
            fields = new
            {
                project = new
                {
                    key = "SCRUM"
                },
                summary = summary,
                issuetype = new
                {
                    name = "Task"
                },
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                new
                {
                    type = "paragraph",
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = description
                        }
                    }
                }
            }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);

        var response = await _httpClient.PostAsync(
            $"{_jiraUrl}/rest/api/3/issue",
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"));

        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine(result);

        return result;
    }
    public async Task<int?> GetActiveSprintIdAsync()
    {
        var response = await _httpClient.GetAsync(
            $"{_jiraUrl}/rest/agile/1.0/board/1/sprint?state=active");

        var json = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("values", out var values))
            return null;

        if (values.GetArrayLength() == 0)
            return null;

        return values[0].GetProperty("id").GetInt32();
    }
    public async Task<bool> AddIssueToSprintAsync(
    int sprintId,
    string issueKey)
    {
        var payload = new
        {
            issues = new[] { issueKey }
        };

        var json = JsonSerializer.Serialize(payload);

        var response = await _httpClient.PostAsync(
            $"{_jiraUrl}/rest/agile/1.0/sprint/{sprintId}/issue",
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"));

        return response.IsSuccessStatusCode;
    }
}