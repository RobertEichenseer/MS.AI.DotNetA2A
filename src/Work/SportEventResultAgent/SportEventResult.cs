using A2A;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MS.AI.A2A;

public class SportEventResult
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
        string agentResponse = "The result was 24:31";

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
            Name = "Echo Agent",
            Description = "Agent which will echo every message it receives.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [],
        });
    }

}