using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

namespace VirtoCommerce.StoreModule.Tests.MCP.StoreSetup;

// A part whose choices fail, as a module's part does when a service it reads is down. The exception's text must never
// reach an answer.
public class TestFailingChoicesPart : IStoreSetupChoices
{
    public const string ExceptionText = "Connection to https://partner.example.com refused, token sk_live_4242";

    public string Name => "broken";

    public string Title => "Broken part";

    public McpSetupPermissions Permissions => StoreSetupPermissions.ForStore();

    public Task<IList<McpSetupChoice>> GetChoicesAsync()
    {
        throw new InvalidOperationException(ExceptionText);
    }
}
