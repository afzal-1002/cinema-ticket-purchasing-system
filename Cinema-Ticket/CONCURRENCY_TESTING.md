# 🎯 Reservation Concurrency Testing Guide

## 📍 Where is the Double-Booking Prevention Code?

### 1️⃣ **Database Level (Primary Defense)**
**File:** `Data/ApplicationDbContext.cs` (Lines 42-44)

```csharp
// ✅ Unique constraint: One seat per screening
modelBuilder.Entity<Reservation>()
    .HasIndex(r => new { r.ScreeningId, r.SeatNumber })
    .IsUnique();
```

**What it does:**
- Creates a **UNIQUE INDEX** in the database on `(ScreeningId, SeatNumber)` combination
- Database will **REJECT** any attempt to insert duplicate seat for same screening
- This is the **strongest protection** - even if application code fails, database prevents duplicates

---

### 2️⃣ **Application Level (First Line of Defense)**
**File:** `Services/Implementation/ReservationService.cs` (Lines 44-63)

```csharp
public async Task<bool> CreateReservationAsync(Reservation reservation)
{
    try
    {
        // ✅ Check if seat is already reserved (first line of defense)
        if (await IsSeatReservedAsync(reservation.ScreeningId, reservation.SeatNumber))
        {
            return false;
        }

        // ✅ Try to save the reservation
        // The database will enforce the unique constraint on (ScreeningId, SeatNumber)
        // If another user books the same seat at the same time, database will throw exception
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return true;
    }
    catch (DbUpdateException)
    {
        // ✅ Database threw exception - unique constraint violation
        // This happens when two users try to book the same seat at the same time
        return false;
    }
}
```

**Helper Method:** `IsSeatReservedAsync` (Line 300-304)
```csharp
public async Task<bool> IsSeatReservedAsync(int screeningId, int seatNumber)
{
    return await _context.Reservations
        .AnyAsync(r => r.ScreeningId == screeningId && r.SeatNumber == seatNumber);
}
```

---

### 3️⃣ **Controller Level (User Feedback)**
**File:** `Controllers/ReservationsController.cs` (Lines 242-287)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Reserve(int screeningId, string selectedSeat)
{
    try
    {
        var reservation = new Reservation
        {
            UserId = userId,
            ScreeningId = screeningId,
            SeatNumber = seatNumber,
            ReservationDate = DateTime.Now,
            IsPaid = false
        };

        var success = await _reservationService.CreateReservationAsync(reservation);

        if (success)
        {
            TempData["SuccessMessage"] = $"✅ Seat {seatNumber} reserved successfully!";
            return RedirectToAction("MyBookings", "Profile");
        }
        else
        {
            TempData["ErrorMessage"] = "❌ Failed to reserve seat. It may already be taken.";
            return RedirectToAction("SeatMap", new { id = screeningId });
        }
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = $"❌ Booking failed: {ex.Message}";
        return RedirectToAction("SeatMap", new { id = screeningId });
    }
}
```

---

## 🧪 How to Test Double-Booking Prevention

### **Test Scenario 1: Manual Testing with Browser**

1. **Start the application:**
   ```bash
   dotnet run
   ```

2. **Open TWO browser windows** (side by side):
   - Window 1: `http://localhost:5147`
   - Window 2: `http://localhost:5147` (use Incognito/Private mode)

3. **In both windows:**
   - Login (or just browse as guest since we use userId=1)
   - Go to Movie Screenings
   - Click "Book Ticket" on the SAME screening
   - Select the SAME seat number (e.g., Seat 5)

4. **Click Reserve in Window 1** → Should show "✅ Seat 5 reserved successfully!"

5. **Click Reserve in Window 2** → Should show "❌ Failed to reserve seat. It may already be taken."

---

### **Test Scenario 2: Verify Database Constraint**

