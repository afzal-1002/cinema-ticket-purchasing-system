# Cinema Ticket Purchasing System - Java Backend

## Overview
A comprehensive cinema ticket purchasing system built with Spring Boot 3.2.1 and Java 17. This system provides a complete solution for managing movies, screenings, reservations, payments, and customer loyalty programs.

## ✅ All Features Implemented

### 1. Movie Management System
Complete CRUD operations for managing movies with rich metadata.

**Features:**
- Movie catalog with title, description, duration, genre, rating
- Director, cast, poster URL, and trailer URL support
- Movie status management (active/inactive)
- Release date tracking
- Average rating and review count
- Genre-based filtering
- Search functionality
- Top-rated movies listing
- New releases tracking

**Endpoints:**
- `GET /api/movies` - Get all active movies
- `GET /api/movies/{id}` - Get movie by ID
- `GET /api/movies/search?title={title}` - Search movies
- `GET /api/movies/genre/{genre}` - Filter by genre
- `GET /api/movies/genres` - Get all genres
- `GET /api/movies/new-releases` - Get recent releases
- `GET /api/movies/top-rated` - Get top-rated movies
- `POST /api/movies` - Create movie (Admin only)
- `PUT /api/movies/{id}` - Update movie (Admin only)
- `DELETE /api/movies/{id}` - Delete movie (Admin only)

**Review System:**
- `GET /api/movies/{movieId}/reviews` - Get movie reviews
- `POST /api/movies/reviews` - Create review
- `PUT /api/movies/reviews/{reviewId}` - Update review
- `DELETE /api/movies/reviews/{reviewId}` - Delete review
- `GET /api/movies/reviews/my-reviews` - Get user's reviews

### 2. Advanced Seat Selection
Real-time seat availability with temporary holds and visual seat mapping.

**Features:**
- Visual seat map with row and seat numbers
- Real-time seat availability (Available, Reserved, Held)
- 15-minute temporary seat holds
- Automatic hold expiration cleanup
- Seat number formatting (e.g., A1, B5)
- Hold management for users

**Endpoints:**
- `GET /api/seats/screening/{screeningId}` - Get seat availability
- `POST /api/seats/hold` - Hold seats temporarily
- `DELETE /api/seats/hold` - Release specific holds
- `DELETE /api/seats/hold/all` - Release all user holds
- `GET /api/seats/hold/my-holds` - Get user's active holds

### 3. Payment Integration
Complete payment processing system with multiple payment methods.

**Features:**
- Payment initiation and processing
- Multiple payment methods (Credit Card, Debit Card, PayPal, Stripe, Cash)
- Payment status tracking (Pending, Processing, Completed, Failed, Refunded)
- Transaction ID generation
- Refund management
- Payment history
- Integration-ready for Stripe/PayPal

**Endpoints:**
- `POST /api/payments/initiate` - Initiate payment
- `POST /api/payments/{paymentId}/process` - Process payment
- `POST /api/payments/{paymentId}/refund` - Refund payment (Admin)
- `GET /api/payments/my-payments` - Get user's payments
- `GET /api/payments/{paymentId}` - Get payment details
- `GET /api/payments/reservation/{reservationId}` - Get payment by reservation

### 4. Email Notification System
Automated email notifications using JavaMailSender.

**Features:**
- Booking confirmation emails with ticket details
- Showtime reminder emails
- Welcome emails for new users
- Promotional email campaigns
- Password reset emails
- HTML-formatted email templates
- Asynchronous email sending

**Email Templates:**
- Booking confirmation with movie, date, time, seat details
- Reminder notifications before showtime
- Welcome email with features overview
- Promotional emails with custom content

**Configuration:**
- SMTP server configuration in `application.properties`
- Supports Gmail and custom SMTP servers
- Configurable app name and sender email

### 5. Admin Dashboard & Analytics
Comprehensive analytics and reporting for administrators.

**Features:**
- Dashboard overview with key metrics
- Revenue analytics by date range
- Popular movies ranking
- Occupancy statistics per screening
- Average occupancy by movie
- Booking trends analysis
- Payment method breakdown
- Daily revenue tracking

**Endpoints:**
- `GET /api/analytics/dashboard` - Dashboard overview
- `GET /api/analytics/revenue?startDate={date}&endDate={date}` - Revenue analytics
- `GET /api/analytics/popular-movies?limit={n}` - Popular movies
- `GET /api/analytics/occupancy/screening/{id}` - Screening occupancy
- `GET /api/analytics/occupancy/movies?limit={n}` - Movie occupancy
- `GET /api/analytics/booking-trends?days={n}` - Booking trends

**Metrics Tracked:**
- Total users, movies, screenings, reservations
- Monthly and yearly revenue
- Average transaction value
- Revenue by payment method
- Daily revenue breakdown
- Occupancy rates
- Booking patterns

### 6. Loyalty & Rewards Program
Complete customer loyalty system with tiers and rewards.

**Features:**
- Loyalty account creation and management
- Points earning on ticket purchases
- Four-tier membership system (Bronze, Silver, Gold, Platinum)
- Tier-based point multipliers
- Points redemption for discounts
- Discount code management
- Transaction history tracking
- Automatic tier upgrades

