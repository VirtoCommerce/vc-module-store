﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.StoreModule.Data.Repositories;

namespace VirtoCommerce.StoreModule.Data.Migrations
{
    [DbContext(typeof(StoreDbContext))]
    partial class StoreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageAltDescription")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Language")
                        .HasColumnType("nvarchar(5)")
                        .HasMaxLength(5);

                    b.Property<string>("MetaDescription")
                        .HasColumnType("nvarchar(1024)")
                        .HasMaxLength(1024);

                    b.Property<string>("MetaKeywords")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreSeoInfo");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreCurrencyEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreCurrency");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreDynamicPropertyObjectValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<bool?>("BooleanValue")
                        .HasColumnType("bit");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("DecimalValue")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("DictionaryItemId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<int?>("IntegerValue")
                        .HasColumnType("int");

                    b.Property<string>("Locale")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("LongTextValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ObjectId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ObjectType")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PropertyId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("PropertyName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("ShortTextValue")
                        .HasColumnType("nvarchar(512)")
                        .HasMaxLength(512);

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
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
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("AdminEmail")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Catalog")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreditCardSavePolicy")
                        .HasColumnType("int");

                    b.Property<string>("DefaultCurrency")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<string>("DefaultLanguage")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("DisplayOutOfStock")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("FulfillmentCenterId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("OuterId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ReturnsFulfillmentCenterId")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("SecureUrl")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<int>("StoreState")
                        .HasColumnType("int");

                    b.Property<string>("TimeZone")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.ToTable("Store");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreFulfillmentCenterEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("FulfillmentCenterId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreFulfillmentCenter");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(32)")
                        .HasMaxLength(32);

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreLanguage");
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("StoreId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

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
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreLanguageEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("Languages")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtoCommerce.StoreModule.Data.Model.StoreTrustedGroupEntity", b =>
                {
                    b.HasOne("VirtoCommerce.StoreModule.Data.Model.StoreEntity", "Store")
                        .WithMany("TrustedGroups")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
