namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// The settings of Virto Commerce modules known to be secrets. The list was found by running the discovered-secret rule
// over the settings of the installed modules; a setting the rule cannot find is added here by hand.
public static class StoreSetupKnownSecrets
{
    public static readonly string[] SettingNames =
    [
        "ApplicationInsights.InstrumentationKey",
        "BuilderIO.PublicApiKey",
        "Contentful.DeliveryApiKey",
        "Contentful.PreviewApiKey",
        "Contract.ContractPassword",
        "ElasticAppSearch.ElasticAppSearchPassword",
        "GDPR.GDPRPassword",
        "PlatformBackup.ZipPassword",
        "PushMessages.FcmReceiverOptions.ApiKey",
        "PushMessages.FcmReceiverOptions.VapidKey",
        "Return.ReturnPassword",
        "Sanity.ApiToken",
        "Shipping.Bopis.GoogleMaps.ApiKey",
        "ShipStation.ShipStationPassword",
    ];
}
