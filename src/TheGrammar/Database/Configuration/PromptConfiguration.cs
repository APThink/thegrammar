using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Domain;

namespace TheGrammar.Database.Configuration;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        var keysConverter = new ValueConverter<Keys, int>(v => (int)v, v => (Keys)v);

        builder.Property(p => p.LeftKey).HasConversion(keysConverter);
        builder.Property(p => p.RightKey).HasConversion(keysConverter);

        builder.Property(p => p.Promt).HasMaxLength(5000);
        builder.HasIndex(p => new { p.LeftKey, p.RightKey }).IsUnique();
    }
}
