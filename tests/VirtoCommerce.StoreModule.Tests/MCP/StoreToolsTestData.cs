using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Tests.MCP;

// The data of one test: the stores the store services read and save, the store-level settings the modules register, and
// the MCP module's pending changes.
public class StoreToolsTestData
{
    public List<Store> Stores { get; } = StoreToolsTestContext.CreateStores();

    public List<SettingDescriptor> StoreSettings { get; } = [StoreToolsTestContext.TaxCalculation, StoreToolsTestContext.MapsApiKey];

    public List<McpPendingChange> PendingChanges { get; } = [];
}
