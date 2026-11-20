using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.MCP.Services;
using StorePermissions = VirtoCommerce.StoreModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.StoreModule.MCP.Tools;

// The store-level settings of every installed module, with what each one means. Secrets are never listed.
[McpServerToolType]
[McpScope(McpScopes.Admin)]
[McpPermission(StorePermissions.Read)]
public class StoreSettingsTools(
    StoreAccess access,
    IStoreSetupSettings setupSettings,
    ISettingsRegistrar settingsRegistrar,
    IMcpPageSizeResolver pageSizeResolver,
    StoreBackOfficeLinks links,
    IMcpSetupValues setupValues)
{
    [McpServerTool(Name = "get_store_settings", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("The store-level settings of every installed module for one store, each with its title, description, " +
        "module, type, default, current value, allowed values and whether a setup can change it. Filter by module, by " +
        "words in the name, title or description, or to the settings changed from their defaults. Secrets are not listed: " +
        "a person enters them in the back office. Returns one page: when has_more is true, call again with next_skip.")]
    public async Task<object> GetStoreSettings(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("Module id, such as VirtoCommerce.Store, to list that module's settings only.")] string module_id = null,
        [Description("Words to find in the name, title or description, such as \"tax calculation\". Every word must be found. Omit it to list every setting.")] string keyword = null,
        [Description("true to list only the settings whose values differ from their defaults.")] bool changed_only = false,
        [Description("Number of settings to skip.")] int? skip = null,
        [Description("Number of settings to return. Omit it for the default page size; larger values are capped.")] int? take = null)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);
        var settings = await setupSettings.GetStoreSettingsAsync(store);

        if (!string.IsNullOrWhiteSpace(module_id))
        {
            settings = settings.Where(x => x.ModuleId.EqualsIgnoreCase(module_id.Trim())).ToList();

            // A wrong module id would otherwise read as a module without store settings. A module whose store settings are
            // all secrets or hidden gets an empty page, with the note on secrets.
            if (settings.Count == 0 && !HasStoreSettings(module_id))
            {
                throw new McpException($"Module '{module_id}' has no store settings here. Module ids look like VirtoCommerce.Store; " +
                    "get_store_settings without module_id lists the store settings of every module.");
            }
        }

        var words = (keyword ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var found = settings
            .Where(x => words.All(word => HasWord(x, word)))
            .Where(x => !changed_only || x.IsChanged);

        return new
        {
            store_id = store.Id,
            name = store.Name,
            settings = McpPage<McpSetupSetting>.FromList(found, paging),
            secrets = await GetSecretsNoteAsync(store.Name ?? store.Id),
            next_step = "apply_store_setup with the 'settings' section changes the settings whose is_changeable is true.",
        };
    }

    [McpServerTool(Name = "compare_store_settings", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("The store-level settings whose values differ between two stores, or between a store and the defaults " +
        "when other_store_id is omitted. Use it to give a new store the settings of a store that works, or to see what " +
        "was changed in a store. Secrets are not compared. Returns one page: when has_more is true, call again with next_skip.")]
    public async Task<object> CompareStoreSettings(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("The store to compare with. Omit it to compare with the defaults.")] string other_store_id = null,
        [Description("Number of settings to skip.")] int? skip = null,
        [Description("Number of settings to return. Omit it for the default page size; larger values are capped.")] int? take = null)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);
        var otherStore = string.IsNullOrWhiteSpace(other_store_id) ? null : await access.GetReadableStoreAsync(context.User, other_store_id);
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);

        var settings = await setupSettings.GetStoreSettingsAsync(store);
        // The other store's values in effect, read as the first store's are.
        var otherValues = otherStore == null
            ? null
            : (await setupSettings.GetStoreSettingsAsync(otherStore))
                .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var differences = settings
            .Select(x => new
            {
                name = x.Name,
                title = x.Title,
                module_id = x.ModuleId,
                value = x.Value,
                other_value = otherValues?.GetValueOrDefault(x.Name) ?? x.DefaultValue,
            })
            .Where(x => !setupValues.AreEqual(x.value, x.other_value));

        return new
        {
            store_id = store.Id,
            compared_with = otherStore?.Id ?? "defaults",
            differences = differences.ToMcpPage(paging, x => x),
            next_step = "apply_store_setup with the 'settings' section copies the values you choose.",
        };
    }

    protected virtual async Task<string> GetSecretsNoteAsync(string storeName)
    {
        var screen = $"Stores > {storeName} > Settings".WithLink(await links.GetStoresLinkAsync());

        return $"Secrets are not listed. A person enters them in the back office: {screen}.";
    }

    // Whether the module registers store-level settings, secret and hidden ones included.
    protected virtual bool HasStoreSettings(string moduleId)
    {
        return (settingsRegistrar.GetSettingsForType(nameof(Store)) ?? []).Any(x => x.ModuleId.EqualsIgnoreCase(moduleId.Trim()));
    }

    protected virtual bool HasWord(McpSetupSetting setting, string word)
    {
        return Contains(setting.Name, word) || Contains(setting.Title, word) || Contains(setting.Description, word);
    }

    private static bool Contains(string text, string word)
    {
        return text?.ContainsIgnoreCase(word) == true;
    }
}
