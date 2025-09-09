using System.Runtime.CompilerServices;
using A2A;
using System.Text.Json;
using System.Diagnostics;

// Retrieve agent card urls from simulated repository
Uri sportWinnerAgent = new Uri("https://localhost:7000");
Uri sportResultAgent = new Uri("https://localhost:7001");
Uri sportWinnerAgentTask = new Uri("https://localhost:8000");
Uri sportResultAgentTask = new Uri("https://localhost:8001");

//Agent Cards
A2ACardResolver cardResolver = new A2ACardResolver(sportResultAgentTask);
AgentCard agentCard = await cardResolver.GetAgentCardAsync();
ShowAgentCard(agentCard);
string sportResultAgentTaskUrl = agentCard.Url;


cardResolver = new A2ACardResolver(sportWinnerAgentTask);
agentCard = await cardResolver.GetAgentCardAsync();
ShowAgentCard(agentCard);
string sportWinnerAgentTaskUrl = agentCard.Url;

cardResolver = new A2ACardResolver(sportWinnerAgent);
agentCard = await cardResolver.GetAgentCardAsync();
ShowAgentCard(agentCard);
string sportWinnerAgentUrl = agentCard.Url;

cardResolver = new A2ACardResolver(sportResultAgent);
agentCard = await cardResolver.GetAgentCardAsync();
ShowAgentCard(agentCard);
string sportResultAgentUrl = agentCard.Url;

// Create MessageSendParam to be sent to agent pair
MessageSendParams messageSendParams = GetMessageSendParams();

// Agents who return Tasks (non-streaming / streaming)
// SportResultAgent
A2AClient a2AClient = new A2AClient(new Uri(sportResultAgentTaskUrl));
A2AResponse a2AResponse = await a2AClient.SendMessageAsync(messageSendParams);

Console.WriteLine("Non-Streaming: ");
await ShowAgentResponse(a2AResponse); //Raw response
AgentTask agentTask = (AgentTask)a2AResponse; //Object casted response
Console.WriteLine(((TextPart)agentTask?.Artifacts.Last<Artifact>().Parts.Last<Part>()).Text);

Console.WriteLine("Streaming: ");
await foreach (var result in a2AClient.SendMessageStreamAsync(messageSendParams))
{
    await ShowAgentResponse(result.Data); //Raw response
    if (result.Data is TaskStatusUpdateEvent taskStatusUpdateEvent)
    {
        Console.WriteLine(taskStatusUpdateEvent.Status);
        if (taskStatusUpdateEvent.Status.State == TaskState.Completed)
        {
            Message messageStream = taskStatusUpdateEvent.Status.Message;
            Console.WriteLine(((TextPart)messageStream.Parts.Last<Part>()).Text);
        }
    }
}

// SportWinnerAgent
Console.WriteLine("Non-Streaming: ");
a2AClient = new A2AClient(new Uri(sportWinnerAgentTaskUrl));
a2AResponse = await a2AClient.SendMessageAsync(messageSendParams);
await ShowAgentResponse(a2AResponse);
agentTask = (AgentTask)a2AResponse; //Object casted response
Console.WriteLine(((TextPart)agentTask?.Artifacts.Last<Artifact>().Parts.Last<Part>()).Text);

Console.WriteLine("Streaming: ");
await foreach (var result in a2AClient.SendMessageStreamAsync(messageSendParams))
{
    await ShowAgentResponse(result.Data);
    if (result.Data is TaskStatusUpdateEvent taskStatusUpdateEvent)
    {
        Console.WriteLine(taskStatusUpdateEvent.Status);
        if (taskStatusUpdateEvent.Status.State == TaskState.Completed)
        {
            Message messageStream = taskStatusUpdateEvent.Status.Message;
            Console.WriteLine(((TextPart)messageStream.Parts.Last<Part>()).Text);
        }
    }
}

// Agents who return Messages
// SportWinnerAgent
Console.WriteLine("Message: ");
a2AClient = new A2AClient(new Uri(sportWinnerAgentUrl));
a2AResponse = await a2AClient.SendMessageAsync(messageSendParams);
await ShowAgentResponse(a2AResponse);
Message message = (Message)a2AResponse; //Object casted response
Console.WriteLine(((TextPart)message?.Parts.Last<Part>()).Text);

//SportResultAgent
a2AClient = new A2AClient(new Uri(sportResultAgentUrl));
a2AResponse = await a2AClient.SendMessageAsync(messageSendParams);
await ShowAgentResponse(a2AResponse);
message = (Message)a2AResponse; //Object casted response
Console.WriteLine(((TextPart)message?.Parts.Last<Part>()).Text);



Console.WriteLine("Client stopped");


// Show AgentCard
void ShowAgentCard(AgentCard agentCard)
{
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

}

async Task ShowAgentResponse(A2AEvent agentEvent)
{
    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    Console.WriteLine($"Agent response => {JsonSerializer.Serialize(agentEvent, jsonOptions)}");
    Console.WriteLine();
}


// Create Message for Agent
MessageSendParams GetMessageSendParams()
{
    return new  MessageSendParams()
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
}


