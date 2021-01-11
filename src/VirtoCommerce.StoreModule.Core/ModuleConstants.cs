using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.StoreModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "store:read",
                    Create = "store:create",
                    Access = "store:access",
                    Update = "store:update",
                    Delete = "store:delete";

                public static readonly string[] AllPermissions = new[] { Access, Create, Read, Update, Delete };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor States = new SettingDescriptor
                {
                    Name = "Stores.States",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Store|General",
                    IsDictionary = true,
                    DefaultValue = "Open",
                    AllowedValues = new[] { "Open", "Closed", "RestrictedAccess" },
                    IsHidden = true
                };

                public static SettingDescriptor TaxCalculationEnabled = new SettingDescriptor
                {
                    Name = "Stores.TaxCalculationEnabled",
                    GroupName = "Store|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor AllowAnonymousUsers = new SettingDescriptor
                {
                    Name = "Stores.AllowAnonymousUsers",
                    GroupName = "Store|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor IsSpa = new SettingDescriptor
                {
                    Name = "Stores.IsSpa",
                    GroupName = "Store|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };
                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return States;
                        yield return TaxCalculationEnabled;
                        yield return AllowAnonymousUsers;
                        yield return IsSpa;
                    }
                }
            }

            public static class SEO
            {
                public static SettingDescriptor SeoLinksType = new SettingDescriptor
                {
                    Name = "Stores.SeoLinksType",
                    GroupName = "Store|SEO",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "Collapsed",
                    AllowedValues = new[] { "None", "Short", "Collapsed", "Long" }
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return SeoLinksType;
                    }
                }
            }
            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings.Concat(SEO.AllSettings);
                }
            }
        }
    }
}
