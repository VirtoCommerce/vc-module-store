using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part that takes a section and keeps what it applies per store, as Payment's part keeps a store's methods: its plan
// lists a change only for what is not applied yet, so a repeat applies only the rest.
public class TestApplyPart(string name) : IStoreSetupSection
{
    public string Name => name;

    public string Title => $"Test part {name}";

    public StoreSetupPermissions Permissions { get; init; } = StoreSetupPermissions.ForStore();

    public Type SectionType => typeof(TestSection);

    public bool IsRequiredForNewStore { get; init; }

    // The part's own refusal, returned by its plan.
    public string PlanError { get; init; }

    // Thrown by ApplyAsync, as a module whose save fails.
    public Exception ApplyError { get; set; }

    public IDictionary<string, IList<string>> Saved { get; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

    public int ApplyCount { get; private set; }

    public Task<StoreSetupPartPlan> PlanAsync(StoreSetupContext context, object section)
    {
        var methods = ((TestSection)section).Methods ?? [];
        var plan = new StoreSetupPartPlan { Part = name };

        if (PlanError != null)
        {
            plan.Errors.Add(PlanError);
        }

        var saved = Saved.TryGetValue(context.Store.Id, out var value) ? value : [];

        if (!methods.SequenceEqual(saved))
        {
            plan.AddChange("methods", saved, methods);
        }

        return Task.FromResult(plan);
    }

    public Task ApplyAsync(StoreSetupContext context, StoreSetupPartPlan plan)
    {
        ApplyCount++;

        if (ApplyError != null)
        {
            throw ApplyError;
        }

        Saved[context.Store.Id] = (IList<string>)plan.Changes.Single().To;

        return Task.CompletedTask;
    }
}
