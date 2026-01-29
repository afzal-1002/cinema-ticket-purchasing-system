using CinemaTicket.Dto.User;
using CinemaTicket.Model;

namespace CinemaTicket.Mapper;

public static class UserMapper
{
    public static UserSummaryDto ToSummary(this User user)
    {
        return new UserSummaryDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsAdmin,
            user.RowVersion
        );
    }
}
