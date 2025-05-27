using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.StoreModule.Data.Model;

namespace VirtoCommerce.StoreModule.Data.Repositories
{
    public class StoreDbContext : DbContextBase
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options)
        {
        }

        protected StoreDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Store
            modelBuilder.Entity<StoreEntity>(builder =>
            {
                builder.ToAuditableEntityTable("Store");
                builder.HasIndex(x => x.OuterId).IsUnique(false);
            });
            #endregion

            #region StoreCurrency
            modelBuilder.Entity<StoreCurrencyEntity>(builder =>
            {
                builder.ToEntityTable("StoreCurrency");
                builder.HasOne(x => x.Store).WithMany(x => x.Currencies)
                    .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region StoreLanguage
            modelBuilder.Entity<StoreLanguageEntity>(builder =>
            {
                builder.ToEntityTable("StoreLanguage");
                builder.HasOne(x => x.Store).WithMany(x => x.Languages)
                    .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region StoreTrustedGroups
            modelBuilder.Entity<StoreTrustedGroupEntity>(builder =>
            {
                builder.ToEntityTable("StoreTrustedGroup");
                builder.HasOne(x => x.Store).WithMany(x => x.TrustedGroups)
                    .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region FulfillmentCenters
            modelBuilder.Entity<StoreFulfillmentCenterEntity>(builder =>
            {
                builder.ToEntityTable("StoreFulfillmentCenter");
                builder.HasOne(x => x.Store).WithMany(x => x.FulfillmentCenters)
                    .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region SeoInfo
            modelBuilder.Entity<SeoInfoEntity>(builder =>
            {
                builder.ToAuditableEntityTable("StoreSeoInfo");
                builder.HasOne(x => x.Store).WithMany(x => x.SeoInfos).HasForeignKey(x => x.StoreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region DynamicProperty
            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>(builder =>
            {
                builder.ToAuditableEntityTable("StoreDynamicPropertyObjectValue");
                builder.Property(x => x.DecimalValue).HasColumnType("decimal(18,5)");
                builder.HasOne(p => p.Store)
                    .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.ObjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                builder.HasIndex(x => new { x.ObjectType, x.ObjectId })
                    .IsUnique(false)
                    .HasDatabaseName("IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId");
            });
            #endregion

            modelBuilder.Entity<StoreAuthenticationSchemeEntity>(builder =>
            {
                builder.ToAuditableEntityTable("StoreAuthenticationScheme");
                builder.HasOne(x => x.Store).WithMany()
                    .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
                builder.HasIndex(x => new { x.StoreId, x.Name }).IsUnique()
                    .HasDatabaseName("IX_StoreAuthenticationScheme_StoreId_Name");
            });

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.StoreModule.Data.XXX project. /> 
            switch (Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StoreModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StoreModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.StoreModule.Data.SqlServer"));
                    break;
            }
        }
    }
}
