using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuanLyPhongTro.Models;
using System.IO;

namespace QuanLyPhongTro.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<BillDetail> BillDetails { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Notice> Notices { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PersonDetail> PersonDetails { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomImage> RoomImages { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<BookingRequest> BookingRequests { get; set; }
        public virtual DbSet<FavoriteRoom> FavoriteRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Bill");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdPersonNavigation)
                      .WithMany(p => p.Bills)
                      .HasForeignKey(d => d.IdPerson)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Bill_Person");

                entity.HasOne(d => d.IdRoomNavigation)
                      .WithMany(p => p.Bills)
                      .HasForeignKey(d => d.IdRoom)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Bill_Room");
            });

            modelBuilder.Entity<BillDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_BillDetail");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdBillNavigation)
                      .WithMany(p => p.BillDetails)
                      .HasForeignKey(d => d.IdBill)
                      .HasConstraintName("FK_BillDetail_Bill");

                entity.HasOne(d => d.IdServiceNavigation)
                      .WithMany(p => p.BillDetails)
                      .HasForeignKey(d => d.IdService)
                      .HasConstraintName("FK_BillDetail_Service");
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Contract");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdRenterNavigation)
                      .WithMany(p => p.Contracts)
                      .HasForeignKey(d => d.IdRenter)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Contract_Renter");

                entity.HasOne(d => d.IdRoomNavigation)
                      .WithMany(p => p.Contracts)
                      .HasForeignKey(d => d.IdRoom)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Contract_Room");
            });

            modelBuilder.Entity<Notice>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Notice");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdreportNavigation)
                      .WithOne(r => r.Notice)
                      .HasForeignKey<Notice>(d => d.Idreport)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Notice_Report");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.IdBill).HasName("PK_Payment");
                entity.Property(e => e.IdBill).ValueGeneratedNever();

                entity.HasOne(d => d.IdPaymentNavigation)
                      .WithOne(p => p.Payment)
                      .HasForeignKey<Payment>(d => d.IdBill)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Payment_Bill");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Person");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdDetailNavigation)
                      .WithMany(p => p.People)
                      .HasForeignKey(d => d.IdDetail)
                      .HasConstraintName("FK_Person_PersonDetail");
            });

            modelBuilder.Entity<PersonDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PersonDetail");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Report");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Status).HasDefaultValue("Pending");

                entity.HasOne(d => d.IdReporterNavigation)
                      .WithMany(p => p.Reports)
                      .HasForeignKey(d => d.IdReporter)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Report_Reporter");

                entity.HasOne(d => d.IdRoomNavigation)
                      .WithMany(p => p.Reports)
                      .HasForeignKey(d => d.IdRoom)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Report_Room");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Room");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdOwnerNavigation)
                      .WithMany(p => p.Rooms)
                      .HasForeignKey(d => d.IdOwner)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Room_Owner");
            });

            modelBuilder.Entity<RoomImage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_RoomImage");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.IdRoomNavigation)
                      .WithMany(p => p.RoomImages)
                      .HasForeignKey(d => d.IdRoom)
                      .HasConstraintName("FK_RoomImage_Room");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Service");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<FavoriteRoom>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_FavoriteRoom");
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasIndex(e => new { e.PersonId, e.RoomId }).IsUnique();

                entity.HasOne(e => e.Person)
                      .WithMany()
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Room)
                      .WithMany()
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}