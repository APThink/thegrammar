using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TheGrammar.Data;

public class Prompt : AuditableEntity
{
    public int Id { get; set; }
    public Keys LeftKey { get; set; } = Keys.Control | Keys.Shift;
    public Keys RightKey { get; set; }
    public required string Promt { get; set; }

    public override string ToString()
    {
        return $"{LeftKey} + {RightKey}";
    }
}

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