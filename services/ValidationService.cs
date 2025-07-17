using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class ValidationService
{
    public ValidationResult ValidateDefinition(WorkflowDefinition definition)
    {
        var result = new ValidationResult { IsValid = true };

        // Check if states exist
        if (!definition.States.Any())
        {
            result.Errors.Add("Workflow definition must have at least one state");
            result.IsValid = false;
        }

        // Check for duplicate state IDs
        var stateIds = definition.States.Select(s => s.Id).ToList();
        var duplicateStateIds = stateIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        if (duplicateStateIds.Any())
        {
            result.Errors.Add($"Duplicate state IDs found: {string.Join(", ", duplicateStateIds)}");
            result.IsValid = false;
        }

        // Check for exactly one initial state
        var initialStates = definition.States.Where(s => s.IsInitial).ToList();
        if (initialStates.Count == 0)
        {
            result.Errors.Add("Workflow definition must have exactly one initial state");
            result.IsValid = false;
        }
        else if (initialStates.Count > 1)
        {
            result.Errors.Add("Workflow definition cannot have more than one initial state");
            result.IsValid = false;
        }

        // Check for duplicate action IDs
        var actionIds = definition.Actions.Select(a => a.Id).ToList();
        var duplicateActionIds = actionIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
        if (duplicateActionIds.Any())
        {
            result.Errors.Add($"Duplicate action IDs found: {string.Join(", ", duplicateActionIds)}");
            result.IsValid = false;
        }

        // Validate actions
        foreach (var action in definition.Actions)
        {
            // Check if fromStates exist
            foreach (var fromStateId in action.FromStates)
            {
                if (!stateIds.Contains(fromStateId))
                {
                    result.Errors.Add($"Action '{action.Id}' references unknown fromState '{fromStateId}'");
                    result.IsValid = false;
                }
            }

            // Check if toState exists
            if (!stateIds.Contains(action.ToState))
            {
                result.Errors.Add($"Action '{action.Id}' references unknown toState '{action.ToState}'");
                result.IsValid = false;
            }

            // Check if fromStates is not empty
            if (!action.FromStates.Any())
            {
                result.Errors.Add($"Action '{action.Id}' must have at least one fromState");
                result.IsValid = false;
            }
        }

        return result;
    }

    public ValidationResult ValidateActionExecution(
        WorkflowInstance instance, 
        WorkflowDefinition definition, 
        string actionId)
    {
        var result = new ValidationResult { IsValid = true };

        // Find the action
        var action = definition.Actions.FirstOrDefault(a => a.Id == actionId);
        if (action == null)
        {
            result.Errors.Add($"Action '{actionId}' not found in workflow definition");
            result.IsValid = false;
            return result;
        }

        // Check if action is enabled
        if (!action.Enabled)
        {
            result.Errors.Add($"Action '{actionId}' is disabled");
            result.IsValid = false;
        }

        // Check if current state allows this action
        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            result.Errors.Add($"Action '{actionId}' cannot be executed from current state '{instance.CurrentStateId}'");
            result.IsValid = false;
        }

        // Check if current state is final
        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        if (currentState != null && currentState.IsFinal)
        {
            result.Errors.Add($"Cannot execute actions on final state '{instance.CurrentStateId}'");
            result.IsValid = false;
        }

        return result;
    }
}