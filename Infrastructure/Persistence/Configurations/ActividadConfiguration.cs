using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ActividadConfiguration : IEntityTypeConfiguration<Actividad>
{
    public void Configure(EntityTypeBuilder<Actividad> builder)
    {
        builder.ToTable("Actividades");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TipoAccion).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Detalles).IsRequired().HasMaxLength(500);
    }
}