using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StoreLanguageEntity : Entity
    {
		[Required]
		[StringLength(32)]
		public string LanguageCode { get; set; }

        #region Navigation Properties
		public string StoreId { get; set; }
        public StoreEntity Store { get; set; }

        #endregion
    }
}
