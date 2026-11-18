namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// A module's part of a store's setup. A part implements the duties it covers (IStoreSetupChoices, IStoreSetupState,
// IStoreSetupCheck, IStoreSetupSection), and a module registers it with AddStoreSetupPart. A part does not choose its
// place: Store lists, plans and applies its own parts first, then the other parts by name.
public interface IStoreSetupPart
{
    // The key of the part's section and the category of its findings, such as "basics" or "payment".
    string Name { get; }

    string Title { get; }

    StoreSetupPermissions Permissions { get; }
}
