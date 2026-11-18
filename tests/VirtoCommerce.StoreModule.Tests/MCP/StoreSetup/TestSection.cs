using System.Collections.Generic;
using System.ComponentModel;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

public class TestSection
{
    [Description("Codes of the active methods: the complete list.")]
    public IList<string> Methods { get; set; }
}
