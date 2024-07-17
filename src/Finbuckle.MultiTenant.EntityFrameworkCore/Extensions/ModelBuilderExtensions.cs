// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Linq;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

public static class FinbuckleModelBuilderExtensions
{
    /// <summary>
    /// Configures any entity's with the [MultiTenant] attribute.
    /// </summary>
    public static ModelBuilder ConfigureMultiTenant(this ModelBuilder modelBuilder)
    {
        // Call IsMultiTenant() to configure the types marked with the MultiTenant Data Attribute
        foreach (var clrType in modelBuilder.Model.GetEntityTypes()
                     .Where(et => et.ClrType.HasMultiTenantAttribute())
                     .Select(et => et.ClrType))
        {
            modelBuilder.Entity(clrType)
                .IsMultiTenant();
        }

        return modelBuilder;
    }

    public static ModelBuilder ConfigureTenantInfoEntity<TEntity>(this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<TEntity>>? buildAction = null)
        where TEntity : class, ITenantInfo
    {
        return modelBuilder.Entity<TEntity>(entity =>
        {
            entity.HasKey(ti => ti.Id);
            entity.Property(ti => ti.Id).HasMaxLength(Constants.TenantIdMaxLength);
            entity.HasIndex(ti => ti.Key).IsUnique();
            buildAction?.Invoke(entity);
        });
    }
}