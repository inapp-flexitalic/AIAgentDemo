using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly EngineeringAgent _agent;

    public AgentController(
        EngineeringAgent agent)
    {
        _agent = agent;
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute(
        [FromBody] AgentRequest request)
    {
        var result =
            await _agent.ExecuteGoalAsync(
                request.Goal);

        return Ok(result);
    }
}