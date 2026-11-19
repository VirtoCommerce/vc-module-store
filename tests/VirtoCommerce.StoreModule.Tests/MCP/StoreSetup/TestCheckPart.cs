using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part that only checks, as Catalog, Pricing and Inventory will: it has no choices, state or section.
public class TestCheckPart(string name) : IStoreSetupCheck
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public IList<McpFinding> Findings { get; init; } = [];

    // Thrown by the check, as a part whose module or third party fails.
    public Exception Error { get; init; }

    public StoreSetupContext CheckedContext { get; private set; }

    public Task<IList<McpFinding>> CheckAsync(StoreSetupContext context)
    {
        CheckedContext = context;

        if (Error != null)
        {
            throw Error;
        }

        return Task.FromResult(Findings);
    }

    // Adds a finding of this part.
    public TestCheckPart WithFinding(string id, string severity)
    {
        Findings.Add(CreateFinding(id, Name, severity));
        return this;
    }

    public static McpFinding CreateFinding(string id, string part, string severity)
    {
        return new McpFinding
        {
            Id = id,
            Category = part,
            Severity = severity,
            Title = $"Finding {id}",
            Why = "It matters.",
            Fix = "Fix it.",
            Screen = "Stores > Test",
        };
    }

    public static Store CreateStore(string id = "test-store")
    {
        return new Store { Id = id, Name = $"Store {id}" };
    }
}
