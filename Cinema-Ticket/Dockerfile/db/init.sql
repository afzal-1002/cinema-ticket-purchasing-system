-- Drop existing tables (fresh start)
DROP TABLE IF EXISTS Reservations;
DROP TABLE IF EXISTS Screenings;
DROP TABLE IF EXISTS Cinemas;
DROP TABLE IF EXISTS Users;

-- Users Table
-- ✅ ADDED: CreatedAt column to match C# entity
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    IsAdmin BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),  -- ✅ ADDED
    RowVersion TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_username (Username),
    INDEX idx_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Cinemas Table
CREATE TABLE Cinemas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    RoomNumber VARCHAR(50) NOT NULL,
    TotalRows INT NOT NULL,
    SeatsPerRow INT NOT NULL,
    RowVersion TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Screenings Table
CREATE TABLE Screenings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CinemaId INT NOT NULL,
    MovieTitle VARCHAR(200) NOT NULL,
    StartTime DATETIME(6) NOT NULL,
    EndTime DATETIME(6) NOT NULL,
    RowVersion TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (CinemaId) REFERENCES Cinemas(Id) ON DELETE CASCADE,
    INDEX idx_cinema_id (CinemaId),
    INDEX idx_start_time (StartTime)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Reservations Table
-- ✅ FIXED: SeatNumber changed from VARCHAR to INT (matches C# entity)
-- ✅ ADDED: IsPaid column (matches C# entity)
CREATE TABLE Reservations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    ScreeningId INT NOT NULL,
    SeatNumber INT NOT NULL,  -- ✅ CHANGED: VARCHAR(10) -> INT
    ReservationDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    IsPaid BOOLEAN NOT NULL DEFAULT FALSE,  -- ✅ ADDED
    RowVersion TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ScreeningId) REFERENCES Screenings(Id) ON DELETE CASCADE,
    UNIQUE KEY UQ_Reservation_Screening_Seat (ScreeningId, SeatNumber),
    INDEX idx_user_id (UserId),
    INDEX idx_screening_id (ScreeningId),
    INDEX idx_reservation_date (ReservationDate),
    INDEX idx_is_paid (IsPaid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =====================================================
-- SEED DATA - USERS
-- =====================================================

-- ✅ ADMIN USERS (5 admins - passwords stored as plain text per your requirements)
INSERT INTO Users (Username, Password, Email, FirstName, LastName, PhoneNumber, IsAdmin, CreatedAt)
VALUES 
    ('admin', 'admin', 'admin@cinema.com', 'Super', 'Admin', '1234567890', TRUE, '2025-01-01 08:00:00'),
    ('admin1', 'admin1', 'admin1@cinema.com', 'Cinema', 'Manager', '1234567891', TRUE, '2025-01-01 08:00:00'),
    ('admin2', 'admin2', 'admin2@cinema.com', 'Floor', 'Supervisor', '1234567892', TRUE, '2025-01-01 08:00:00'),
    ('admin3', 'admin3', 'admin3@cinema.com', 'Cinema', 'Owner', '1234567893', TRUE, '2025-01-01 08:00:00'),
    ('admin4', 'admin4', 'admin4@cinema.com', 'Cinema', 'Director', '1234567894', TRUE, '2025-01-01 08:00:00');

-- ✅ REGULAR USERS (4 users)
INSERT INTO Users (Username, Password, Email, FirstName, LastName, PhoneNumber, IsAdmin, CreatedAt)
VALUES 
    ('john', 'user123', 'john.doe@email.com', 'John', 'Doe', '5551234567', FALSE, '2025-10-15 10:30:00'),
    ('jane', 'user123', 'jane.smith@email.com', 'Jane', 'Smith', '5551234568', FALSE, '2025-10-20 14:15:00'),
    ('alice', 'user123', 'alice.wonder@email.com', 'Alice', 'Wonder', '5551234569', FALSE, '2025-11-01 09:00:00'),
    ('bob', 'user123', 'bob.builder@email.com', 'Bob', 'Builder', '5551234570', FALSE, '2025-11-05 16:45:00');

-- =====================================================
-- SEED DATA - CINEMAS
-- =====================================================

INSERT INTO Cinemas (Name, RoomNumber, TotalRows, SeatsPerRow)
VALUES 
    ('IMAX Grand Theater', 'IMAX-1', 15, 20),
    ('Premium Lounge', 'VIP-A', 8, 12),
    ('Classic Cinema', 'Hall-3', 12, 16),
    ('Family Theater', 'Hall-4', 10, 14),
    ('Student Special', 'Hall-5', 10, 12);

-- =====================================================
-- SEED DATA - SCREENINGS
-- =====================================================

INSERT INTO Screenings (CinemaId, MovieTitle, StartTime, EndTime)
VALUES 
    -- IMAX Grand Theater (CinemaId = 1)
    (1, 'The Matrix Resurrections', '2025-12-05 14:00:00', '2025-12-05 16:30:00'),
    (1, 'Inception', '2025-12-05 18:00:00', '2025-12-05 20:30:00'),
    (1, 'Interstellar', '2025-12-06 15:00:00', '2025-12-06 18:00:00'),
    (1, 'Dune: Part Two', '2025-12-06 20:00:00', '2025-12-06 23:00:00'),
    
    -- Premium Lounge (CinemaId = 2)
    (2, 'Oppenheimer', '2025-12-05 17:00:00', '2025-12-05 20:00:00'),
    (2, 'The Dark Knight', '2025-12-05 21:00:00', '2025-12-05 23:30:00'),
    (2, 'Barbie', '2025-12-06 14:00:00', '2025-12-06 16:00:00'),
    
    -- Classic Cinema (CinemaId = 3)
    (3, 'Avatar: The Way of Water', '2025-12-05 13:00:00', '2025-12-05 16:30:00'),
    (3, 'Top Gun: Maverick', '2025-12-05 19:00:00', '2025-12-05 21:30:00'),
    (3, 'Spider-Man: No Way Home', '2025-12-06 16:00:00', '2025-12-06 18:30:00'),
    
    -- Family Theater (CinemaId = 4)
    (4, 'Frozen II', '2025-12-05 10:00:00', '2025-12-05 12:00:00'),
    (4, 'The Lion King', '2025-12-05 14:00:00', '2025-12-05 16:00:00'),
    (4, 'Toy Story 4', '2025-12-06 11:00:00', '2025-12-06 13:00:00'),
    
    -- Student Special (CinemaId = 5)
    (5, 'The Avengers: Endgame', '2025-12-05 12:00:00', '2025-12-05 15:00:00'),
    (5, 'Guardians of the Galaxy Vol. 3', '2025-12-06 13:00:00', '2025-12-06 15:30:00');

-- =====================================================
-- SEED DATA - RESERVATIONS
-- ✅ UPDATED: SeatNumber now uses INT (e.g., 5, 6, 10, etc.)
-- ✅ ADDED: IsPaid column (TRUE for paid, FALSE for unpaid)
-- =====================================================

INSERT INTO Reservations (UserId, ScreeningId, SeatNumber, ReservationDate, IsPaid)
VALUES 
    -- john (UserId = 6) reservations
    (6, 1, 5, '2025-12-01 10:30:00', TRUE),    -- The Matrix - Row A, Seat 5
    (6, 1, 6, '2025-12-01 10:30:00', TRUE),    -- The Matrix - Row A, Seat 6
    (6, 5, 70, '2025-12-02 14:20:00', TRUE),   -- Oppenheimer - Seat 70
    (6, 6, 15, '2025-12-03 09:15:00', FALSE),  -- The Dark Knight - Seat 15 (unpaid)
    (6, 15, 15, '2025-12-04 16:45:00', TRUE),  -- Guardians - Seat 15
    
    -- jane (UserId = 7) reservations
    (7, 2, 27, '2025-12-01 11:00:00', TRUE),   -- Inception - Seat 27
    (7, 2, 28, '2025-12-01 11:00:00', TRUE),   -- Inception - Seat 28
    (7, 8, 92, '2025-12-02 15:30:00', TRUE),   -- Avatar - Seat 92
    (7, 10, 87, '2025-12-03 13:20:00', FALSE), -- Spider-Man - Seat 87 (unpaid)
    (7, 3, 32, '2025-12-04 10:00:00', TRUE),   -- Interstellar - Seat 32
    
    -- alice (UserId = 8) reservations
    (8, 3, 95, '2025-12-01 12:45:00', TRUE),   -- Interstellar - Seat 95
    (8, 7, 1, '2025-12-02 16:00:00', TRUE),    -- Barbie - Seat 1
    (8, 7, 2, '2025-12-02 16:00:00', TRUE),    -- Barbie - Seat 2
    (8, 11, 65, '2025-12-03 10:30:00', TRUE),  -- Frozen II - Seat 65
    (8, 12, 44, '2025-12-04 11:15:00', FALSE), -- The Lion King - Seat 44 (unpaid)
    
    -- bob (UserId = 9) reservations
    (9, 4, 160, '2025-12-01 14:00:00', TRUE),  -- Dune - Seat 160
    (9, 9, 48, '2025-12-02 18:00:00', TRUE),   -- Top Gun - Seat 48
    (9, 14, 110, '2025-12-03 11:00:00', TRUE), -- The Avengers - Seat 110
    (9, 13, 69, '2025-12-04 09:30:00', FALSE), -- Toy Story - Seat 69 (unpaid)
    
    -- Group booking examples (alice - The Matrix)
    (8, 1, 45, '2025-12-01 15:30:00', TRUE),   -- Row C, Seat 5
    (8, 1, 46, '2025-12-01 15:30:00', TRUE),   -- Row C, Seat 6
    (8, 1, 47, '2025-12-01 15:30:00', TRUE),   -- Row C, Seat 7
    
    -- Group booking (bob - Oppenheimer)
    (9, 5, 81, '2025-12-02 17:00:00', TRUE),   -- Row E, Seat 1
    (9, 5, 82, '2025-12-02 17:00:00', TRUE),   -- Row E, Seat 2
    (9, 5, 83, '2025-12-02 17:00:00', TRUE);   -- Row E, Seat 3

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

SELECT '✅ Database initialized successfully!' AS Status;
SELECT '=====================================' AS Separator;

SELECT CONCAT('👥 Total Users: ', COUNT(*)) AS UserStats FROM Users;
SELECT CONCAT('   - Admins: ', COUNT(*)) AS AdminCount FROM Users WHERE IsAdmin = TRUE;
SELECT CONCAT('   - Regular Users: ', COUNT(*)) AS RegularUserCount FROM Users WHERE IsAdmin = FALSE;

SELECT '=====================================' AS Separator;
SELECT CONCAT('🎬 Total Cinemas: ', COUNT(*)) AS CinemaStats FROM Cinemas;
SELECT CONCAT('🎥 Total Screenings: ', COUNT(*)) AS ScreeningStats FROM Screenings;
SELECT CONCAT('🎫 Total Reservations: ', COUNT(*)) AS ReservationStats FROM Reservations;
SELECT CONCAT('💰 Paid Reservations: ', COUNT(*)) AS PaidReservations FROM Reservations WHERE IsPaid = TRUE;
SELECT CONCAT('⏳ Unpaid Reservations: ', COUNT(*)) AS UnpaidReservations FROM Reservations WHERE IsPaid = FALSE;

SELECT '=====================================' AS Separator;
SELECT '📊 Detailed Statistics:' AS DetailedStats;

-- Show screenings per cinema
SELECT 
    c.Name AS Cinema, 
    COUNT(s.Id) AS TotalScreenings 
FROM Cinemas c 
LEFT JOIN Screenings s ON c.Id = s.CinemaId 
GROUP BY c.Id, c.Name;

SELECT '=====================================' AS Separator;

-- Show reservations per user
SELECT 
    u.Username, 
    u.FirstName, 
    u.LastName, 
    COUNT(r.Id) AS TotalReservations,
    SUM(CASE WHEN r.IsPaid = TRUE THEN 1 ELSE 0 END) AS PaidCount,
    SUM(CASE WHEN r.IsPaid = FALSE THEN 1 ELSE 0 END) AS UnpaidCount
FROM Users u 
LEFT JOIN Reservations r ON u.Id = r.UserId 
WHERE u.IsAdmin = FALSE
GROUP BY u.Id, u.Username, u.FirstName, u.LastName
ORDER BY TotalReservations DESC;

SELECT '=====================================' AS Separator;

-- Show most popular screenings
SELECT 
    s.MovieTitle, 
    c.Name AS Cinema,
    s.StartTime,
    COUNT(r.Id) AS TotalBookings,
    SUM(CASE WHEN r.IsPaid = TRUE THEN 1 ELSE 0 END) AS PaidBookings
FROM Screenings s 
LEFT JOIN Reservations r ON s.Id = r.ScreeningId 
LEFT JOIN Cinemas c ON s.CinemaId = c.Id
GROUP BY s.Id, s.MovieTitle, c.Name, s.StartTime
ORDER BY TotalBookings DESC
LIMIT 5;

SELECT '=====================================' AS Separator;

-- Show reservation details with seat numbers as INT
SELECT 
    u.Username,
    s.MovieTitle,
    r.SeatNumber,
    r.ReservationDate,
    r.IsPaid,
    s.StartTime AS ScreeningTime
FROM Reservations r
INNER JOIN Users u ON r.UserId = u.Id
INNER JOIN Screenings s ON r.ScreeningId = s.Id
WHERE u.IsAdmin = FALSE
ORDER BY r.ReservationDate DESC
LIMIT 10;

SELECT '=====================================' AS Separator;
SELECT '✅ Initialization complete! Ready to test application.' AS FinalStatus;
SELECT '✅ Schema updated: SeatNumber INT, IsPaid BOOLEAN, CreatedAt DATETIME' AS SchemaChanges;
