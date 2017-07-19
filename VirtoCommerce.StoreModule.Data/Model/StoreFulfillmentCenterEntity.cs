using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StoreFulfillmentCenterEntity : Entity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public FulfillmentCenterType Type { get; set; }

        #region Navigation Properties
        public string StoreId { get; set; }

        public StoreEntity Store { get; set; }
        #endregion
    }
}