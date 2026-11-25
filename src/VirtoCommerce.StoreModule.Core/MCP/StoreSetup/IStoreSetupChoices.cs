using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// What can be chosen on this installation, such as languages, currencies or method codes.
public interface IStoreSetupChoices : IStoreSetupPart
{
    Task<IList<McpSetupChoice>> GetChoicesAsync();
}
