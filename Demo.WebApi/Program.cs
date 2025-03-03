using Demo.Shared.Extensions;
using Demo.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseRouting();
app.UseSharedInfrastructure();

// In-memory store for demo purposes
var todos = new List<Todo>
{
    new Todo { Id = 1, Title = "Buy milk", IsCompleted = false },
    new Todo { Id = 2, Title = "Read book", IsCompleted = true }
};

// CRUD Endpoints
app.MapGet("/todos", (ILogger<Program> logger) =>
{
    logger.LogInformation("Retrieving all todos. Count: {Count}", todos.Count);
    return Results.Ok(todos);
});

app.MapGet("/todos/{id}", (int id, ILogger<Program> logger) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null)
    {
        logger.LogWarning("Todo with ID {Id} not found", id);
        return Results.NotFound();
    }

    logger.LogInformation("Retrieved todo: {@Todo}", todo);
    return Results.Ok(todo);
});

app.MapPost("/todos", (Todo todo, ILogger<Program> logger) =>
{
    todo.Id = todos.Count  + 1; 
    todos.Add(todo);
    logger.LogInformation("Created todo: {@Todo}", todo);
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id}", (int id, Todo updatedTodo, ILogger<Program> logger) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null)
    {
        logger.LogWarning("Todo with ID {Id} not found for update", id);
        return Results.NotFound();
    }

    todo.Title = updatedTodo.Title;
    todo.IsCompleted = updatedTodo.IsCompleted;
    logger.LogInformation("Updated todo: {@Todo}", todo);
    return Results.Ok(todo);
});

app.MapDelete("/todos/{id}", (int id, ILogger<Program> logger) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo == null)
    {
        logger.LogWarning("Todo with ID {Id} not found for deletion", id);
        return Results.NotFound();
    }

    todos.Remove(todo);
    logger.LogInformation("Deleted todo with ID {Id}", id);
    return Results.NoContent();
});

app.Run();

