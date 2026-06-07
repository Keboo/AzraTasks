using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzraTasks.Data;

public abstract class TrackingBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastModifiedDate { get; set; }
}
