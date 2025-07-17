
# Workflow Engine

A simple, flexible workflow engine built with ASP.NET Core.

This engine lets you define workflows as a series of states and actions. You can create a workflow definition, start workflow instances, and move them between states by executing actions. All data is stored in JSON files, and you interact with the engine using a REST API.

## Features

- Define custom workflows with states and actions
- Track workflow progress and history
- JSON file storage (no database needed)
- REST API for managing workflows and instances

## Getting Started

### Prerequisites

- .NET 8.0 or later


### Setup in Local Repository

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd <repository-folder>
   ```

2. Restore dependencies, build, and run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

The API will be available at `http://localhost:5000`.

## Example Usage

1. **Create a workflow definition**
2. **Start a workflow instance**
3. **Execute actions to move between states**

Use any REST client (like Postman or curl) to interact with the API.

## Run Commands

```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at `http://localhost:5000`.

## License

MIT License
