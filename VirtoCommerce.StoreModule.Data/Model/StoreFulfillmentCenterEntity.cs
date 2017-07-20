using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StoreFulfillmentCenterEntity : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public string Type { get; set; }

        public string FulfillmentCenterId { get; set; }

        #region Navigation Properties
        public string StoreId { get; set; }

        public StoreEntity Store { get; set; }
        #endregion

        public virtual void Patch(StoreFulfillmentCenterEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            target.FulfillmentCenterId = target.FulfillmentCenterId;
            target.Id = this.Id;
            target.Name = this.Name;
            target.StoreId = this.StoreId;
            target.Name = this.Name;
            target.Type = this.Type;
        }
    }
}