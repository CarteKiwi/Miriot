﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Miriot.Api.Models;
using Miriot.Common.Model;
using System;

namespace Miriot.Api.Migrations
{
    [DbContext(typeof(MiriotContext))]
    partial class MiriotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Miriot.Common.Model.MiriotConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("MiriotDeviceId");

                    b.Property<string>("Name");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("Miriot.Common.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Emotion");

                    b.Property<DateTime?>("LastLoginDate");

                    b.Property<string>("Name");

                    b.Property<byte[]>("Picture");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Miriot.Common.Model.Widget", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Infos");

                    b.Property<int>("MiriotConfigurationId");

                    b.Property<int>("Type");

                    b.Property<int?>("X");

                    b.Property<int?>("Y");

                    b.HasKey("Id");

                    b.HasIndex("MiriotConfigurationId");

                    b.ToTable("Widgets");
                });

            modelBuilder.Entity("Miriot.Model.ToothbrushingEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<int>("Duration");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ToothbrushingHistory");
                });

            modelBuilder.Entity("Miriot.Common.Model.MiriotConfiguration", b =>
                {
                    b.HasOne("Miriot.Common.Model.User")
                        .WithMany("Devices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Miriot.Common.Model.Widget", b =>
                {
                    b.HasOne("Miriot.Common.Model.MiriotConfiguration")
                        .WithMany("Widgets")
                        .HasForeignKey("MiriotConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Miriot.Model.ToothbrushingEntry", b =>
                {
                    b.HasOne("Miriot.Common.Model.User")
                        .WithMany("ToothbrushingHistory")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