1. **Check the database directly:**
   ```bash
   # Connect to MySQL
   mysql -u cinemauser -p
   # Password: CinemaPass123!
   
   USE CinemaTicketDB;
   
   # View the unique index
   SHOW INDEX FROM Reservations WHERE Key_name LIKE '%ScreeningId%';
   
   # Try to insert duplicate manually (should fail)
   INSERT INTO Reservations (UserId, ScreeningId, SeatNumber, ReservationDate, IsPaid, RowVersion)
   VALUES (1, 1, 5, NOW(), 0, 0x00000000000007D1);
   
   # Try again with same seat - DATABASE SHOULD REJECT IT
   INSERT INTO Reservations (UserId, ScreeningId, SeatNumber, ReservationDate, IsPaid, RowVersion)
   VALUES (2, 1, 5, NOW(), 0, 0x00000000000007D2);
   ```
   
   **Expected Result:**
   ```
   ERROR 1062 (23000): Duplicate entry '1-5' for key 'IX_Reservations_ScreeningId_SeatNumber'
   ```

---

### **Test Scenario 3: Concurrent Booking Simulation**

Open two terminals and run this test:

**Terminal 1:**
```bash
curl -X POST http://localhost:5147/Reservations/Reserve \
  -d "screeningId=1&selectedSeat=10" \
  -H "Content-Type: application/x-www-form-urlencoded"
```

**Terminal 2 (run at SAME TIME):**
```bash
curl -X POST http://localhost:5147/Reservations/Reserve \
  -d "screeningId=1&selectedSeat=10" \
  -H "Content-Type: application/x-www-form-urlencoded"
```

**Expected Result:**
- One request: Success (200 OK)
- Other request: Redirected with error message

---

### **Test Scenario 4: Check Database Records**

After testing, verify in database:

```sql
USE CinemaTicketDB;

-- Check reservations for screening 1
SELECT Id, UserId, ScreeningId, SeatNumber, ReservationDate 
FROM Reservations 
WHERE ScreeningId = 1 
ORDER BY SeatNumber;

-- Should show ONLY ONE reservation per seat number
-- Example output:
-- | Id | UserId | ScreeningId | SeatNumber | ReservationDate      |
-- |----|--------|-------------|------------|----------------------|
-- | 1  | 1      | 1           | 5          | 2025-12-04 10:30:00 |
-- | 2  | 1      | 1           | 10         | 2025-12-04 10:35:00 |
```

---

## 🎯 What Happens When Two Users Click at Same Time?

### **Timeline:**

```
TIME    USER A                          USER B
----    ------                          ------
T1      Click "Reserve Seat 5"          Click "Reserve Seat 5"
T2      IsSeatReservedAsync() → false   IsSeatReservedAsync() → false
T3      Add to context                  Add to context
T4      SaveChangesAsync() → SUCCESS ✅ SaveChangesAsync() → DATABASE REJECTS ❌
T5      "Seat 5 reserved!"              Catch DbUpdateException
T6                                       Return false
T7                                       "Failed to reserve seat"
```

**Key Point:** Even though both passed the `IsSeatReservedAsync()` check, the **database unique constraint** catches the conflict and prevents the duplicate.

---

## 🔍 Debug Logging

To see what's happening, check the console output:

```csharp
// In ReservationService.CreateReservationAsync
catch (DbUpdateException ex)
{
    Console.WriteLine($"DATABASE REJECTED: Seat {reservation.SeatNumber} already reserved!");
    Console.WriteLine($"Exception: {ex.Message}");
    return false;
}
```

---

## ✅ Summary

**Three Layers of Protection:**

1. **UI Layer** - Seat map shows reserved seats as disabled
2. **Application Layer** - `IsSeatReservedAsync()` checks before saving
3. **Database Layer** - Unique constraint `(ScreeningId, SeatNumber)` enforces uniqueness

**Why all three?**
- UI: Better user experience (don't even show reserved seats)
- Application: Faster rejection (no need to hit database)
- Database: **Bulletproof guarantee** - prevents race conditions

The database constraint is the **strongest** because it's atomic and handles concurrent requests perfectly! 🎯
