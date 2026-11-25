using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// An object with settings, such as a payment method, for the settings rules.
internal sealed class TestMethod : IHasSettings
{
    public string Id { get; set; }

    public string TypeName => nameof(TestMethod);

    public ICollection<ObjectSettingEntry> Settings { get; set; }
}
