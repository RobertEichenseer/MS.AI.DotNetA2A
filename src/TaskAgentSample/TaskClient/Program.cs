using System.Runtime.CompilerServices;
using A2A;
using System.Text.Json;
using System.Diagnostics;

// Agent Card Uri
Uri taskAgent = new Uri("https://localhost:9010");

//Agent Card
A2ACardResolver cardResolver = new A2ACardResolver(taskAgent);
AgentCard agentCard = await cardResolver.GetAgentCardAsync();
Console.WriteLine("Agent Card: ");
Console.WriteLine($"\tName: {agentCard.Name}");
Console.WriteLine($"\tVersion: {agentCard.Version}");
Console.WriteLine($"\tUrl: {agentCard.Url}");
Console.WriteLine($"\tUrl: {agentCard.Description}");
Console.WriteLine("\tSkills:");
foreach (AgentSkill agentSkill in agentCard.Skills)
{
    Console.WriteLine($"\t\tID: {agentSkill.Id}");
    Console.WriteLine($"\t\tName: {agentSkill.Name}");
    Console.WriteLine($"\t\tInputModes: {String.Join(", ", agentSkill.InputModes?.ToArray())}");
    Console.WriteLine($"\t\tInputModes: {String.Join(", ", agentSkill.OutputModes?.ToArray())}");
}

// Create A2A Client
A2AClient a2AClient = new A2AClient(new Uri(agentCard.Url));
// Create MessageSendParam to be sent to agent pair
MessageSendParams messageSendParams = new MessageSendParams()
{
    Message = new Message()
    {
        Role = MessageRole.User,
        Parts = [
            new TextPart() {
                Text = "Which team won the Super Championship 2025?"
            },
            new TextPart() {
                Text = "In which City was the final?"
            },
            new TextPart() {
                Text = "What was the result?"
            }
        ]
    }
};
// Send Message to Agent
A2AResponse a2AResponse = await a2AClient.SendMessageAsync(messageSendParams);

await foreach (var result in a2AClient.SendMessageStreamAsync(messageSendParams))
{
    await ShowAgentResponse(result.Data); //Raw response

    if (result.Data is TaskStatusUpdateEvent taskStatusUpdateEvent)
    {
        if (taskStatusUpdateEvent.Status.State == TaskState.Completed)
        {
            Console.WriteLine("Object Casted Agent Response:");
            Console.WriteLine(taskStatusUpdateEvent.Status);
            Message messageStream = taskStatusUpdateEvent.Status.Message;
            Console.WriteLine(((TextPart)messageStream.Parts.Last<Part>()).Text);
        }
    }
}

Console.WriteLine("Task Client terminated");

async Task ShowAgentResponse(A2AEvent agentEvent)
{
    Console.WriteLine("Raw Agent Response:");
    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    Console.WriteLine(JsonSerializer.Serialize(agentEvent, jsonOptions));
    Console.WriteLine();
}



