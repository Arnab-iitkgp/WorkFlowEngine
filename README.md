# Workflow Engine

A flexible and configurable workflow engine built with ASP.NET Core that allows you to define, manage, and execute workflows with state transitions and actions.

## 📋 Features

- **Dynamic Workflow Definitions**: Create workflows with custom states and actions
- **State Management**: Track workflow instances through different states
- **Action Execution**: Execute actions with comprehensive validation
- **History Tracking**: Complete audit trail of all state transitions
- **JSON Persistence**: File-based storage using JSON for definitions and instances
- **Validation Rules**: Comprehensive validation for definitions and executions
- **Auto-naming**: Automatic sequential naming for workflow instances
- **Multiple FromStates**: Actions can be executed from multiple source states
- **Final State Protection**: Prevents actions on completed workflows

## 🏗️ Architecture

```
├── Controllers/         # API Controllers (if using controller-based approach)
├── Data/               # Data access layer
│   ├── WorkFlowRepo.cs # JSON file repository implementation
│   └── definitions.json # Workflow definitions storage (gitignored)
│   └── instances.json  # Workflow instances storage (gitignored)
├── Models/             # Data models
│   ├── WorkflowDefinition.cs
│   ├── WorkflowInstance.cs
│   ├── State.cs
│   ├── Action.cs
│   └── ValidationResult.cs
├── Services/           # Business logic
│   ├── WorkflowService.cs
│   └── ValidationService.cs
└── Program.cs          # Application entry point and API endpoints
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 or later
- Any REST client (Postman, curl, etc.)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Infonetica_Assignment
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

The application will start on `http://localhost:5000`

### Quick Start

1. **Check application health**
   ```bash
   curl http://localhost:5000/health
   ```

2. **Create your first workflow definition**
   ```bash
   curl -X POST http://localhost:5000/api/definitions \
   -H "Content-Type: application/json" \
   -d '{
     "name": "Simple Workflow",
     "description": "A basic workflow example",
     "states": [
       {
         "id": "start",
         "name": "Start",
         "isInitial": true,
         "isFinal": false,
         "description": "Starting state"
       },
       {
         "id": "end",
         "name": "End",
         "isInitial": false,
         "isFinal": true,
         "description": "Ending state"
       }
     ],
     "actions": [
       {
         "id": "finish",
         "name": "Finish",
         "enabled": true,
         "fromStates": ["start"],
         "toState": "end",
         "description": "Complete the workflow"
       }
     ]
   }'
   ```

3. **Start a workflow instance**
   ```bash
   curl -X POST "http://localhost:5000/api/instances?definitionId={DEFINITION_ID}"
   ```

4. **Execute an action**
   ```bash
   curl -X POST http://localhost:5000/api/instances/{INSTANCE_ID}/execute \
   -H "Content-Type: application/json" \
   -d '{"actionName": "finish"}'
   ```

## 📚 API Documentation

### Base URL
```
http://localhost:5000
```

### Endpoints

#### Health Check
- **GET** `/health` - Check application status

#### Workflow Definitions
- **POST** `/api/definitions` - Create a new workflow definition
- **GET** `/api/definitions` - Get all workflow definitions
- **GET** `/api/definitions/{id}` - Get a specific workflow definition

#### Workflow Instances
- **POST** `/api/instances?definitionId={id}` - Start a new workflow instance
- **GET** `/api/instances` - Get all workflow instances
- **GET** `/api/instances/{id}` - Get a specific workflow instance
- **GET** `/api/definitions/{definitionId}/instances` - Get instances for a definition
- **POST** `/api/instances/{id}/execute` - Execute an action on an instance

### Request/Response Examples

