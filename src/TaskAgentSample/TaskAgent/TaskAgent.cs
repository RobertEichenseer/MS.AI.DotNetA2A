using A2A;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MS.AI.A2A;

public class TaskAgent
{
    private ITaskManager? _taskManager;

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;

        taskManager.OnAgentCardQuery = AgentCardQueryHandler;

        taskManager.OnTaskCreated = TaskCreatedHandler;
        taskManager.OnTaskUpdated = TaskUpdatedHandler;
        
    }

    private async Task TaskCreatedHandler(AgentTask agentTask, CancellationToken cancellationToken)
    {
        //Invoked when first message is sent to agent
        await InvokeAgent(agentTask, cancellationToken);
    }

    private async Task TaskUpdatedHandler(AgentTask agentTask, CancellationToken cancellationToken)
    {
        //Invoked when follow up messages with task id are sent to agent
        await InvokeAgent(agentTask, cancellationToken);
    }

    private async Task InvokeAgent(AgentTask agentTask, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskState currentState;

        // Retrieve last message & client request
        Message lastMessage = agentTask.History!.Last();

        string inputText = String.Join(" ",
            lastMessage
                .Parts
                .OfType<TextPart>()
                .Select(p => p.Text)
        );

        //Simulate some initial "Submitted Activities" & echo request text
        await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
        {
            Parts = [
                new TextPart() {
                    Text = $"Request from Client: {inputText}"
                }
            ]
        }, cancellationToken);

        currentState = TaskState.Submitted;
        await _taskManager!.UpdateStatusAsync(
            agentTask.Id,
            status: currentState,
            cancellationToken: cancellationToken
        );

        //Report "JobStart"
        currentState = TaskState.Working;
        await _taskManager!.UpdateStatusAsync(
            agentTask.Id,
            status: currentState,
            cancellationToken: cancellationToken
        );


        //Simulate processing updates - Provide Artifacts
        for (int i = 1; i <= 5; i++)
        {
            await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
            {
                Parts = [
                    new TextPart(){
                        Text = $"Agent response: Processing {i*20} %"
                    }
                ]
            }, cancellationToken);
        }

        //Simulate answer retrieval & return answer to caller
        string finalResponse = "The Munich Flying Dolphins won in Munich with a score of 24:31.";
        await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
        {
            Parts = [
                new TextPart(){
                    Text = finalResponse
                }
            ]
        }, cancellationToken);

        //Signal Task completion
        Message message = new Message()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = agentTask.ContextId,
            Parts = [
                new TextPart(){
                    Text = finalResponse
                }
            ]   
        };

        currentState = TaskState.Completed;
        await _taskManager!.UpdateStatusAsync(
            taskId: agentTask.Id,
            status: currentState,
            message: message,
            final: true,
            cancellationToken: cancellationToken
        );
    }

    private Task<AgentCard> AgentCardQueryHandler(string agentUrl, CancellationToken cancellationToken)
    {
        AgentCapabilities agentCapabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "Simple Agent",
            Description = "Sport Kiosk Agent which can provide the winner, the location and the result of a sport event",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["application/json", "text/plain"],
            DefaultOutputModes = ["text/plain"],
            Capabilities = agentCapabilities,
            Skills = [
                new AgentSkill(){
                    Id = Guid.NewGuid().ToString(),
                    Name = "SportEventWinner",
                    Description = "Returns the winner of a provided sport event",
                    Tags = ["Sport", "Event", "Winner"],
                    Examples = ["Who won the Super Sports Championship 2025"],
                    InputModes = ["text/plain"],
                    OutputModes = ["text/plain"]
                },
                new AgentSkill(){
                    Id = Guid.NewGuid().ToString(),
                    Name = "SportEventResult",
                    Description = "Returns the result of a provided sport event",
                    Tags = ["Sport", "Event", "Result"],
                    Examples = ["What was the result of the Super Sports Championship 2025"],
                    InputModes = ["text/plain"],
                    OutputModes = ["text/plain"]
                },
                new AgentSkill(){
                    Id = Guid.NewGuid().ToString(),
                    Name = "SportEventCity",
                    Description = "Returns the locaton of a provided sport event",
                    Tags = ["Sport", "Event", "Location"],
                    Examples = ["Where did the Super Sports Championship 2025 take place?"],
                    InputModes = ["text/plain"],
                    OutputModes = ["text/plain"]
                },
            ],
        });
    }
}