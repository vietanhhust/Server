using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ServerAPI.Model.Database
{
    public partial class ClientManagerContext : DbContext
    {

        public ClientManagerContext(DbContextOptions<ClientManagerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryItem> CategoryItems { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<FrontendRole> FrontendRoles { get; set; }
        public virtual DbSet<GroupClient> GroupClients { get; set; }
        public virtual DbSet<GroupRole> GroupRoles { get; set; }
        public virtual DbSet<HistoryChangeBalance> HistoryChangeBalances { get; set; }
        public virtual DbSet<ManagingAccount> ManagingAccounts { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleActive> RoleActives { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=ClientManager;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.AccountName).IsRequired();

                entity.Property(e => e.Address).HasMaxLength(50);

                entity.Property(e => e.Balance).HasDefaultValueSql("((0))");

                entity.Property(e => e.Credit).HasDefaultValueSql("((0))");

                entity.Property(e => e.Debit).HasDefaultValueSql("((0))");

                entity.Property(e => e.Description).HasDefaultValueSql("((0))");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.IdentityNumber).HasMaxLength(15);

                entity.Property(e => e.IsActived).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsLogged).HasDefaultValueSql("((0))");

                entity.Property(e => e.Password).IsRequired();

                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryName).IsRequired();
            });

            modelBuilder.Entity<CategoryItem>(entity =>
            {
                entity.ToTable("CategoryItem");

                entity.Property(e => e.CategoryId).HasComment("Trường này tham chiếu bảng Category");

                entity.Property(e => e.CategoryItemName).IsRequired();

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryItems)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryItem_Category");
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("Client");

                entity.Property(e => e.IsNew).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.ClientGroup)
                    .WithMany(p => p.Clients)
                    .HasForeignKey(d => d.ClientGroupId)
                    .HasConstraintName("FK_Client_GroupClient1");
            });

            modelBuilder.Entity<FrontendRole>(entity =>
            {
                entity.ToTable("FrontendRole");
            });

            modelBuilder.Entity<GroupClient>(entity =>
            {
                entity.ToTable("GroupClient");

                entity.Property(e => e.GroupName).IsRequired();
            });

            modelBuilder.Entity<GroupRole>(entity =>
            {
                entity.ToTable("GroupRole");

                entity.Property(e => e.GroupName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<HistoryChangeBalance>(entity =>
            {
                entity.ToTable("HistoryChangeBalance");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.HistoryChangeBalances)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HistoryChangeBalance_Account");

                entity.HasOne(d => d.ManagingAccount)
                    .WithMany(p => p.HistoryChangeBalances)
                    .HasForeignKey(d => d.ManagingAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HistoryChangeBalance_ManagingAccount");
            });

            modelBuilder.Entity<ManagingAccount>(entity =>
            {
                entity.ToTable("ManagingAccount");

                entity.Property(e => e.GroupRoleId).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.Password).IsRequired();

                entity.HasOne(d => d.GroupRole)
                    .WithMany(p => p.ManagingAccounts)
                    .HasForeignKey(d => d.GroupRoleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ManagingAccount_GroupRole");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_Account");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_ManagingAccount");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.HasOne(d => d.CategoryItem)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.CategoryItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_CategoryItem");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.FrontendCode).HasDefaultValueSql("((0))");

                entity.Property(e => e.Method)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Template).IsRequired();
            });

            modelBuilder.Entity<RoleActive>(entity =>
            {
                entity.ToTable("RoleActive");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsCreate).HasColumnName("isCreate");

                entity.Property(e => e.IsDelete).HasColumnName("isDelete");

                entity.Property(e => e.IsPut).HasColumnName("isPut");

                entity.Property(e => e.IsView).HasColumnName("isView");

                entity.HasOne(d => d.FrontendRole)
                    .WithMany(p => p.RoleActives)
                    .HasForeignKey(d => d.FrontendRoleId)
                    .HasConstraintName("FK_RoleActive_FrontendRole");

                entity.HasOne(d => d.GroupRole)
                    .WithMany(p => p.RoleActives)
                    .HasForeignKey(d => d.GroupRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoleActive_GroupRole");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
