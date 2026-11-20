using System.Collections.Generic;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.MCP.Core.Settings;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Data.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A solution's subclass of Store's settings rules: it notes the settings each apply copies, then copies them as Store does.
internal sealed class RecordingSetupSettings(
    ISettingsRegistrar settingsRegistrar,
    IMcpSecretSettings secretSettings,
    IMcpSettingTexts settingTexts,
    IMcpSetupValues setupValues)
    : StoreSetupSettings(settingsRegistrar, secretSettings, settingTexts, setupValues)
{
    public List<string> CopiedNames { get; } = [];

    public override void CopySettings(IHasSettings planned, IHasSettings target, ICollection<string> names)
    {
        CopiedNames.AddRange(names);
        base.CopySettings(planned, target, names);
    }
}
