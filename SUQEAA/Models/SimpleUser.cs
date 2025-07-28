using System.ComponentModel.DataAnnotations;

public class SimpleUser
{
    public int Id { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Role { get; set; }

     public bool IsAdmin => Role == "Admin";
    public bool IsDistributor => Role == "Distributor";
    public bool IsCustomer => Role == "Customer";
}
