using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using VirtoCommerce.MCP.Core;
using VirtoCommerce.MCP.Core.Attributes;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.MCP.Core.Services;
using VirtoCommerce.Platform.Core.Common;
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
    IMcpPageSizeResolver pageSizeResolver,
    StoreBackOfficeLinks links)
{
    [McpServerTool(Name = "get_store_settings", ReadOnly = true, Destructive = false, Idempotent = true)]
    [Description("The store-level settings of every installed module for one store, each with its title, description, " +
        "module, type, default, current value, allowed values and whether a setup can change it. Filter by module, by a " +
        "keyword, or to the settings changed from their defaults. Secrets are not listed: a person enters them in the " +
        "back office. Returns one page: when has_more is true, call again with next_skip.")]
    public async Task<object> GetStoreSettings(
        RequestContext<CallToolRequestParams> context,
        [Description("Store id, as list_stores returns it.")] string store_id,
        [Description("Module id, such as VirtoCommerce.Store, to list that module's settings only.")] string module_id = null,
        [Description("A part of the setting's name, title or description.")] string keyword = null,
        [Description("true to list only the settings whose values differ from their defaults.")] bool changed_only = false,
        [Description("Number of settings to skip.")] int? skip = null,
        [Description("Number of settings to return. Omit it for the default page size; larger values are capped.")] int? take = null)
    {
        var store = await access.GetReadableStoreAsync(context.User, store_id);
        var paging = await pageSizeResolver.GetPagingAsync(skip, take);

        var settings = (await setupSettings.GetStoreSettingsAsync(store))
            .Where(x => string.IsNullOrEmpty(module_id) || x.ModuleId.EqualsIgnoreCase(module_id))
            .Where(x => string.IsNullOrEmpty(keyword) || Contains(x.Name, keyword) || Contains(x.Title, keyword) || Contains(x.Description, keyword))
            .Where(x => !changed_only || x.IsChanged);

        return new
        {
            store_id = store.Id,
            name = store.Name,
            settings = McpPage<McpSetupSetting>.FromList(settings, paging),
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
        // The other store has the same settings, so only its values are read.
        var otherValues = otherStore == null ? null : GetValues(otherStore);

        var differences = settings
            .Select(x => new
            {
                name = x.Name,
                title = x.Title,
                module_id = x.ModuleId,
                value = x.Value,
                other_value = otherValues?.GetValueOrDefault(x.Name) ?? x.DefaultValue,
            })
            .Where(x => !McpSetupValues.AreEqual(x.value, x.other_value));

        return new
        {
            store_id = store.Id,
            compared_with = otherStore?.Id ?? "defaults",
            differences = differences.ToMcpPage(paging, x => x),
            next_step = "apply_store_setup with the 'settings' section copies the values you choose.",
        };
    }

    private async Task<string> GetSecretsNoteAsync(string storeName)
    {
        var screen = $"Stores > {storeName} > Settings".WithLink(await links.GetStoresLinkAsync());

        return $"Secrets are not listed. A person enters them in the back office: {screen}.";
    }

    private static Dictionary<string, object> GetValues(Store store)
    {
        return (store.Settings ?? [])
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static bool Contains(string text, string keyword)
    {
        return text?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
    }
}
