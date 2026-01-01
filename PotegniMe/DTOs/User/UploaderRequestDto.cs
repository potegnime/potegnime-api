namespace PotegniMe.DTOs.User;

public class UploaderRequestDto
{
    // Role requested by user
    public required string RequestedRole { get; set; }

    // "Imaš kaj izkušenj z nalaganje torrentov? (na kratko razloži)*"
    public required string Experience { get; set; }

    // "Kakšno vrsto vsebine nameravaš nalagati?*"
    public required string Content { get; set; }

    // "Povezave do prejšnjih nalaganj(če obstajajo):"
    public string? Proof { get; set; }

    // "Ali si že član_ica drugih zasebnih trackerjev? Če ja, katerih?"
    public string? OtherTrackers { get; set; }
}
