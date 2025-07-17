
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
   git clone https://github.com/Arnab-iitkgp/WorkFlowEngine.git
   cd WorkFlowEngine
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
## Example of a Definition for creating a workflow
```
POST /api/definitions
```

```
{
  "name": "Bug Report Workflow",
  "description": "Tracks the status of a reported software bug with multiple transitions",
  "states": [
    {
      "id": "reported",
      "name": "Reported",
      "isInitial": true,
      "isFinal": false,
      "description": "Bug has been reported"
    },
    {
      "id": "in_review",
      "name": "In Review",
      "isInitial": false,
      "isFinal": false,
      "description": "Bug is being reviewed by the team"
    },
    {
      "id": "needs_more_info",
      "name": "Needs More Info",
      "isInitial": false,
      "isFinal": false,
      "description": "Bug report requires more information from reporter"
    },
    {
      "id": "resolved",
      "name": "Resolved",
      "isInitial": false,
      "isFinal": false,
      "description": "Bug has been fixed and is pending closure"
    },
    {
      "id": "closed",
      "name": "Closed",
      "isInitial": false,
      "isFinal": true,
      "description": "Bug is closed after resolution or dismissal"
    }
  ],
  "actions": [
    {
      "id": "start_review",
      "name": "Start Review",
      "enabled": true,
      "fromStates": ["reported"],
      "toState": "in_review",
      "description": "Begin reviewing the reported bug"
    },
    {
      "id": "request_info",
      "name": "Request More Info",
      "enabled": true,
      "fromStates": ["in_review"],
      "toState": "needs_more_info",
      "description": "Ask reporter for more details"
    },
    {
      "id": "resubmit_info",
      "name": "Resubmit Info",
      "enabled": true,
      "fromStates": ["needs_more_info"],
      "toState": "in_review",
      "description": "Resubmit bug details for review"
    },
    {
      "id": "resolve_bug",
      "name": "Resolve Bug",
      "enabled": true,
      "fromStates": ["in_review"],
      "toState": "resolved",
      "description": "Mark the bug as fixed"
    },
    {
      "id": "close_bug",
      "name": "Close Bug",
      "enabled": true,
      "fromStates": ["resolved", "needs_more_info", "in_review"],
      "toState": "closed",
      "description": "Close the bug after review or resolution"
    }
  ]
}

```
## Example of creating an instance using param query
```
POST /api/instances?definitionId=Bug Report Workflow
```
## Data Storing

- When you **create a workflow definition**, it is saved as a JSON file in the `Data/definitions.json`.
- When you **create a workflow instance**, it is saved as a JSON file in the `Data/instances.json`.

All workflow data is persisted in these JSON files, so you can view or back up your workflows and instances by accessing the `Data` directory in your project.
## Example of executing an action
```
POST /api/instances/{id}/execute
```
```
{
  "actionName": "Approve"
}
```
The API will be available at `http://localhost:5000`.

## License

MIT License
