using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StoreEntity : AuditableEntity
    {
        public StoreEntity()
        {
            Languages = new NullCollection<StoreLanguageEntity>();
            Currencies = new NullCollection<StoreCurrencyEntity>();
            PaymentMethods = new NullCollection<StorePaymentMethodEntity>();
            ShippingMethods = new NullCollection<StoreShippingMethodEntity>();
            TaxProviders = new NullCollection<StoreTaxProviderEntity>();
            TrustedGroups = new NullCollection<StoreTrustedGroupEntity>();
            FulfillmentCenters = new NullCollection<StoreFulfillmentCenterEntity>();
        }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [StringLength(256)]
        public string Url { get; set; }

        public int StoreState { get; set; }

        [StringLength(128)]
        public string TimeZone { get; set; }

        [StringLength(128)]
        public string Country { get; set; }

        [StringLength(128)]
        public string Region { get; set; }

        [StringLength(128)]
        public string DefaultLanguage { get; set; }

        [StringLength(64)]
        public string DefaultCurrency { get; set; }

        [StringLength(128)]
        [Required]
        public string Catalog { get; set; }

        public int CreditCardSavePolicy { get; set; }

        [StringLength(128)]
        public string SecureUrl { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        [StringLength(128)]
        public string AdminEmail { get; set; }

        public bool DisplayOutOfStock { get; set; }

        [StringLength(128)]
        public string FulfillmentCenterId { get; set; }
        [StringLength(128)]
        public string ReturnsFulfillmentCenterId { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<StoreLanguageEntity> Languages { get; set; }

        public virtual ObservableCollection<StoreCurrencyEntity> Currencies { get; set; }
        public virtual ObservableCollection<StoreTrustedGroupEntity> TrustedGroups { get; set; }

        public virtual ObservableCollection<StorePaymentMethodEntity> PaymentMethods { get; set; }
        public virtual ObservableCollection<StoreShippingMethodEntity> ShippingMethods { get; set; }
        public virtual ObservableCollection<StoreTaxProviderEntity> TaxProviders { get; set; }

        public virtual ObservableCollection<StoreFulfillmentCenterEntity> FulfillmentCenters { get; set; }
        #endregion

        public virtual Store ToModel(Store store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            store.Id = this.Id;
            store.AdminEmail = this.AdminEmail;
            store.Catalog = this.Catalog;
            store.Country = this.Country;
            store.CreatedBy = this.CreatedBy;
            store.CreatedDate = this.CreatedDate;
            store.DefaultCurrency = this.DefaultCurrency;
            store.DefaultLanguage = this.DefaultLanguage;
            store.Description = this.Description;
            store.DisplayOutOfStock = this.DisplayOutOfStock;
            store.Email = this.Email;
            store.ModifiedBy = this.ModifiedBy;
            store.ModifiedDate = this.ModifiedDate;
            store.Name = this.Name;
            store.Region = this.Region;
            store.SecureUrl = this.SecureUrl;
            store.TimeZone = this.TimeZone;
            store.Url = this.Url;

            store.StoreState = EnumUtility.SafeParse<StoreState>(this.StoreState.ToString(), Domain.Store.Model.StoreState.Open);
            store.Languages = this.Languages.Select(x => x.LanguageCode).ToList();
            store.Currencies = this.Currencies.Select(x => x.CurrencyCode).ToList();
            store.TrustedGroups = this.TrustedGroups.Select(x => x.GroupName).ToList();

            return store;
        }

        public virtual StoreEntity FromModel(Store store, PrimaryKeyResolvingMap pkMap)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            pkMap.AddPair(store, this);

            this.Id = store.Id;
            this.AdminEmail = store.AdminEmail;
            this.Catalog = store.Catalog;
            this.Country = store.Country;
            this.CreatedBy = store.CreatedBy;
            this.CreatedDate = store.CreatedDate;
            this.DefaultCurrency = store.DefaultCurrency;
            this.DefaultLanguage = store.DefaultLanguage;
            this.Description = store.Description;
            this.DisplayOutOfStock = store.DisplayOutOfStock;
            this.Email = store.Email;
            this.ModifiedBy = store.ModifiedBy;
            this.ModifiedDate = store.ModifiedDate;
            this.Name = store.Name;
            this.Region = store.Region;
            this.SecureUrl = store.SecureUrl;
            this.TimeZone = store.TimeZone;
            this.Url = store.Url;
            this.StoreState = (int)store.StoreState;

            if (store.DefaultCurrency != null)
            {
                this.DefaultCurrency = store.DefaultCurrency.ToString();
            }
            if (store.FulfillmentCenter != null)
            {
                this.FulfillmentCenterId = store.FulfillmentCenter.Id;
            }
            if (store.ReturnsFulfillmentCenter != null)
            {
                this.ReturnsFulfillmentCenterId = store.ReturnsFulfillmentCenter.Id;
            }
            if (store.Languages != null)
            {
                this.Languages = new ObservableCollection<StoreLanguageEntity>(store.Languages.Select(x => new StoreLanguageEntity
                {
                    LanguageCode = x
                }));
            }

            if (store.Currencies != null)
            {
                this.Currencies = new ObservableCollection<StoreCurrencyEntity>(store.Currencies.Select(x => new StoreCurrencyEntity
                {
                    CurrencyCode = x.ToString()
                }));
            }

            if (store.TrustedGroups != null)
            {
                this.TrustedGroups = new ObservableCollection<StoreTrustedGroupEntity>(store.TrustedGroups.Select(x => new StoreTrustedGroupEntity
                {
                    GroupName = x
                }));
            }

            if (store.ShippingMethods != null)
            {
                this.ShippingMethods = new ObservableCollection<StoreShippingMethodEntity>(store.ShippingMethods.Select(x => AbstractTypeFactory<StoreShippingMethodEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (store.PaymentMethods != null)
            {
                this.PaymentMethods = new ObservableCollection<StorePaymentMethodEntity>(store.PaymentMethods.Select(x => AbstractTypeFactory<StorePaymentMethodEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (store.TaxProviders != null)
            {
                this.TaxProviders = new ObservableCollection<StoreTaxProviderEntity>(store.TaxProviders.Select(x => AbstractTypeFactory<StoreTaxProviderEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (store.FulfillmentCenters != null || store.ReturnsFulfillmentCenters != null)
            {
                this.FulfillmentCenters = new ObservableCollection<StoreFulfillmentCenterEntity>();
                if (store.FulfillmentCenters != null)
                {
                    this.FulfillmentCenters.AddRange(store.FulfillmentCenters.Select(fc => new StoreFulfillmentCenterEntity
                    {
                        FulfillmentCenterId = fc.Id,
                        Name = fc.Name,
                        StoreId = store.Id,
                        Type = FulfillmentCenterType.Main
                    }));
                }
                if (store.ReturnsFulfillmentCenters != null)
                {
                    this.FulfillmentCenters.AddRange(store.ReturnsFulfillmentCenters.Select(fc => new StoreFulfillmentCenterEntity
                    {
                        FulfillmentCenterId = fc.Id,
                        Name = fc.Name,
                        StoreId = store.Id,
                        Type = FulfillmentCenterType.Returns
                    }));
                }
            }

            return this;
        }

        public virtual void Patch(StoreEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.AdminEmail = this.AdminEmail;
            target.Catalog = this.Catalog;
            target.Country = this.Country;
            target.DefaultCurrency = this.DefaultCurrency;
            target.DefaultLanguage = this.DefaultLanguage;
            target.Description = this.Description;
            target.DisplayOutOfStock = this.DisplayOutOfStock;
            target.Email = this.Email;
            target.ModifiedBy = this.ModifiedBy;
            target.ModifiedDate = this.ModifiedDate;
            target.Name = this.Name;
            target.Region = this.Region;
            target.SecureUrl = this.SecureUrl;
            target.TimeZone = this.TimeZone;
            target.Url = this.Url;
            target.StoreState = (int)this.StoreState;
            target.FulfillmentCenterId = this.FulfillmentCenterId;
            target.ReturnsFulfillmentCenterId = this.ReturnsFulfillmentCenterId;

            if (!this.Languages.IsNullCollection())
            {
                var languageComparer = AnonymousComparer.Create((StoreLanguageEntity x) => x.LanguageCode);
                this.Languages.Patch(target.Languages, languageComparer,
                                      (sourceLang, targetLang) => targetLang.LanguageCode = sourceLang.LanguageCode);
            }
            if (!this.Currencies.IsNullCollection())
            {
                var currencyComparer = AnonymousComparer.Create((StoreCurrencyEntity x) => x.CurrencyCode);
                this.Currencies.Patch(target.Currencies, currencyComparer,
                                      (sourceCurrency, targetCurrency) => targetCurrency.CurrencyCode = sourceCurrency.CurrencyCode);
            }
            if (!this.TrustedGroups.IsNullCollection())
            {
                var trustedGroupComparer = AnonymousComparer.Create((StoreTrustedGroupEntity x) => x.GroupName);
                this.TrustedGroups.Patch(target.TrustedGroups, trustedGroupComparer,
                                      (sourceGroup, targetGroup) => sourceGroup.GroupName = targetGroup.GroupName);
            }

            if (!this.PaymentMethods.IsNullCollection())
            {
                var paymentComparer = AnonymousComparer.Create((StorePaymentMethodEntity x) => x.Code);
                this.PaymentMethods.Patch(target.PaymentMethods, paymentComparer,
                                      (sourceMethod, targetMethod) => sourceMethod.Patch(targetMethod));
            }
            if (!this.ShippingMethods.IsNullCollection())
            {
                var shippingComparer = AnonymousComparer.Create((StoreShippingMethodEntity x) => x.Code);
                this.ShippingMethods.Patch(target.ShippingMethods, shippingComparer,
                                      (sourceMethod, targetMethod) => sourceMethod.Patch(targetMethod));
            }
            if (!this.TaxProviders.IsNullCollection())
            {
                var shippingComparer = AnonymousComparer.Create((StoreTaxProviderEntity x) => x.Code);
                this.TaxProviders.Patch(target.TaxProviders, shippingComparer,
                                      (sourceProvider, targetProvider) => sourceProvider.Patch(targetProvider));
            }
            if (!this.FulfillmentCenters.IsNullCollection())
            {
                var fulfillmentCenterComparer = AnonymousComparer.Create((StoreFulfillmentCenterEntity fc) => $"{fc.FulfillmentCenterId}-{fc.Type}");
                this.FulfillmentCenters.Patch(target.FulfillmentCenters, fulfillmentCenterComparer,
                                      (sourceFulfillmentCenter, targetFulfillmentCenter) => sourceFulfillmentCenter.Patch(targetFulfillmentCenter));
            }
        }

        public static ValidationResult ValidateStoreId(string value, ValidationContext context)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new ValidationResult("Code can't be empty");
            }

            const string invalidKeywordCharacters = @"$+;=%{}[]|\/@ ~#!^*&?:'<>,";

            if (value.IndexOfAny(invalidKeywordCharacters.ToCharArray()) > -1)
            {
                return new ValidationResult(@"Code must be valid");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}