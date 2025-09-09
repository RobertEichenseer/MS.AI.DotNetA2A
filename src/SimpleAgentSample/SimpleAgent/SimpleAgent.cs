using A2A;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MS.AI.A2A;

public class SimpleAgent
{
   
    public void Attach(ITaskManager taskManager)
    {
        taskManager.OnMessageReceived = MessageReceivedEventHandler;
        taskManager.OnAgentCardQuery = AgentCardQueryEventHandler;
    }

    private async Task<Message> MessageReceivedEventHandler(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        //Extract MessageSendParams of type TextPart
        IAsyncEnumerable<TextPart> textParts =
            messageSendParams
                .Message
                .Parts.OfType<TextPart>()
                .ToAsyncEnumerable();

        string inputText = "";
        await foreach (TextPart textPart in textParts) {
            inputText += String.Concat(textPart.Text, ' ');
        }

        //Process message
        //Simulated response
        string agentResponse = "The Munich Flying Dolphins won in Munich with a result of 24:31";

        //Create agent response (Part(s) & Message)
        List<Part> parts = [
            new TextPart(){
                Text = $"{agentResponse}"
            }
        ];
        
        Message message = new Message()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = parts   
        };

        return message;

    }
    private Task<AgentCard> AgentCardQueryEventHandler(string agentUrl, CancellationToken cancellationToken)
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