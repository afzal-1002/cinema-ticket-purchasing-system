using CinemaTicket.Config;
using CinemaTicket.Model;
using CinemaTicket.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinemaTicket.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpPost("initiate")]
    [Authorize]
    public async Task<ActionResult<object>> InitiatePayment(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == userId, cancellationToken);

        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found" });
        }

        var payment = new Payment
        {
            ReservationId = request.ReservationId,
            UserId = userId,
            Amount = reservation.TotalPrice,
            Method = request.PaymentMethod,
            Status = PaymentStatus.PENDING,
            TransactionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPaymentById), new { paymentId = payment.Id }, new
        {
            paymentId = payment.Id,
            transactionId = payment.TransactionId,
            amount = payment.Amount,
            status = payment.Status.ToString(),
            message = "Payment initiated successfully"
        });
    }

    [HttpPost("{paymentId}/process")]
    [Authorize]
    public async Task<ActionResult<object>> ProcessPayment(
        long paymentId,
        [FromBody] Dictionary<string, string> paymentDetails,
        CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        // Simulate payment processing
        payment.Status = PaymentStatus.COMPLETED;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.Reservation.Status = ReservationStatus.Confirmed;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            paymentId = payment.Id,
            status = payment.Status.ToString(),
            processedAt = payment.ProcessedAt,
            message = "Payment processed successfully"
        });
    }

    [HttpPost("{paymentId}/refund")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<object>> RefundPayment(
        long paymentId,
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        payment.Status = PaymentStatus.REFUNDED;
        payment.Reservation.Status = ReservationStatus.Cancelled;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            paymentId = payment.Id,
            status = payment.Status.ToString(),
            reason = request.Reason,
            message = "Payment refunded successfully"
        });
    }

    [HttpGet("my-payments")]
    [Authorize]
    public async Task<ActionResult<object>> GetMyPayments(CancellationToken cancellationToken)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var payments = await _context.Payments
            .Where(p => p.UserId == userId)
            .Include(p => p.Reservation)
            .ThenInclude(r => r.Screening)
            .ThenInclude(s => s.Movie)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                paymentId = p.Id,
                transactionId = p.TransactionId,
                amount = p.Amount,
                paymentMethod = p.Method.ToString(),
                status = p.Status.ToString(),
                createdAt = p.CreatedAt,
                processedAt = p.ProcessedAt,
                reservation = new
                {
                    id = p.Reservation.Id,
                    movieTitle = p.Reservation.Screening.Movie.Title,
                    startDateTime = p.Reservation.Screening.StartDateTime,
                    numberOfSeats = p.Reservation.NumberOfSeats
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(payments);
    }

    [HttpGet("{paymentId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetPaymentById(long paymentId, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        return Ok(new
        {
            paymentId = payment.Id,
            transactionId = payment.TransactionId,
            amount = payment.Amount,
            paymentMethod = payment.Method.ToString(),
            status = payment.Status.ToString(),
            createdAt = payment.CreatedAt,
            processedAt = payment.ProcessedAt
        });
    }

    [HttpGet("reservation/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetPaymentByReservation(long reservationId, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.ReservationId == reservationId, cancellationToken);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found for this reservation" });
        }

        return Ok(new
        {
            paymentId = payment.Id,
            transactionId = payment.TransactionId,
            amount = payment.Amount,
            status = payment.Status.ToString()
        });
    }
}

public record InitiatePaymentRequest(long ReservationId, PaymentMethod PaymentMethod);
public record RefundRequest(string Reason = "Refund requested");
