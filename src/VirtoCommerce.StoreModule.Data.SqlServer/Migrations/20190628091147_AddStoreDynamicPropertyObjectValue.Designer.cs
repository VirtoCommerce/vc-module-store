// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.SqlServer.Migrations
{
    [DbContext(typeof(StoreDbContext))]
    [Migration("20190628091147_AddStoreDynamicPropertyObjectValue")]
    partial class AddStoreDynamicPropertyObjectValue
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate");

                    b.Property<string>("ImageAltDescription")
                        .HasMaxLength(255);

                    b.Property<bool>("IsActive");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Language")
                        .HasMaxLength(5);

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(1024);

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(255);

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128);

                    b.Property<string>("Title")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreSeoInfo");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreCurrencyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("StoreId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreCurrency");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreDynamicPropertyObjectValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<bool?>("BooleanValue");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate");

                    b.Property<DateTime?>("DateTimeValue");

                    b.Property<decimal?>("DecimalValue")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("DictionaryItemId")
                        .HasMaxLength(128);

                    b.Property<int?>("IntegerValue");

                    b.Property<string>("Locale")
                        .HasMaxLength(64);

                    b.Property<string>("LongTextValue");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<string>("ObjectId")
                        .HasMaxLength(128);

                    b.Property<string>("ObjectType")
                        .HasMaxLength(256);

                    b.Property<string>("PropertyId")
                        .HasMaxLength(128);

                    b.Property<string>("PropertyName")
                        .HasMaxLength(256);

                    b.Property<string>("ShortTextValue")
                        .HasMaxLength(512);

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("ObjectType", "ObjectId")
                        .HasName("IX_ObjectType_ObjectId");

                    b.ToTable("StoreDynamicPropertyObjectValue");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("AdminEmail")
                        .HasMaxLength(128);

                    b.Property<string>("Catalog")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("Country")
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int>("CreditCardSavePolicy");

                    b.Property<string>("DefaultCurrency")
                        .HasMaxLength(64);

                    b.Property<string>("DefaultLanguage")
                        .HasMaxLength(128);

                    b.Property<string>("Description")
                        .HasMaxLength(256);

                    b.Property<bool>("DisplayOutOfStock");

                    b.Property<string>("Email")
                        .HasMaxLength(128);

                    b.Property<string>("FulfillmentCenterId")
                        .HasMaxLength(128);

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("OuterId")
                        .HasMaxLength(128);

                    b.Property<string>("Region")
                        .HasMaxLength(128);

                    b.Property<string>("ReturnsFulfillmentCenterId")
                        .HasMaxLength(128);

                    b.Property<string>("SecureUrl")
                        .HasMaxLength(128);

                    b.Property<int>("StoreState");

                    b.Property<string>("TimeZone")
                        .HasMaxLength(128);

                    b.Property<string>("Url")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("FulfillmentCenterId")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("StoreId")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreFulfillmentCenter");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<string>("StoreId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreLanguage");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128);

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasMaxLength(128);

                    b.Property<string>("StoreId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreTrustedGroup");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("SeoInfos")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreCurrencyEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("Currencies")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreDynamicPropertyObjectValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("DynamicPropertyObjectValues")
                        .HasForeignKey("ObjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("FulfillmentCenters")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("Languages")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("TrustedGroups")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
