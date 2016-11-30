using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Payment.Model;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StorePaymentMethodEntity : Entity
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

        public bool IsActive { get; set; }

        public bool IsAvailableForPartial { get; set; }

        #region Navigation Properties

        public string StoreId { get; set; }

        public StoreEntity Store { get; set; }

        #endregion

        public virtual PaymentMethod ToModel(PaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            paymentMethod.IsActive = this.IsActive;
            paymentMethod.Code = this.Code;
            paymentMethod.Description = this.Description;
            paymentMethod.IsAvailableForPartial = this.IsAvailableForPartial;
            paymentMethod.LogoUrl = this.LogoUrl;
            paymentMethod.Name = this.Name;
            paymentMethod.Priority = this.Priority;

            return paymentMethod;
        }

        public virtual StorePaymentMethodEntity FromModel(PaymentMethod paymentMethod, PrimaryKeyResolvingMap pkMap)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            pkMap.AddPair(paymentMethod, this);

            this.IsActive = paymentMethod.IsActive;
            this.Code = paymentMethod.Code;
            this.Description = paymentMethod.Description;
            this.IsAvailableForPartial = paymentMethod.IsAvailableForPartial;
            this.LogoUrl = paymentMethod.LogoUrl;
            this.Name = paymentMethod.Name;
            this.Priority = paymentMethod.Priority;

            return this;
        }

        public virtual void Patch(StorePaymentMethodEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.IsActive = this.IsActive;
            target.Code = this.Code;
            target.Description = this.Description;
            target.IsAvailableForPartial = this.IsAvailableForPartial;
            target.LogoUrl = this.LogoUrl;
            target.Name = this.Name;
            target.Priority = this.Priority;
        }
    }
}
