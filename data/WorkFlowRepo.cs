using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

public class WorkflowRepository
{
    private readonly string _definitionsPath;
    private readonly string _instancesPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();

    public WorkflowRepository()
    {
        _definitionsPath = Path.Combine("data", "definitions.json");
        _instancesPath = Path.Combine("data", "instances.json");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Ensure data directory exists
        Directory.CreateDirectory("data");
        
        // Create files if they don't exist
        if (!File.Exists(_definitionsPath))
            File.WriteAllText(_definitionsPath, "[]");
        if (!File.Exists(_instancesPath))
            File.WriteAllText(_instancesPath, "[]");
    }

    private List<WorkflowDefinition> LoadDefinitions()
    {
        try
        {
            var json = File.ReadAllText(_definitionsPath);
            return JsonSerializer.Deserialize<List<WorkflowDefinition>>(json, _jsonOptions) ?? new List<WorkflowDefinition>();
        }
        catch
        {
            return new List<WorkflowDefinition>();
        }
    }

    private void SaveDefinitions(List<WorkflowDefinition> definitions)
    {
        var json = JsonSerializer.Serialize(definitions, _jsonOptions);
        File.WriteAllText(_definitionsPath, json);
    }

    private List<WorkflowInstance> LoadInstances()
    {
        try
        {
            var json = File.ReadAllText(_instancesPath);
            return JsonSerializer.Deserialize<List<WorkflowInstance>>(json, _jsonOptions) ?? new List<WorkflowInstance>();
        }
        catch
        {
            return new List<WorkflowInstance>();
        }
    }

    private void SaveInstances(List<WorkflowInstance> instances)
    {
        var json = JsonSerializer.Serialize(instances, _jsonOptions);
        File.WriteAllText(_instancesPath, json);
    }

    // Workflow Definitions
    public void SaveDefinition(WorkflowDefinition definition)
    {
        lock (_lock)
        {
            var definitions = LoadDefinitions();
            var existingIndex = definitions.FindIndex(d => d.Id == definition.Id);
            if (existingIndex >= 0)
            {
                definitions[existingIndex] = definition;
            }
            else
            {
                definitions.Add(definition);
            }
            SaveDefinitions(definitions);
        }
    }

    public WorkflowDefinition? GetDefinition(string id)
    {
        lock (_lock)
        {
            var definitions = LoadDefinitions();
            return definitions.FirstOrDefault(d => d.Id == id);
        }
    }

    public List<WorkflowDefinition> GetAllDefinitions()
    {
        lock (_lock)
        {
            return LoadDefinitions();
        }
    }

    // Workflow Instances
    public void SaveInstance(WorkflowInstance instance)
    {
        lock (_lock)
        {
            var instances = LoadInstances();
            var existingIndex = instances.FindIndex(i => i.Id == instance.Id);
            if (existingIndex >= 0)
            {
                instances[existingIndex] = instance;
            }
            else
            {
                instances.Add(instance);
            }
            SaveInstances(instances);
        }
    }

    public WorkflowInstance? GetInstance(string id)
    {
        lock (_lock)
        {
            var instances = LoadInstances();
            return instances.FirstOrDefault(i => i.Id == id);
        }
    }

    public List<WorkflowInstance> GetAllInstances()
    {
        lock (_lock)
        {
            return LoadInstances();
        }
    }

    public List<WorkflowInstance> GetInstancesByDefinition(string definitionId)
    {
        lock (_lock)
        {
            var instances = LoadInstances();
            return instances.Where(i => i.DefinitionId == definitionId).ToList();
        }
    }
}