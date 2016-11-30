using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Shipping.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
	public class StoreShippingMethodEntity : Entity
	{
		[Required]
		[StringLength(128)]
		public string Code { get; set; }

		public int Priority { get; set; }

		[StringLength(128)]
		public string Name { get; set; }

		public string Description { get; set; }

		[StringLength(2048)]
		public string LogoUrl { get; set; }

		[StringLength(64)]
		public string TaxType { get; set; }

		public bool IsActive { get; set; }


		#region Navigation Properties

		public string StoreId { get; set; }

		public StoreEntity Store { get; set; }

        #endregion

        public virtual ShippingMethod ToModel(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            shippingMethod.IsActive = this.IsActive;
            shippingMethod.Code = this.Code;
            shippingMethod.Description = this.Description;
            shippingMethod.TaxType = this.TaxType;
            shippingMethod.LogoUrl = this.LogoUrl;
            shippingMethod.Name = this.Name;
            shippingMethod.Priority = this.Priority;

            return shippingMethod;
        }

        public virtual StoreShippingMethodEntity FromModel(ShippingMethod shippingMethod, PrimaryKeyResolvingMap pkMap)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            pkMap.AddPair(shippingMethod, this);

            this.IsActive = shippingMethod.IsActive;
            this.Code = shippingMethod.Code;
            this.Description = shippingMethod.Description;
            this.TaxType = shippingMethod.TaxType;
            this.LogoUrl = shippingMethod.LogoUrl;
            this.Name = shippingMethod.Name;
            this.Priority = shippingMethod.Priority;

            return this;
        }

        public virtual void Patch(StoreShippingMethodEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.IsActive = this.IsActive;
            target.Code = this.Code;
            target.Description = this.Description;
            target.TaxType = this.TaxType;
            target.LogoUrl = this.LogoUrl;
            target.Name = this.Name;
            target.Priority = this.Priority;
        }
    }
}
