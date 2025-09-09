using A2A;
using A2A.AspNetCore;
using MS.AI.A2A;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Create and register your agent
var taskManager = new TaskManager();
var agent = new SportEventWinner();
agent.Attach(taskManager);

app.MapA2A(taskManager, "/echo");
app.Run();

