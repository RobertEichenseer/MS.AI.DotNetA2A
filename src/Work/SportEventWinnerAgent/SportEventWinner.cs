using A2A;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MS.AI.A2A;

public class SportEventWinner
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
        string agentResponse = "The Munich Flying Dolphins won the Super Sports Championship in Munich.";

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
        return Task.FromResult(new AgentCard
        {
            Name = "Sport_Event_Winner",
            Description = "Provides the winner of a specific sport event",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["Sport Event Name", "Sport Event Year" ],
            DefaultOutputModes = ["Winner", "City"],
            Capabilities = new AgentCapabilities
            {
                Streaming = false
            },
            Skills = [
                new AgentSkill(){
                    Name = "RetrieveSportEventWinner",
                    Id = Guid.NewGuid().ToString(),
                    Description = "Provides the winner of a given sport event",
                    InputModes = ["EventName", "EventYear"],
                    OutputModes = ["Winner"],

                },
               
            ]
            
        });
    }
}