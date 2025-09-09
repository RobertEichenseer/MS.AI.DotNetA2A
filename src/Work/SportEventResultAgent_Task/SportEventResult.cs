using A2A;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MS.AI.A2A;

public class SportEventResult
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
                        Text = $"Processing {i*20} %"
                    }
                ]
            }, cancellationToken);
        }

        //Simulate answer retrieval & return answer to caller
        string finalResponse = "The result was 24:31";
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
                    Text = $"{finalResponse}"
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
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "Sport Event Result Task",
            Description = "Agent provides sport event result as task",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["Sport event name & sport event year"],
            DefaultOutputModes = ["Sport event result"],
            Capabilities = capabilities,
            Skills = [],
        });
    }

}