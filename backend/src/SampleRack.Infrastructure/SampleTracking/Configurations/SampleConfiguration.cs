using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleRack.Infrastructure.SampleTracking.Configurations;

public sealed class SampleConfiguration : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        builder.ToTable("samples", "sample_tracking");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SampleCode).HasColumnName("sample_code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.SampleName).HasColumnName("sample_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ItemCode).HasColumnName("item_code").HasMaxLength(100);
        builder.Property(e => e.ProductCategory).HasColumnName("product_category").HasMaxLength(100);
        builder.Property(e => e.Model).HasColumnName("model").HasMaxLength(100);
        builder.Property(e => e.Size).HasColumnName("size").HasMaxLength(100);
        builder.Property(e => e.Variant).HasColumnName("variant").HasMaxLength(100);
        builder.Property(e => e.MaterialType).HasColumnName("material_type").HasMaxLength(100);

        builder.Property(e => e.SupplierName).HasColumnName("supplier_name").HasMaxLength(200);
        builder.Property(e => e.SupplierCode).HasColumnName("supplier_code").HasMaxLength(100);
        builder.Property(e => e.PurchaseOrderNumber).HasColumnName("purchase_order_number").HasMaxLength(100);
        builder.Property(e => e.GrnNumber).HasColumnName("grn_number").HasMaxLength(100);
        builder.Property(e => e.SupplierReference).HasColumnName("supplier_reference").HasMaxLength(200);

        builder.Property(e => e.ReceivedDate).HasColumnName("received_date").IsRequired();
        builder.Property(e => e.ReceivedTime).HasColumnName("received_time");
        builder.Property(e => e.ReceivedQuantity).HasColumnName("received_quantity").HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.ReceivedBy).HasColumnName("received_by").HasMaxLength(150);
        builder.Property(e => e.Department).HasColumnName("department").HasMaxLength(150);
        builder.Property(e => e.SampleOwner).HasColumnName("sample_owner").HasMaxLength(150);

        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(e => e.Purpose).HasColumnName("purpose").HasMaxLength(1000);
        builder.Property(e => e.TestingRequirement).HasColumnName("testing_requirement").HasMaxLength(1000);
        builder.Property(e => e.RequiredTestingType).HasColumnName("required_testing_type").HasMaxLength(150);
        builder.Property(e => e.StorageRemarks).HasColumnName("storage_remarks").HasMaxLength(1000);

        builder.Property(e => e.RackLocationId).HasColumnName("rack_location_id");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.HasMany(e => e.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Movements)
            .WithOne()
            .HasForeignKey(m => m.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.TestingRecords)
            .WithOne()
            .HasForeignKey(t => t.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey(a => a.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Disposal)
            .WithOne()
            .HasForeignKey<DisposalRecord>(d => d.SampleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.SampleCode }).IsUnique().HasDatabaseName("ix_samples_tenant_code");
        builder.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("ix_samples_tenant_status");
        builder.HasIndex(e => new { e.TenantId, e.RackLocationId }).HasDatabaseName("ix_samples_tenant_location");
        builder.HasIndex(e => new { e.TenantId, e.ReceivedDate }).HasDatabaseName("ix_samples_tenant_received");
    }
}
