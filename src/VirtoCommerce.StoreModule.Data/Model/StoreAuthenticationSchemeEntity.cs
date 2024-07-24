using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.StoreModule.Data.Model;

public class StoreAuthenticationSchemeEntity : AuditableEntity, IDataEntity<StoreAuthenticationSchemeEntity, StoreAuthenticationScheme>
{
    [Required]
    [StringLength(128)]
    public string StoreId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    public bool IsActive { get; set; }

    public int Position { get; set; }

    public StoreEntity Store { get; set; }

    public virtual StoreAuthenticationScheme ToModel(StoreAuthenticationScheme model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.StoreId = StoreId;
        model.Name = Name;
        model.IsActive = IsActive;
        model.Position = Position;

        return model;
    }

    public virtual StoreAuthenticationSchemeEntity FromModel(StoreAuthenticationScheme model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        StoreId = model.StoreId;
        Name = model.Name;
        IsActive = model.IsActive;
        Position = model.Position;

        return this;
    }

    public virtual void Patch(StoreAuthenticationSchemeEntity target)
    {
        target.StoreId = StoreId;
        target.Name = Name;
        target.IsActive = IsActive;
        target.Position = Position;
    }
}
