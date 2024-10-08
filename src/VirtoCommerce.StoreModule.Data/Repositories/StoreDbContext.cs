using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
            #region Store
            modelBuilder.Entity<StoreEntity>().ToTable("Store").HasKey(x => x.Id);
            modelBuilder.Entity<StoreEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            #endregion

            #region StoreCurrency
            modelBuilder.Entity<StoreCurrencyEntity>().ToTable("StoreCurrency").HasKey(x => x.Id);
            modelBuilder.Entity<StoreCurrencyEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreCurrencyEntity>().HasOne(x => x.Store).WithMany(x => x.Currencies)
                .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region StoreLanguage
            modelBuilder.Entity<StoreLanguageEntity>().ToTable("StoreLanguage").HasKey(x => x.Id);
            modelBuilder.Entity<StoreLanguageEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreLanguageEntity>().HasOne(x => x.Store).WithMany(x => x.Languages)
                .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region StoreTrustedGroups
            modelBuilder.Entity<StoreTrustedGroupEntity>().ToTable("StoreTrustedGroup").HasKey(x => x.Id);
            modelBuilder.Entity<StoreTrustedGroupEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreTrustedGroupEntity>().HasOne(x => x.Store).WithMany(x => x.TrustedGroups)
                .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region FulfillmentCenters
            modelBuilder.Entity<StoreFulfillmentCenterEntity>().ToTable("StoreFulfillmentCenter").HasKey(x => x.Id);
            modelBuilder.Entity<StoreFulfillmentCenterEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreFulfillmentCenterEntity>().HasOne(x => x.Store).WithMany(x => x.FulfillmentCenters)
                .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region SeoInfo
            modelBuilder.Entity<SeoInfoEntity>().ToTable("StoreSeoInfo").HasKey(x => x.Id);
            modelBuilder.Entity<SeoInfoEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<SeoInfoEntity>().HasOne(x => x.Store).WithMany(x => x.SeoInfos).HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region DynamicProperty

            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>().ToTable("StoreDynamicPropertyObjectValue").HasKey(x => x.Id);
            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>().Property(x => x.DecimalValue).HasColumnType("decimal(18,5)");
            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>().HasOne(p => p.Store)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.ObjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StoreDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.ObjectId })
                        .IsUnique(false)
                        .HasDatabaseName("IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId");
            #endregion

            modelBuilder.Entity<StoreAuthenticationSchemeEntity>().ToTable("StoreAuthenticationScheme").HasKey(x => x.Id);
            modelBuilder.Entity<StoreAuthenticationSchemeEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<StoreAuthenticationSchemeEntity>().HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StoreAuthenticationSchemeEntity>().HasIndex(x => new { x.StoreId, x.Name }).IsUnique()
                .HasDatabaseName("IX_StoreAuthenticationScheme_StoreId_Name");


            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.StoreModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
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
