using System.Runtime.CompilerServices;
using A2A;
using System.Text.Json;

// Agent Card Uri
Uri simpleAgent = new Uri("https://localhost:9000");

//Agent Card
A2ACardResolver cardResolver = new A2ACardResolver(simpleAgent);
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
    Console.WriteLine($"\t\tInputModes: {String.Join(", ", agentSkill.InputModes.ToArray())}");
    Console.WriteLine($"\t\tInputModes: {String.Join(", ", agentSkill.OutputModes.ToArray())}");
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
Console.WriteLine("Raw Response:");
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
};
Console.WriteLine(JsonSerializer.Serialize(a2AResponse, jsonOptions));
Console.WriteLine();

Console.WriteLine("Object Casted Response");
Message message = (Message)a2AResponse;
Console.WriteLine(((TextPart)message.Parts.First<Part>()).Text);

Console.WriteLine("Simple Client terminated");

