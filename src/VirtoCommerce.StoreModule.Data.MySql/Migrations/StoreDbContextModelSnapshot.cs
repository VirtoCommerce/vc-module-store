﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.StoreModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.StoreModule.Data.MySql.Migrations
{
    [DbContext(typeof(StoreDbContext))]
    partial class StoreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ImageAltDescription")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Language")
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Title")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreSeoInfo", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreAuthenticationSchemeEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId", "Name")
                        .IsUnique()
                        .HasDatabaseName("IX_StoreAuthenticationScheme_StoreId_Name");

                    b.ToTable("StoreAuthenticationScheme", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreCurrencyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreCurrency", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreDynamicPropertyObjectValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool?>("BooleanValue")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal?>("DecimalValue")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("DictionaryItemId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int?>("IntegerValue")
                        .HasColumnType("int");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("LongTextValue")
                        .HasColumnType("longtext");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ObjectId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ObjectType")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PropertyId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("PropertyName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("ShortTextValue")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("ObjectType", "ObjectId")
                        .HasDatabaseName("IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId");

                    b.ToTable("StoreDynamicPropertyObjectValue", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("AdminEmail")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("AdminEmailName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("Catalog")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Country")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreditCardSavePolicy")
                        .HasColumnType("int");

                    b.Property<string>("DefaultCurrency")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("DefaultLanguage")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("DisplayOutOfStock")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Email")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("EmailName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("FulfillmentCenterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Region")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ReturnsFulfillmentCenterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("SecureUrl")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int>("StoreState")
                        .HasColumnType("int");

                    b.Property<string>("TimeZone")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Url")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.ToTable("Store", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("FulfillmentCenterId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreFulfillmentCenter", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreLanguage", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreTrustedGroup", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("SeoInfos")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreAuthenticationSchemeEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreCurrencyEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("Currencies")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreDynamicPropertyObjectValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("DynamicPropertyObjectValues")
                        .HasForeignKey("ObjectId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("FulfillmentCenters")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("Languages")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("TrustedGroups")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreEntity", b =>
                {
                    b.Navigation("Currencies");

                    b.Navigation("DynamicPropertyObjectValues");

                    b.Navigation("FulfillmentCenters");

                    b.Navigation("Languages");

                    b.Navigation("SeoInfos");

                    b.Navigation("TrustedGroups");
                });
#pragma warning restore 612, 618
        }
    }
}
