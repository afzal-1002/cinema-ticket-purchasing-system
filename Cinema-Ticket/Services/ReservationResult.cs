namespace CinemaTicket.Services
{
    public class ReservationResult
    {
        public bool Success { get; }
        public string Error { get; }

        private ReservationResult(bool success, string error)
        {
            Success = success;
            Error = error ?? string.Empty;
        }

        public static ReservationResult Ok() => new ReservationResult(true, string.Empty);
        public static ReservationResult Fail(string error) => new ReservationResult(false, error);
    }
}