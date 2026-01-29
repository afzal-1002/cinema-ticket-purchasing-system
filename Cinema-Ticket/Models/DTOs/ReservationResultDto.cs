using System;
using System.Collections.Generic;



namespace CinemaTicket.Models.DTOs
{
    public class ReservationResultDto
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;  // ENSURE this property exists

        public static ReservationResultDto Ok()
        {
            return new ReservationResultDto { Success = true, Error = string.Empty };
        }

        public static ReservationResultDto Fail(string errorMessage)
        {
            return new ReservationResultDto { Success = false, Error = errorMessage };
        }
    }
}

