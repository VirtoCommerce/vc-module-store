using System.Collections.Generic;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// An allowed value of a section field, such as the code of a shipping method with the settings it takes.
public class StoreSetupChoice
{
    // The field the choice fills, starting with its section: "basics.languages", "shipping.methods.code". So a part can
    // offer choices for another part's field.
    public string Field { get; set; }

    public string Value { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    // The settings this choice takes, without secrets.
    public IList<StoreSetupSetting> Settings { get; set; } = [];
}
