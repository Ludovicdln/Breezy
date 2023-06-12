using Breezy.Tests.Models.Enums;

namespace Breezy.Tests.Models.Constraints;

public class UserCopy
{
    public int Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime Birthday { get; set; }
    public Gender Gender { get; set; }
    public bool IsMinor { get; set; }
    public List<CarUser> Cars { get; set; } = new ();
    public List<HouseUser> Houses { get; set; } = new ();
    
    public bool IsValid()
    {
        return Id != 0 && !string.IsNullOrEmpty(Firstname) && !string.IsNullOrEmpty(Lastname) && Cars != null && Houses != null;
    }
}