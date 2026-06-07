using System.ComponentModel.DataAnnotations;

namespace AzraTasks.Data;

public abstract class UserObject : TrackingBase
{
    // NB: we don't apply the required modifier and suppress the NRT warning because
    // the RequiredAttribute will ensure this is enforced by the database. The DbContext
    // uses the IUserIdProvider to ensure this is set when new records are added.
    [Required]
    public string CreatedById { get; set; } = null!;
    public ApplicationUser? CreatedBy { get; set; }
}
