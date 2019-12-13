using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Domain.Shipping.Model;
using VirtoCommerce.Domain.Shipping.Services;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Domain.Tax.Model;
using VirtoCommerce.Domain.Tax.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Web.JsonConverters
{
    public class PolymorphicStoreJsonConverter : JsonConverter
    {
        private static Type[] _knowTypes = new[] { typeof(Store), typeof(SearchCriteria), typeof(PaymentMethod), typeof(ShippingMethod), typeof(TaxProvider) };

        private readonly IPaymentMethodsService _paymentMethodsService;
        private readonly IShippingMethodsService _shippingMethodsService;
        private readonly ITaxService _taxService;
        public PolymorphicStoreJsonConverter(IPaymentMethodsService paymentMethodsService, IShippingMethodsService shippingMethodsService, ITaxService taxService)
        {
            _paymentMethodsService = paymentMethodsService;
            _shippingMethodsService = shippingMethodsService;
            _taxService = taxService;
        }

        public override bool CanWrite { get { return false; } }
        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = null;
            var obj = JObject.Load(reader);
          
            if (typeof(Store).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<Store>.TryCreateInstance();
            }      
            else if (typeof(SearchCriteria).IsAssignableFrom(objectType))
            {
                retVal = AbstractTypeFactory<SearchCriteria>.TryCreateInstance();
            }
            else if (objectType == typeof(PaymentMethod))
            {
                var paymentGatewayCode = obj["code"].Value<string>();
                retVal = _paymentMethodsService.GetAllPaymentMethods().FirstOrDefault(x => x.Code.EqualsInvariant(paymentGatewayCode));
            }
            else if (typeof(ShippingMethod).IsAssignableFrom(objectType))
            {
                var shippingGatewayCode = obj["code"].Value<string>();
                var shippingMethod = _shippingMethodsService.GetAllShippingMethods().FirstOrDefault(x => x.Code.EqualsInvariant(shippingGatewayCode));
                if (shippingMethod != null && objectType.IsAssignableFrom(shippingMethod.GetType()))
                {
                    retVal = shippingMethod;
                }
            }
            else if (objectType == typeof(TaxProvider))
            {
                var taxProviderCode = obj["code"].Value<string>();
                retVal = _taxService.GetAllTaxProviders().FirstOrDefault(x => x.Code.EqualsInvariant(taxProviderCode));
            }
            serializer.Populate(obj.CreateReader(), retVal);
            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
