using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Dto.User;

public class UpdateUserRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
