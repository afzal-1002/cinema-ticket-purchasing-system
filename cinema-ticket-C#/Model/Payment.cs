using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Model;

public enum PaymentMethod
{
    CREDIT_CARD,
    DEBIT_CARD,
    PAYPAL,
    STRIPE
}

public enum PaymentStatus
{
    PENDING,
    COMPLETED,
    FAILED,
    REFUNDED
}

public class Payment
{
    public long Id { get; set; }

    [Required]
    public long ReservationId { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }

    [Required]
    public PaymentStatus Status { get; set; }

    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    public Reservation Reservation { get; set; } = null!;
    public User User { get; set; } = null!;
}
