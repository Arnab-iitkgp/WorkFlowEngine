using WorkflowEngine.Data;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<WorkflowRepository>();
builder.Services.AddSingleton<ValidationService>();
builder.Services.AddSingleton<WorkflowService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");

// Workflow Definition endpoints
app.MapPost("/api/definitions", (WorkflowService service, CreateDefinitionRequest request) =>
{
    var response = service.CreateDefinitionAsync(request);
    return response.Success ? Results.Created($"/api/definitions/{response.Data!.Id}", response) : Results.BadRequest(response);
});

app.MapGet("/api/definitions/{id}", (WorkflowService service, string id) =>
{
    var response = service.GetDefinitionAsync(id);
    return response.Success ? Results.Ok(response) : Results.NotFound(response);
});

app.MapGet("/api/definitions", (WorkflowService service) =>
{
    var response = service.GetAllDefinitionsAsync();
    return Results.Ok(response);
});

// Workflow Instance endpoints
app.MapPost("/api/instances", (WorkflowService service, string definitionId) =>
{
    var response = service.StartInstanceAsync(definitionId);
    return response.Success ? Results.Created($"/api/instances/{response.Data!.Id}", response) : Results.BadRequest(response);
});

app.MapGet("/api/instances/{id}", (WorkflowService service, string id) =>
{
    var response = service.GetInstanceAsync(id);
    return response.Success ? Results.Ok(response) : Results.NotFound(response);
});

app.MapGet("/api/instances", (WorkflowService service) =>
{
    var response = service.GetAllInstancesAsync();
    return Results.Ok(response);
});

app.MapGet("/api/definitions/{definitionId}/instances", (WorkflowService service, string definitionId) =>
{
    var response = service.GetInstancesByDefinitionAsync(definitionId);
    return Results.Ok(response);
});

app.MapPost("/api/instances/{id}/execute", (WorkflowService service, string id, ExecuteActionRequest request) =>
{
    var response = service.ExecuteActionAsync(id, request.ActionName);
    return response.Success ? Results.Ok(response) : Results.BadRequest(response);
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Root endpoint with API documentation
app.MapGet("/", () => Results.Ok(new
{
    message = "Configurable Workflow Engine API",
    version = "1.0.0",
    endpoints = new
    {
        definitions = new
        {
            create = "POST /api/definitions",
            get = "GET /api/definitions/{id}",
            list = "GET /api/definitions"
        },
        instances = new
        {
            create = "POST /api/instances?definitionId={definitionId}",
            get = "GET /api/instances/{id}",
            list = "GET /api/instances",
            listByDefinition = "GET /api/definitions/{definitionId}/instances",
            execute = "POST /api/instances/{id}/execute"
        }
    }
}));

app.Run();