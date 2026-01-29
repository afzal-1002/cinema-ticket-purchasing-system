namespace CinemaTicket.Dto.User;

public record UserSummaryDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    bool IsAdmin,
    byte[] RowVersion
);
