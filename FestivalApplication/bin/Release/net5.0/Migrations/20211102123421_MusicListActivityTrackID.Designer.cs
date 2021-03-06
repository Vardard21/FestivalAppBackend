// <auto-generated />
using System;
using FestivalApplication.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FestivalApplication.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20211102123421_MusicListActivityTrackID")]
    partial class MusicListActivityTrackID
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

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

            modelBuilder.Entity("FestivalApplication.Model.Interaction", b =>
                {
                    b.Property<int>("InteractionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("InteractionType")
                        .HasColumnType("int");

                    b.Property<int?>("MessageID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime");

                    b.Property<int?>("UserActivityID")
                        .HasColumnType("int");

                    b.HasKey("InteractionID");

                    b.HasIndex("MessageID");

                    b.HasIndex("UserActivityID");

                    b.ToTable("Interaction");
                });

            modelBuilder.Entity("FestivalApplication.Model.LoyaltyPoints", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("LoyaltyPoints");
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

            modelBuilder.Entity("FestivalApplication.Model.MusicList", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ListName")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("MusicList");
                });

            modelBuilder.Entity("FestivalApplication.Model.MusicListActivity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ListID")
                        .HasColumnType("int");

                    b.Property<int?>("MusicListID")
                        .HasColumnType("int");

                    b.Property<int>("StageID")
                        .HasColumnType("int");

                    b.Property<int>("TrackID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("MusicListID");

                    b.ToTable("MusicListActivity");
                });

            modelBuilder.Entity("FestivalApplication.Model.Stage", b =>
                {
                    b.Property<int>("StageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("Archived")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Genre")
                        .HasColumnType("text");

                    b.Property<string>("Restriction")
                        .HasColumnType("text");

                    b.Property<bool>("StageActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("StageName")
                        .HasColumnType("text");

                    b.HasKey("StageID");

                    b.ToTable("Stage");
                });

            modelBuilder.Entity("FestivalApplication.Model.Track", b =>
                {
                    b.Property<int>("TrackID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("TrackName")
                        .HasColumnType("text");

                    b.Property<string>("TrackSource")
                        .HasColumnType("text");

                    b.HasKey("TrackID");

                    b.ToTable("Track");
                });

            modelBuilder.Entity("FestivalApplication.Model.TrackActivity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("MusicListID")
                        .HasColumnType("int");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("int");

                    b.Property<bool>("Playing")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("TrackID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("MusicListID");

                    b.ToTable("TrackActivity");
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

                    b.Property<int?>("StageID")
                        .HasColumnType("int");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("UserActivityID");

                    b.HasIndex("StageID");

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

            modelBuilder.Entity("FestivalApplication.Model.Interaction", b =>
                {
                    b.HasOne("FestivalApplication.Model.Message", "Message")
                        .WithMany()
                        .HasForeignKey("MessageID");

                    b.HasOne("FestivalApplication.Model.UserActivity", "UserActivity")
                        .WithMany("Interactions")
                        .HasForeignKey("UserActivityID");

                    b.Navigation("Message");

                    b.Navigation("UserActivity");
                });

            modelBuilder.Entity("FestivalApplication.Model.LoyaltyPoints", b =>
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

            modelBuilder.Entity("FestivalApplication.Model.MusicListActivity", b =>
                {
                    b.HasOne("FestivalApplication.Model.MusicList", null)
                        .WithMany("PlayList")
                        .HasForeignKey("MusicListID");
                });

            modelBuilder.Entity("FestivalApplication.Model.TrackActivity", b =>
                {
                    b.HasOne("FestivalApplication.Model.MusicList", null)
                        .WithMany("MusicTracks")
                        .HasForeignKey("MusicListID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FestivalApplication.Model.UserActivity", b =>
                {
                    b.HasOne("FestivalApplication.Model.Stage", "Stage")
                        .WithMany("Log")
                        .HasForeignKey("StageID");

                    b.HasOne("FestivalApplication.Model.User", "User")
                        .WithMany("Log")
                        .HasForeignKey("UserID");

                    b.Navigation("Stage");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FestivalApplication.Model.MusicList", b =>
                {
                    b.Navigation("MusicTracks");

                    b.Navigation("PlayList");
                });

            modelBuilder.Entity("FestivalApplication.Model.Stage", b =>
                {
                    b.Navigation("Log");
                });

            modelBuilder.Entity("FestivalApplication.Model.User", b =>
                {
                    b.Navigation("Log");
                });

            modelBuilder.Entity("FestivalApplication.Model.UserActivity", b =>
                {
                    b.Navigation("Interactions");

                    b.Navigation("MessageHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
