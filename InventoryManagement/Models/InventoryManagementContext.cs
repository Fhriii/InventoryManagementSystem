using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class InventoryManagementContext : DbContext
{
    public InventoryManagementContext()
    {
    }

    public InventoryManagementContext(DbContextOptions<InventoryManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BillOfMaterial> BillOfMaterials { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryIn> InventoryIns { get; set; }

    public virtual DbSet<InventoryOut> InventoryOuts { get; set; }

    public virtual DbSet<ItemMaster> ItemMasters { get; set; }

    public virtual DbSet<Production> Productions { get; set; }

    public virtual DbSet<ProductionMaterialUsage> ProductionMaterialUsages { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<RequestOrder> RequestOrders { get; set; }

    public virtual DbSet<RequestOrderDetail> RequestOrderDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=InventoryManagement;User=sa;Password=Fahri@2024!;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillOfMaterial>(entity =>
        {
            entity.HasKey(e => e.Bomid).HasName("PK__BillOfMa__CA12FCBBF6F3436F");

            entity.Property(e => e.Bomid).HasColumnName("BOMID");
            entity.Property(e => e.FinishedGoodId).HasColumnName("FinishedGoodID");
            entity.Property(e => e.QuantityRequired).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.RawMaterialId).HasColumnName("RawMaterialID");
            entity.Property(e => e.Unit).HasMaxLength(20);

            entity.HasOne(d => d.FinishedGood).WithMany(p => p.BillOfMaterialFinishedGoods)
                .HasForeignKey(d => d.FinishedGoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BillOfMat__Finis__6D0D32F4");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.BillOfMaterialRawMaterials)
                .HasForeignKey(d => d.RawMaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BillOfMat__RawMa__6E01572D");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryId).HasName("PK__Deliveri__626D8FEE38785290");

            entity.HasIndex(e => e.DeliveryNumber, "UQ__Deliveri__CB28B4375A11FFBA").IsUnique();

            entity.Property(e => e.DeliveryId).HasColumnName("DeliveryID");
            entity.Property(e => e.DeliveryNumber).HasMaxLength(50);
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Request).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deliverie__Reque__73BA3083");

            entity.HasOne(d => d.User).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deliverie__UserI__74AE54BC");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventory");

            entity.Property(e => e.BatchNumber)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.DateIn).HasColumnType("datetime");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasOne(d => d.Item).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_Inventory_ItemMaster");
        });

        modelBuilder.Entity<InventoryIn>(entity =>
        {
            entity.HasKey(e => e.InventoryInId).HasName("PK__Inventor__BDF1FDD010263427");

            entity.ToTable("InventoryIn");

            entity.Property(e => e.InventoryInId).HasColumnName("InventoryInID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ReferenceNo).HasMaxLength(50);
            entity.Property(e => e.SourceType).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryIns)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_InventoryIn_ItemMaster");

            entity.HasOne(d => d.User).WithMany(p => p.InventoryIns)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__UserI__5441852A");
        });

        modelBuilder.Entity<InventoryOut>(entity =>
        {
            entity.HasKey(e => e.InventoryOutId).HasName("PK__Inventor__87D08304BD80DAD8");

            entity.ToTable("InventoryOut");

            entity.Property(e => e.InventoryOutId).HasColumnName("InventoryOutID");
            entity.Property(e => e.DestinationType).HasMaxLength(20);
            entity.Property(e => e.ReferenceNo).HasMaxLength(50);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryOuts)
                .HasForeignKey(d => d.InventoryId)
                .HasConstraintName("FK_InventoryOut_Inventory");

            entity.HasOne(d => d.User).WithMany(p => p.InventoryOuts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__UserI__59063A47");
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__ItemMast__727E83EBEA8DFA7D");

            entity.ToTable("ItemMaster");

            entity.HasIndex(e => e.ItemCode, "UQ__ItemMast__3ECC0FEA765681EE").IsUnique();

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.ItemName).HasMaxLength(100);
            entity.Property(e => e.ItemType).HasMaxLength(20);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<Production>(entity =>
        {
            entity.HasKey(e => e.ProductionId).HasName("PK__Producti__D5D9A2F542F4CCA2");

            entity.ToTable("Production");

            entity.Property(e => e.ProductionId).HasColumnName("ProductionID");
            entity.Property(e => e.ProductItemId).HasColumnName("ProductItemID");
            entity.Property(e => e.ReferenceNo).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.Productions)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productio__Produ__656C112C");

            entity.HasOne(d => d.User).WithMany(p => p.Productions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productio__UserI__66603565");
        });

        modelBuilder.Entity<ProductionMaterialUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__Producti__29B197C0FCE82623");

            entity.ToTable("ProductionMaterialUsage");

            entity.Property(e => e.UsageId).HasColumnName("UsageID");
            entity.Property(e => e.ProductionId).HasColumnName("ProductionID");
            entity.Property(e => e.RawMaterialItemId).HasColumnName("RawMaterialItemID");

            entity.HasOne(d => d.Production).WithMany(p => p.ProductionMaterialUsages)
                .HasForeignKey(d => d.ProductionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productio__Produ__693CA210");

            entity.HasOne(d => d.RawMaterialItem).WithMany(p => p.ProductionMaterialUsages)
                .HasForeignKey(d => d.RawMaterialItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Productio__RawMa__6A30C649");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId).HasName("PK__Purchase__036BAC44B8C7C702");

            entity.HasIndex(e => e.Ponumber, "UQ__Purchase__69B9A841FAF10F5D").IsUnique();

            entity.Property(e => e.PurchaseOrderId).HasColumnName("PurchaseOrderID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .HasColumnName("PONumber");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__Suppl__4AB81AF0");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__UserI__4BAC3F29");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.PodetailId).HasName("PK__Purchase__4EB47B5E4AF4291D");

            entity.Property(e => e.PodetailId).HasColumnName("PODetailID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("PurchaseOrderID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__ItemI__4F7CD00D");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PurchaseO__Purch__4E88ABD4");
        });

        modelBuilder.Entity<RequestOrder>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__RequestO__33A8519A40804E10");

            entity.HasIndex(e => e.RequestNumber, "UQ__RequestO__9ADA6BE0268D8A00").IsUnique();

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.RequestNumber).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.RequestOrders)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_RequestOrders_ItemMaster");

            entity.HasOne(d => d.User).WithMany(p => p.RequestOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestOr__UserI__5EBF139D");
        });

        modelBuilder.Entity<RequestOrderDetail>(entity =>
        {
            entity.HasKey(e => e.RequestDetailId).HasName("PK__RequestO__DC528B7040F4B0DB");

            entity.Property(e => e.RequestDetailId).HasColumnName("RequestDetailID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.RequestOrderDetails)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestOr__ItemI__628FA481");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestOrderDetails)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestOr__Reque__619B8048");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666945C2C881C");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasColumnType("ntext");
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC6F8F1B48");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E40EC61B1E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
