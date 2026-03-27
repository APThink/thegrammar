using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheGrammar.Domain;

namespace TheGrammar.Database.Configuration;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.HasIndex(m => m.Key)
            .IsUnique();

        builder.Property(m => m.Key)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.ModelName)
            .HasMaxLength(100)
            .IsRequired();
    }
}