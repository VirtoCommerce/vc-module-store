using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.StoreModule.Core.Model
{
    public class Store : AuditableEntity, IHasDynamicProperties, IHasSettings, ISeoSupport, ISupportSecurityScopes, IHasOuterId, ICloneable
    {
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Store current state (Open, Closed, RestrictedAccess)
        /// </summary>
        public StoreState StoreState { get; set; }

        public string TimeZone { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string DefaultLanguage { get; set; }

        public string DefaultCurrency { get; set; }
        /// <summary>
        /// Catalog id used as primary store catalog
        /// </summary>
        public string Catalog { get; set; }
        public bool CreditCardSavePolicy { get; set; }
        /// <summary>
        /// Store storefront url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Store storefront https url
        /// </summary>
        public string SecureUrl { get; set; }
        /// <summary>
        /// Primary store contact email can be used for store event notifications and for feed back
        /// </summary>
        public string Email { get; set; }
        public string AdminEmail { get; set; }
        public string EmailName { get; set; }
        public string AdminEmailName { get; set; }
        [JsonIgnore]
        public string EmailWithName
        {
            get
            {
                return FormatEmailWithName(Email, EmailName);
            }
        }
        [JsonIgnore]
        public string AdminEmailWithName
        {
            get
            {
                return FormatEmailWithName(AdminEmail, AdminEmailName);
            }
        }
        public bool DisplayOutOfStock { get; set; }
        public string OuterId { get; set; }

        /// <summary>
        /// Primary (default) fulfillment center id
        /// </summary>
        public string MainFulfillmentCenterId { get; set; }
        /// <summary>
        /// Alternate fulfillment centers ids
        /// </summary>
        public ICollection<string> AdditionalFulfillmentCenterIds { get; set; }
        /// <summary>
        /// Primary (default) fulfillment center for order return
        /// </summary>
        public string MainReturnsFulfillmentCenterId { get; set; }
        /// <summary>
        /// Alternate fulfillment centers for order return
        /// </summary>
        public ICollection<string> ReturnsFulfillmentCenterIds { get; set; }

        /// <summary>
        /// All store supported languages
        /// </summary>
        public ICollection<string> Languages { get; set; }
        /// <summary>
        /// All store supported currencies
        /// </summary>
        public ICollection<string> Currencies { get; set; }
        /// <summary>
        /// All store trusted groups (group of stores that shared the user logins)
        /// </summary>
        public ICollection<string> TrustedGroups { get; set; }


        #region ISeoSupport Members
        public string SeoObjectType => nameof(Store);
        public IList<SeoInfo> SeoInfos { get; set; }
        #endregion

        #region IHasDynamicProperties Members
        public string ObjectType => typeof(Store).FullName;
        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }
        #endregion

        #region IHasSettings Members
        public ICollection<ObjectSettingEntry> Settings { get; set; }
        public virtual string TypeName => nameof(Store);
        #endregion

        #region ISupportSecurityScopes Members
        public IEnumerable<string> Scopes { get; set; }
        #endregion

        #region ICloneable members

        public virtual object Clone()
        {
            var result = MemberwiseClone() as Store;

            result.SeoInfos = SeoInfos?.Select(x => x.Clone()).OfType<SeoInfo>().ToList();
            result.Settings = Settings?.Select(x => x.Clone()).OfType<ObjectSettingEntry>().ToList();

            return result;
        }

        #endregion

        /// <summary>
        /// Format email and name into single address string 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string FormatEmailWithName(string email, string name)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            else
            {
                return string.IsNullOrEmpty(name) ? email : $@"""{name}"" <{email}>";
            }
        }
    }
}
