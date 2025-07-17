using WorkflowEngine.Data;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class WorkflowService
{
    private readonly WorkflowRepository _repository;
    private readonly ValidationService _validationService;

    public WorkflowService(WorkflowRepository repository, ValidationService validationService)
    {
        _repository = repository;
        _validationService = validationService;
    }

    public ApiResponse<WorkflowDefinition> CreateDefinitionAsync(CreateDefinitionRequest request)
    {
        // Check for duplicate definition name
        var existingDefinitions = _repository.GetAllDefinitions();
        if (existingDefinitions.Any(d => d.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                Error = $"Workflow definition with name '{request.Name}' already exists"
            };
        }

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            States = request.States,
            Actions = request.Actions,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        // Validate the definition
        var validationResult = _validationService.ValidateDefinition(definition);
        if (!validationResult.IsValid)
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                Error = "Validation failed",
                Errors = validationResult.Errors
            };
        }

        _repository.SaveDefinition(definition);

        return new ApiResponse<WorkflowDefinition>
        {
            Success = true,
            Data = definition
        };
    }

    public ApiResponse<WorkflowDefinition> GetDefinitionAsync(string id)
    {
        var definition = _repository.GetDefinition(id);
        if (definition == null)
        {
            return new ApiResponse<WorkflowDefinition>
            {
                Success = false,
                Error = "Workflow definition not found"
            };
        }

        return new ApiResponse<WorkflowDefinition>
        {
            Success = true,
            Data = definition
        };
    }

    public ApiResponse<List<WorkflowDefinition>> GetAllDefinitionsAsync()
    {
        var definitions = _repository.GetAllDefinitions();
        return new ApiResponse<List<WorkflowDefinition>>
        {
            Success = true,
            Data = definitions
        };
    }

    public ApiResponse<WorkflowInstance> StartInstanceAsync(string definitionId)
    {
        var definition = _repository.GetDefinition(definitionId);
        if (definition == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Workflow definition not found"
            };
        }

        var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
        if (initialState == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "No initial state found in workflow definition"
            };
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = definitionId,
            CurrentStateId = initialState.Id,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _repository.SaveInstance(instance);

        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }

    public ApiResponse<WorkflowInstance> ExecuteActionAsync(string instanceId, string actionName)
    {
        var instance = _repository.GetInstance(instanceId);
        if (instance == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Workflow instance not found"
            };
        }

        var definition = _repository.GetDefinition(instance.DefinitionId);
        if (definition == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Workflow definition not found"
            };
        }

        // Find the action by name
        var action = definition.Actions.FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));
        if (action == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{actionName}' not found in workflow definition"
            };
        }

        // Check if action is enabled
        if (!action.Enabled)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{actionName}' is currently disabled"
            };
        }

        // Check if action can be executed from current state
        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{actionName}' cannot be executed from current state '{instance.CurrentStateId}'"
            };
        }

        // Check if current state is final (cannot execute actions on final states)
        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState != null && currentState.IsFinal)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Cannot execute actions on final state '{instance.CurrentStateId}'"
            };
        }

        // Check if target state exists (transition to unknown state)
        var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState);
        if (targetState == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = $"Action '{actionName}' references unknown target state '{action.ToState}'"
            };
        }

        var previousStateId = instance.CurrentStateId;

        // Update instance state
        instance.CurrentStateId = action.ToState;
        instance.LastUpdated = DateTime.UtcNow;

        // Add to history
        instance.History.Add(new ActionHistory
        {
            ActionId = action.Id,
            ActionName = action.Name,
            FromStateId = previousStateId,
            ToStateId = action.ToState,
            ExecutedAt = DateTime.UtcNow
        });

        _repository.SaveInstance(instance);

        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }

    public ApiResponse<WorkflowInstance> GetInstanceAsync(string id)
    {
        var instance = _repository.GetInstance(id);
        if (instance == null)
        {
            return new ApiResponse<WorkflowInstance>
            {
                Success = false,
                Error = "Workflow instance not found"
            };
        }

        return new ApiResponse<WorkflowInstance>
        {
            Success = true,
            Data = instance
        };
    }

    public ApiResponse<List<WorkflowInstance>> GetAllInstancesAsync()
    {
        var instances = _repository.GetAllInstances();
        return new ApiResponse<List<WorkflowInstance>>
        {
            Success = true,
            Data = instances
        };
    }

    public ApiResponse<List<WorkflowInstance>> GetInstancesByDefinitionAsync(string definitionId)
    {
        var instances = _repository.GetInstancesByDefinition(definitionId);
        return new ApiResponse<List<WorkflowInstance>>
        {
            Success = true,
            Data = instances
        };
    }
}