**Member Tiers:**
- **Bronze**: 0+ points, 1.0x multiplier
- **Silver**: 500+ points, 1.5x multiplier
- **Gold**: 1000+ points, 2.0x multiplier
- **Platinum**: 2500+ points, 3.0x multiplier

**Discount Types:**
- Percentage discounts
- Fixed amount discounts
- Points redemption discounts
- Tier-based exclusive discounts

**Endpoints:**
- `POST /api/loyalty/account` - Create loyalty account
- `GET /api/loyalty/account` - Get loyalty account
- `POST /api/loyalty/redeem` - Redeem points
- `GET /api/loyalty/transactions` - Transaction history
- `GET /api/loyalty/discounts` - Active discounts
- `GET /api/loyalty/discounts/validate/{code}` - Validate discount
- `POST /api/loyalty/discounts` - Create discount (Admin)

## Technology Stack

### Backend Framework
- **Spring Boot 3.2.1**
- **Java 17**
- **Maven** for dependency management

### Database
- **H2** (Development/Testing)
- **MySQL** (Production-ready)
- **MS SQL Server** (Alternative option)

### Security
- **Spring Security**
- **JWT Authentication** (JJWT 0.12.3)
- Role-based access control (USER, ADMIN)

### Additional Libraries
- **Lombok** - Reduce boilerplate code
- **Spring Data JPA** - Database operations
- **Hibernate** - ORM framework
- **JavaMailSender** - Email notifications
- **Spring Validation** - Input validation

### Features
- **Scheduled Tasks** - Automatic seat hold cleanup
- **Async Processing** - Background email sending
- **Transaction Management** - ACID compliance
- **Optimistic Locking** - Concurrent reservation handling

## Database Schema

### Core Entities
1. **User** - User accounts with authentication
2. **Cinema** - Cinema halls with seating configuration
3. **Movie** - Movie catalog with metadata
4. **Screening** - Movie showtimes with pricing
5. **Reservation** - Ticket bookings
6. **Review** - Movie reviews and ratings

### Payment System
7. **Payment** - Payment transactions and status
8. **SeatHold** - Temporary seat reservations

### Loyalty System
9. **LoyaltyAccount** - Customer loyalty accounts
10. **LoyaltyTransaction** - Points transaction history
11. **Discount** - Promotional discount codes

## API Documentation

### Authentication
All authenticated endpoints require JWT token in the Authorization header:
```
Authorization: Bearer <token>
```

### Role-Based Access
- **Public**: Movie listings, seat availability
- **USER**: Bookings, payments, reviews, loyalty
- **ADMIN**: Analytics, discount management, movie CRUD

## Configuration

### Application Properties
```properties
# Database (H2 for development)
spring.datasource.url=jdbc:h2:mem:cinemadb
spring.h2.console.enabled=true

# Email Configuration
spring.mail.host=smtp.gmail.com
spring.mail.port=587
spring.mail.username=your-email@gmail.com
spring.mail.password=your-app-password

# JWT Configuration
jwt.secret=YourSecretKey
jwt.expiration=604800000

# Application
app.name=Cinema Ticket System
```

### Email Setup
To enable email notifications:
1. Configure SMTP server in `application.properties`
2. For Gmail: Enable "App Passwords" in Google Account settings
3. Update `spring.mail.username` and `spring.mail.password`

## Running the Application

### Prerequisites
- Java 17 or higher
- Maven 3.6+

### Steps
1. Navigate to the Backend folder
2. Run the application:
   ```bash
   mvn spring-boot:run
   ```
3. Access the application at: `http://localhost:8080`
4. H2 Console: `http://localhost:8080/h2-console`

### Default Admin Credentials
- Email: `admin@cinema.com`
- Password: `Admin123!`

## Security Considerations

✅ **No CVE Vulnerabilities**: All dependencies have been validated and are free from known critical and high-severity CVEs.

### Security Features
- Password encryption with BCrypt
- JWT token-based authentication
- CORS configuration
- Role-based authorization
- SQL injection protection via JPA
- XSS protection
- CSRF protection

## Business Logic Highlights

### Seat Hold System
- Seats are held for 15 minutes during booking
- Automatic cleanup every minute
- Prevents double-booking
- Optimistic locking for concurrent requests

### Points Calculation
- Base: $1 = 10 points
- Multiplied by tier bonus
- Automatic tier upgrades
- Lifetime points tracking

### Email Automation
- Asynchronous sending (non-blocking)
- HTML templates with branding
- Automatic triggers on booking
- Configurable sender and app name

### Analytics
- Real-time calculations
- Date range filtering
- Aggregated metrics
- Performance optimized queries

## Future Enhancements

Potential additions:
- SMS notifications (Twilio integration)
- QR code ticket generation
- Mobile app support (REST API ready)
- Social media login (OAuth2)
- Advanced reporting dashboard
- Multi-language support
- Multi-cinema management
- Online food ordering
- Group booking discounts

## API Testing

Use tools like Postman or curl to test the APIs:

**Example: Get Active Movies**
```bash
curl -X GET http://localhost:8080/api/movies
```

**Example: Create Booking with Authentication**
```bash
curl -X POST http://localhost:8080/api/reservations \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"screeningId": 1, "row": 5, "seat": 10}'
```

## Support

For questions or issues, please refer to the API documentation or contact the development team.

---

**Built with ❤️ using Spring Boot and Java**
