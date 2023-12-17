using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TheGrammar.Data;

public class Request : AuditableEntity
{
    public int Id { get; set; }
    public required string RequestText { get; set; }
    public required string ResponseText { get; set; }
}

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(p => p.RequestText)
            .HasMaxLength(5000);

        builder.Property(p => p.ResponseText)
            .HasMaxLength(5000);
    }
}