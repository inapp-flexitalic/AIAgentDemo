using System.Text.Json;

public class EngineeringAgent
{
    private readonly JiraService _jira;

    public EngineeringAgent(JiraService jira)
    {
        _jira = jira;
    }

    public async Task<object> ExecuteGoalAsync(string goal)
    {
        var jiraResponse =
            await _jira.CreateTicketAsync(
                goal,
                "Created automatically by AI Agent");

        var doc = JsonDocument.Parse(jiraResponse);

        var ticketKey =
            doc.RootElement.GetProperty("key").GetString();

        int? sprintId =
            await _jira.GetActiveSprintIdAsync();

        bool addedToSprint = false;

        if (sprintId.HasValue)
        {
            addedToSprint =
                await _jira.AddIssueToSprintAsync(
                    sprintId.Value,
                    ticketKey!);
        }

        return new
        {
            Goal = goal,
            Ticket = ticketKey,
            SprintId = sprintId,
            AddedToSprint = addedToSprint,
            Status = "Completed"
        };
    }
}