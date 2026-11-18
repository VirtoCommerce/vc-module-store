using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

// The plan of the basics section, with the section itself: applied again to the saved store, it changes only what is
// not applied yet.
public class StoreBasicsSetupPlan : StoreSetupPartPlan
{
    public StoreBasicsSection Section { get; set; }
}
