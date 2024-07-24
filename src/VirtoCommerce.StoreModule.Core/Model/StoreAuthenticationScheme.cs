using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.StoreModule.Core.Model;

public class StoreAuthenticationScheme : AuditableEntity, ICloneable
{
    public string StoreId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
