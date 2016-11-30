using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Tax.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Data.Model
{
    public class StoreTaxProviderEntity : Entity
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


        #region Navigation Properties

        public string StoreId { get; set; }

        public StoreEntity Store { get; set; }

        #endregion


        public virtual TaxProvider ToModel(TaxProvider taxProvider)
        {
            if (taxProvider == null)
                throw new ArgumentNullException("taxProvider");

            taxProvider.IsActive = this.IsActive;
            taxProvider.Code = this.Code;
            taxProvider.Description = this.Description;
            taxProvider.LogoUrl = this.LogoUrl;
            taxProvider.Name = this.Name;
            taxProvider.Priority = this.Priority;

            return taxProvider;
        }

        public virtual StoreTaxProviderEntity FromModel(TaxProvider taxProvider, PrimaryKeyResolvingMap pkMap)
        {
            if (taxProvider == null)
                throw new ArgumentNullException("taxProvider");

            pkMap.AddPair(taxProvider, this);

            this.IsActive = taxProvider.IsActive;
            this.Code = taxProvider.Code;
            this.Description = taxProvider.Description;
            this.LogoUrl = taxProvider.LogoUrl;
            this.Name = taxProvider.Name;
            this.Priority = taxProvider.Priority;

            return this;
        }

        public virtual void Patch(StoreTaxProviderEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.IsActive = this.IsActive;
            target.Code = this.Code;
            target.Description = this.Description;
            target.LogoUrl = this.LogoUrl;
            target.Name = this.Name;
            target.Priority = this.Priority;
        }
    }
}