#### Create Workflow Definition
```json
POST /api/definitions
{
  "name": "Order Processing Workflow",
  "description": "Handles order from creation to fulfillment",
  "states": [
    {
      "id": "created",
      "name": "Created",
      "isInitial": true,
      "isFinal": false,
      "description": "Order has been created"
    },
    {
      "id": "completed",
      "name": "Completed",
      "isInitial": false,
      "isFinal": true,
      "description": "Order has been completed"
    }
  ],
  "actions": [
    {
      "id": "complete",
      "name": "Complete",
      "enabled": true,
      "fromStates": ["created"],
      "toState": "completed",
      "description": "Complete the order"
    }
  ]
}
```

#### Execute Action
```json
POST /api/instances/{instanceId}/execute
{
  "actionName": "complete"
}
```

#### Response Format
```json
{
  "success": true,
  "data": { /* response data */ },
  "error": null,
  "errors": null
}
```

## 🔧 Configuration

### JSON File Storage
The application uses JSON files for data persistence:
- `data/definitions.json` - Stores workflow definitions
- `data/instances.json` - Stores workflow instances

These files are automatically created if they don't exist and are excluded from git tracking.

### Auto-naming Convention
Workflow instances are automatically named using the pattern:
```
{DefinitionName}_{SerialNumber}
```
Examples:
- `Order Processing Workflow_1`
- `Order Processing Workflow_2`
- `Customer Onboarding_1`

## ✅ Validation Rules

### Definition Validation
- ✅ No duplicate state IDs
- ✅ No duplicate action IDs
- ✅ Exactly one initial state
- ✅ Valid fromStates and toState references
- ✅ No duplicate definition names
- ✅ Actions must have at least one fromState

### Execution Validation
- ✅ Action must exist in definition
- ✅ Action must be enabled
- ✅ Current state must be in action's fromStates
- ✅ Target state must exist
- ✅ Cannot execute actions on final states

## 🧪 Testing

### Using Postman
1. Import the API endpoints into Postman
2. Set base URL to `http://localhost:5000`
3. Test the workflow creation and execution flow

### Test Scenarios
1. **Happy Path**: Create definition → Start instance → Execute actions
2. **Validation**: Try invalid definitions and action executions
3. **Final States**: Test protection of final states
4. **Multiple FromStates**: Test actions with multiple source states
5. **Disabled Actions**: Test enabled/disabled action functionality

## 🔄 Sample Workflow

The application comes with a sample "Order Processing Workflow":

**States**: `created` → `validated` → `shipped` → `completed`

**Actions**:
- `validate`: `created` → `validated`
- `ship`: `validated` → `shipped`
- `complete`: `shipped` → `completed`
- `cancel`: `created|validated|shipped` → `cancelled`

## 📁 Data Structure

### Workflow Definition
```json
{
  "id": "unique-guid",
  "name": "Workflow Name",
  "description": "Workflow description",
  "states": [...],
  "actions": [...],
  "createdAt": "2025-07-18T..."
}
```

### Workflow Instance
```json
{
  "id": "unique-guid",
  "name": "Auto-generated name",
  "definitionId": "definition-guid",
  "currentStateId": "current-state",
  "history": [...],
  "createdAt": "2025-07-18T...",
  "lastUpdated": "2025-07-18T..."
}
```

## 🛠️ Development

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Run in development mode with hot reload
dotnet watch run

# Build for release
dotnet build -c Release

# Publish the application
dotnet publish -c Release -o ./publish
```

### Project Structure
```
WorkflowEngine/
├── Controllers/        # API Controllers
├── Data/              # Data access layer
├── Models/            # Domain models
├── Services/          # Business logic
├── data/              # JSON data files (auto-created, gitignored)
│   ├── definitions.json
│   └── instances.json
├── Program.cs         # Application entry point
└── WorkflowEngine.csproj
```

## 🔒 Error Handling

The application provides comprehensive error handling with detailed error messages:

```json
{
  "success": false,
  "data": null,
  "error": "Primary error message",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

## 🚀 Deployment

### Local Development
```bash
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet WorkflowEngine.dll
```

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📞 Support

For questions or issues, please open an issue in the repository.
