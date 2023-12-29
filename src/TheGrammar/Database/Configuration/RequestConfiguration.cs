using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Domain;

namespace TheGrammar.Database.Configuration;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(p => p.RequestText)
            .HasMaxLength(5000);

        builder.Property(p => p.ResponseText)
            .HasMaxLength(5000);

        builder.Property(p => p.ChatVersion)
                 .HasConversion(
                     v => v.HasValue ? v.ToString() : null,
                     v => v == null ? null : (ChatVersion?)Enum.Parse(typeof(ChatVersion), v))
                 .HasMaxLength(50);
    }
}