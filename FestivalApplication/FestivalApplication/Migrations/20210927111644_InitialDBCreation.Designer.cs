﻿// <auto-generated />
using System;
using FestivalApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FestivalApplication.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20210927111644_InitialDBCreation")]
    partial class InitialDBCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("FestivalApplication.Model.Authentication", b =>
                {
                    b.Property<int>("AuthenticationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AuthenticationKey")
                        .HasColumnType("text");

                    b.Property<DateTime>("CurrentExpiryDate")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("MaxExpiryDate")
                        .HasColumnType("datetime");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("AuthenticationID");

                    b.HasIndex("UserID");

                    b.ToTable("Authentication");
                });

            modelBuilder.Entity("FestivalApplication.Model.Message", b =>
                {
                    b.Property<int>("MessageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("MessageText")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime");

                    b.Property<int?>("UserActivityID")
                        .HasColumnType("int");

                    b.HasKey("MessageID");

                    b.HasIndex("UserActivityID");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("FestivalApplication.Model.Stage", b =>
                {
                    b.Property<int>("StageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("StageActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("StageName")
                        .HasColumnType("text");

                    b.HasKey("StageID");

                    b.ToTable("Stage");
                });

            modelBuilder.Entity("FestivalApplication.Model.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("PassWord")
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("UserID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("FestivalApplication.Model.UserActivity", b =>
                {
                    b.Property<int>("UserActivityID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Entry")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("Exit")
                        .HasColumnType("datetime");

                    b.Property<int>("StageID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("UserActivityID");

                    b.HasIndex("UserID");

                    b.ToTable("UserActivity");
                });

            modelBuilder.Entity("FestivalApplication.Model.Authentication", b =>
                {
                    b.HasOne("FestivalApplication.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FestivalApplication.Model.Message", b =>
                {
                    b.HasOne("FestivalApplication.Model.UserActivity", "UserActivity")
                        .WithMany("MessageHistory")
                        .HasForeignKey("UserActivityID");

                    b.Navigation("UserActivity");
                });

            modelBuilder.Entity("FestivalApplication.Model.UserActivity", b =>
                {
                    b.HasOne("FestivalApplication.Model.User", null)
                        .WithMany("Log")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FestivalApplication.Model.User", b =>
                {
                    b.Navigation("Log");
                });

            modelBuilder.Entity("FestivalApplication.Model.UserActivity", b =>
                {
                    b.Navigation("MessageHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
