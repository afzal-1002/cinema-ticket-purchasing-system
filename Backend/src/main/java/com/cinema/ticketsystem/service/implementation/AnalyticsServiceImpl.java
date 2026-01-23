package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.model.Payment;
import com.cinema.ticketsystem.repository.*;
import com.cinema.ticketsystem.service.AnalyticsService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class AnalyticsServiceImpl implements AnalyticsService {
    
    private final PaymentRepository paymentRepository;
    private final ReservationRepository reservationRepository;
    private final ScreeningRepository screeningRepository;
    private final MovieRepository movieRepository;
    private final UserRepository userRepository;
    
    @Transactional(readOnly = true)
    public Map<String, Object> getDashboardOverview() {
        Map<String, Object> overview = new HashMap<>();
        
        // Total users
        long totalUsers = userRepository.count();
        overview.put("totalUsers", totalUsers);
        
        // Total movies
        long totalMovies = movieRepository.count();
        long activeMovies = movieRepository.findByIsActiveTrue().size();
        overview.put("totalMovies", totalMovies);
        overview.put("activeMovies", activeMovies);
        
        // Total screenings
        long totalScreenings = screeningRepository.count();
        overview.put("totalScreenings", totalScreenings);
        
        // Total reservations
        long totalReservations = reservationRepository.count();
        overview.put("totalReservations", totalReservations);
        
        // Revenue statistics
        LocalDateTime now = LocalDateTime.now();
        LocalDateTime startOfMonth = now.withDayOfMonth(1).withHour(0).withMinute(0).withSecond(0);
        
        BigDecimal monthlyRevenue = paymentRepository.getTotalRevenueBetween(startOfMonth, now)
                .orElse(BigDecimal.ZERO);
        overview.put("monthlyRevenue", monthlyRevenue);
        
        LocalDateTime startOfYear = now.withMonth(1).withDayOfMonth(1).withHour(0).withMinute(0).withSecond(0);
        BigDecimal yearlyRevenue = paymentRepository.getTotalRevenueBetween(startOfYear, now)
                .orElse(BigDecimal.ZERO);
        overview.put("yearlyRevenue", yearlyRevenue);
        
        return overview;
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getRevenueAnalytics(LocalDate startDate, LocalDate endDate) {
        LocalDateTime startDateTime = startDate.atStartOfDay();
        LocalDateTime endDateTime = endDate.atTime(23, 59, 59);
        
        List<Payment> payments = paymentRepository.findCompletedPaymentsBetween(startDateTime, endDateTime);
        
        Map<String, Object> analytics = new HashMap<>();
        
        // Total revenue
        BigDecimal totalRevenue = payments.stream()
                .map(Payment::getAmount)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
        analytics.put("totalRevenue", totalRevenue);
        
        // Total transactions
        analytics.put("totalTransactions", payments.size());
        
        // Average transaction value
        BigDecimal averageValue = payments.isEmpty() 
                ? BigDecimal.ZERO 
                : totalRevenue.divide(BigDecimal.valueOf(payments.size()), 2, RoundingMode.HALF_UP);
        analytics.put("averageTransactionValue", averageValue);
        
        // Revenue by payment method
        Map<String, BigDecimal> revenueByMethod = payments.stream()
                .collect(Collectors.groupingBy(
                        p -> p.getPaymentMethod().toString(),
                        Collectors.mapping(Payment::getAmount,
                                Collectors.reducing(BigDecimal.ZERO, BigDecimal::add))
                ));
        analytics.put("revenueByPaymentMethod", revenueByMethod);
        
        // Daily revenue breakdown
        Map<LocalDate, BigDecimal> dailyRevenue = payments.stream()
                .collect(Collectors.groupingBy(
                        p -> p.getCompletedAt().toLocalDate(),
                        TreeMap::new,
                        Collectors.mapping(Payment::getAmount,
                                Collectors.reducing(BigDecimal.ZERO, BigDecimal::add))
                ));
        analytics.put("dailyRevenue", dailyRevenue);
        
        return analytics;
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getPopularMovies(int limit) {
        return screeningRepository.findAll().stream()
                .collect(Collectors.groupingBy(
                        screening -> screening.getMovie().getId(),
                        Collectors.counting()
                ))
                .entrySet().stream()
                .sorted(Map.Entry.<Long, Long>comparingByValue().reversed())
                .limit(limit)
                .map(entry -> {
                    Map<String, Object> movieStats = new HashMap<>();
                    Long movieId = entry.getKey();
                    Long screeningCount = entry.getValue();
                    
                    if (movieId != null) {
                        movieRepository.findById(movieId).ifPresent(movie -> {
                        movieStats.put("movieId", movie.getId());
                        movieStats.put("title", movie.getTitle());
                        movieStats.put("genre", movie.getGenre());
                        movieStats.put("averageRating", movie.getAverageRating());
                        movieStats.put("totalScreenings", screeningCount);
                        
                        // Get reservation count
                        long reservationCount = reservationRepository.findAll().stream()
                                .filter(r -> r.getScreening().getMovie().getId().equals(movieId))
                                .count();
                        movieStats.put("totalReservations", reservationCount);
                        });
                    }
                    
                    return movieStats;
                })
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getOccupancyStatistics(Long screeningId) {
        if (screeningId == null) {
            return new HashMap<>();
        }
        return screeningRepository.findById(screeningId)
                .map(screening -> {
                    Map<String, Object> stats = new HashMap<>();
                    
                    int totalSeats = screening.getCinema().getRows() * screening.getCinema().getSeatsPerRow();
                    long reservedSeats = reservationRepository.findByScreeningId(screeningId).size();
                    
                    double occupancyRate = totalSeats > 0 
                            ? (reservedSeats * 100.0 / totalSeats) 
                            : 0.0;
                    
                    stats.put("screeningId", screeningId);
                    stats.put("movieTitle", screening.getMovie().getTitle());
                    stats.put("startDateTime", screening.getStartDateTime());
                    stats.put("cinemaName", screening.getCinema().getName());
                    stats.put("totalSeats", totalSeats);
                    stats.put("reservedSeats", reservedSeats);
                    stats.put("availableSeats", totalSeats - reservedSeats);
                    stats.put("occupancyRate", String.format("%.2f%%", occupancyRate));
                    
                    return stats;
                })
                .orElse(new HashMap<>());
    }
    
    @Transactional(readOnly = true)
    public List<Map<String, Object>> getAverageOccupancyByMovie(int limit) {
        return movieRepository.findAll().stream()
                .map(movie -> {
                    List<Long> screeningIds = screeningRepository.findAll().stream()
                            .filter(s -> s.getMovie().getId().equals(movie.getId()))
                            .map(screening -> screening.getId())
                            .collect(Collectors.toList());
                    
                    if (screeningIds.isEmpty()) {
                        return null;
                    }
                    
                    double avgOccupancy = screeningIds.stream()
                            .mapToDouble(screeningId -> {
                                if (screeningId == null) return 0.0;
                                var screening = screeningRepository.findById(screeningId).orElse(null);
                                if (screening == null) return 0.0;
                                
                                int totalSeats = screening.getCinema().getRows() * screening.getCinema().getSeatsPerRow();
                                long reservedSeats = reservationRepository.findByScreeningId(screeningId).size();
                                
                                return totalSeats > 0 ? (reservedSeats * 100.0 / totalSeats) : 0.0;
                            })
                            .average()
                            .orElse(0.0);
                    
                    Map<String, Object> movieOccupancy = new HashMap<>();
                    movieOccupancy.put("movieId", movie.getId());
                    movieOccupancy.put("title", movie.getTitle());
                    movieOccupancy.put("genre", movie.getGenre());
                    movieOccupancy.put("totalScreenings", screeningIds.size());
                    movieOccupancy.put("averageOccupancy", String.format("%.2f%%", avgOccupancy));
                    
                    return movieOccupancy;
                })
                .filter(Objects::nonNull)
                .sorted((a, b) -> {
                    String occA = (String) a.get("averageOccupancy");
                    String occB = (String) b.get("averageOccupancy");
                    double valA = Double.parseDouble(occA.replace("%", ""));
                    double valB = Double.parseDouble(occB.replace("%", ""));
                    return Double.compare(valB, valA);
                })
                .limit(limit)
                .collect(Collectors.toList());
    }
    
    @Transactional(readOnly = true)
    public Map<String, Object> getBookingTrends(int days) {
        LocalDateTime endDate = LocalDateTime.now();
        LocalDateTime startDate = endDate.minusDays(days);
        
        List<Object[]> dailyBookings = reservationRepository.findDailyBookingCount(startDate, endDate);
        
        Map<String, Object> trends = new HashMap<>();
        trends.put("periodDays", days);
        trends.put("startDate", startDate.toLocalDate());
        trends.put("endDate", endDate.toLocalDate());
        
        Map<LocalDate, Long> bookingsByDate = new TreeMap<>();
        for (Object[] row : dailyBookings) {
            LocalDate date = ((java.sql.Date) row[0]).toLocalDate();
            Long count = (Long) row[1];
            bookingsByDate.put(date, count);
        }
        
        trends.put("dailyBookings", bookingsByDate);
        
        long totalBookings = bookingsByDate.values().stream().mapToLong(Long::longValue).sum();
        double averagePerDay = days > 0 ? (double) totalBookings / days : 0.0;
        
        trends.put("totalBookings", totalBookings);
        trends.put("averageBookingsPerDay", String.format("%.2f", averagePerDay));
        
        return trends;
    }
}
