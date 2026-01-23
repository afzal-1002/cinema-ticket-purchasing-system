# Cinema Ticket Purchasing System - Spring Boot Backend

## Project Overview
This is a complete backend implementation for a Cinema Ticket Purchasing System using Spring Boot, JPA/Hibernate, and JWT authentication.

## Technologies Used
- **Java 17**
- **Spring Boot 3.2.1**
- **Spring Data JPA** (Entity Framework equivalent)
- **Spring Security** with JWT
- **H2 Database** (for development)
- **MySQL/SQL Server** support (for production)
- **Maven** (dependency management)
- **Lombok** (reduce boilerplate code)

## Features Implemented

### Task 3 Requirements ✅
1. **User Registration and Editing**
   - New users can register
   - Users can update their own profile (name, surname, phone number)
   - Administrators can edit any user's profile
   - **Optimistic locking** implemented using `@Version` annotation for concurrency control

2. **User/Session Management with Concurrency**
   - Parallelism handled when editing/deleting users
   - Optimistic locking prevents lost updates
   - Proper error handling for concurrent modifications

3. **Screening Creation and Deletion**
   - Administrators can create new film screenings
   - Administrators can delete screenings
   - **Cascade delete** automatically removes all reservations when a screening is deleted

### Task 4 Requirements ✅
1. **Seat Reservation/Cancellation**
   - Users can reserve seats by specifying row and seat position (x, y)
   - Users can cancel their own reservations
   - Seat validation ensures row/seat are within cinema bounds

2. **Conflict Handling**
   - **Unique constraint** on (screening_id, row, seat) prevents double booking
   - **Optimistic locking** on Reservation entity handles concurrent reservation attempts
   - Database-level constraint ensures data integrity
   - Clear error messages when seat is already reserved

3. **Display Occupied Seats**
   - API endpoint shows all seats for a screening
   - Marks which seats are reserved and by whom
   - Real-time seat availability information

4. **Production Configuration**
   - Separate `application-prod.properties` for production
   - Support for MySQL and SQL Server databases
   - Security settings optimized for production
   - H2 console disabled in production

## Database Structure

### Entities
1. **User**
   - id, email (unique), password, firstName, lastName, phoneNumber
   - isAdmin flag
   - version field for optimistic locking
   - createdAt timestamp

2. **Cinema**
   - id, name, rows, seatsPerRow
   - Pre-seeded with 5 cinemas

3. **Screening**
   - id, cinemaId, movieTitle, startDateTime
   - createdAt timestamp
   - Cascade delete to reservations

4. **Reservation**
   - id, userId, screeningId, row, seat
   - Unique constraint on (screeningId, row, seat)
   - version field for optimistic locking
   - createdAt timestamp

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Users
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/me` - Get current user
- `PUT /api/users/{id}` - Update user (own profile or Admin)
- `DELETE /api/users/{id}` - Delete user (Admin only)

### Cinemas
- `GET /api/cinemas` - Get all cinemas (public)

### Screenings
- `GET /api/screenings` - Get all screenings (public)
- `GET /api/screenings/{id}` - Get screening with seat map
- `POST /api/screenings` - Create screening (Admin only)
- `DELETE /api/screenings/{id}` - Delete screening (Admin only)

### Reservations
- `GET /api/reservations/my` - Get current user's reservations
- `POST /api/reservations` - Create reservation
- `DELETE /api/reservations/screening/{screeningId}/row/{row}/seat/{seat}` - Cancel reservation

## Running the Application

### Prerequisites
- Java 17 or higher
- Maven 3.6+

### Development Mode
```bash
cd Backend
mvn clean install
mvn spring-boot:run
```

The application will start at:
- **API**: http://localhost:8080
- **H2 Console**: http://localhost:8080/h2-console
  - JDBC URL: `jdbc:h2:mem:cinemadb`
  - Username: `sa`
  - Password: (leave empty)

### Production Mode
```bash
# Update application-prod.properties with your database credentials
mvn clean package
java -jar -Dspring.profiles.active=prod target/ticket-system-1.0.0.jar
```

## Default Admin Credentials
- **Email**: admin@cinema.com
- **Password**: Admin123!

## Concurrency Control

### User Updates (Task 3)
- Uses `@Version` annotation on User entity
- When updating a user, the version must match
- If another user modified the record, OptimisticLockException is thrown
- Client receives 409 Conflict status with appropriate message

### Seat Reservations (Task 4)
- Unique database constraint on (screening_id, row, seat)
- `@Version` annotation on Reservation entity
- When two users try to reserve the same seat:
  1. First transaction succeeds
  2. Second transaction fails with DataIntegrityViolationException
  3. User receives clear message: "This seat is already reserved"

## Database Configuration

### Development (H2 - In-Memory)
```properties
spring.datasource.url=jdbc:h2:mem:cinemadb
spring.jpa.hibernate.ddl-auto=create-drop
```

### Production Options

#### MySQL
```properties
spring.datasource.url=jdbc:mysql://localhost:3306/cinemadb
spring.datasource.username=root
spring.datasource.password=yourpassword
spring.jpa.database-platform=org.hibernate.dialect.MySQLDialect
spring.jpa.hibernate.ddl-auto=update
```

#### SQL Server
```properties
spring.datasource.url=jdbc:sqlserver://localhost:1433;databaseName=cinemadb
spring.datasource.username=sa
spring.datasource.password=yourpassword
spring.jpa.database-platform=org.hibernate.dialect.SQLServerDialect
spring.jpa.hibernate.ddl-auto=update
```

## Security
- JWT-based authentication
- Password encryption using BCrypt
- Role-based access control (USER, ADMIN)
- CORS configured for Angular frontend (http://localhost:4200)
- Stateless session management

## Testing the API

### Using the H2 Console
1. Start the application
2. Go to http://localhost:8080/h2-console
3. Use JDBC URL: `jdbc:h2:mem:cinemadb`, username: `sa`, password: (empty)
4. You can query all tables directly

### Using Postman/curl

**Register a new user:**
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@cinema.com",
    "password": "Admin123!"
  }'
```

**Create a screening (Admin):**
```bash
curl -X POST http://localhost:8080/api/screenings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "cinemaId": 1,
    "movieTitle": "The Matrix",
    "startDateTime": "2026-02-01T19:00:00"
  }'
```

**Reserve a seat:**
```bash
curl -X POST http://localhost:8080/api/reservations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "screeningId": 1,
    "row": 5,
    "seat": 10
  }'
```

## Project Structure
```
Backend/
├── src/
│   ├── main/
│   │   ├── java/com/cinema/ticketsystem/
│   │   │   ├── config/          # Security & Data Initialization
│   │   │   ├── controller/      # REST Controllers
│   │   │   ├── dto/             # Data Transfer Objects
│   │   │   ├── model/           # JPA Entities
│   │   │   ├── repository/      # JPA Repositories
│   │   │   ├── security/        # JWT & Security
│   │   │   ├── service/         # Business Logic
│   │   │   └── CinemaTicketSystemApplication.java
│   │   └── resources/
│   │       ├── application.properties
│   │       └── application-prod.properties
│   └── test/
├── pom.xml
└── README.md
```

## Next Steps
1. Build and run the backend
2. Test all endpoints using Postman or H2 Console
3. Integrate with Angular frontend
4. Deploy to production server

## Support
For issues or questions, refer to the Spring Boot documentation:
- https://spring.io/projects/spring-boot
- https://spring.io/guides/gs/securing-web/
- https://spring.io/guides/gs/accessing-data-jpa/
