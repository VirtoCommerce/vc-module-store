using System.Collections.Generic;
using System.ComponentModel;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Store's own fields in a setup. A field that is left out changes nothing; a list is the complete new list.
public class StoreBasicsSection
{
    [Description("Store name. Required for a new store.")]
    public string Name { get; set; }

    [Description("Store description.")]
    public string Description { get; set; }

    [Description("Default language code, one of languages, such as en-US.")]
    public string DefaultLanguage { get; set; }

    [Description("Language codes of the store: the complete list.")]
    public IList<string> Languages { get; set; }

    [Description("Default currency code, one of currencies, such as USD.")]
    public string DefaultCurrency { get; set; }

    [Description("Currency codes of the store: the complete list.")]
    public IList<string> Currencies { get; set; }

    [Description("Storefront URL, used in links of emails and of the checkout handoff.")]
    public string Url { get; set; }

    [Description("Storefront HTTPS URL.")]
    public string SecureUrl { get; set; }

    [Description("Store email: the sender of the store's notifications.")]
    public string Email { get; set; }

    [Description("Administrator email: the recipient of the store's notifications to its staff.")]
    public string AdminEmail { get; set; }

    [Description("Time zone id, as the platform lists time zones.")]
    public string TimeZone { get; set; }

    [Description("Country code, as the platform lists countries (ISO 3166-1 alpha-3, such as DEU).")]
    public string Country { get; set; }

    [Description("Region or state.")]
    public string Region { get; set; }

    [Description("Id of an existing fulfillment center: the store's main one. Not checked.")]
    public string MainFulfillmentCenterId { get; set; }

    [Description("Ids of existing fulfillment centers whose stock the store also sells: the complete list. Not checked.")]
    public IList<string> AdditionalFulfillmentCenterIds { get; set; }
}
