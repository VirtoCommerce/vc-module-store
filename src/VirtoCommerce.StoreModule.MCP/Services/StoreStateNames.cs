using ModelContextProtocol;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.MCP.Services;

// The store states as the tools name them.
public static class StoreStateNames
{
    public const string Open = "open";
    public const string Closed = "closed";
    public const string RestrictedAccess = "restricted_access";

    public static string ToName(StoreState state)
    {
        return state.ToString().ToSnakeCase();
    }

    public static StoreState Parse(string name)
    {
        return name?.Trim().ToLowerInvariant() switch
        {
            Open => StoreState.Open,
            Closed => StoreState.Closed,
            RestrictedAccess or "restrictedaccess" => StoreState.RestrictedAccess,
            _ => throw new McpException($"Unknown state '{name}'. Use {Open}, {Closed} or {RestrictedAccess}."),
        };
    }
}
