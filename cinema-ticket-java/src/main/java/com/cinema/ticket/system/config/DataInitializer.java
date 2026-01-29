package com.cinema.ticket.system.config;

import com.cinema.ticket.system.model.Cinema;
import com.cinema.ticket.system.model.Movie;
import com.cinema.ticket.system.model.Screening;
import com.cinema.ticket.system.model.User;
import com.cinema.ticket.system.repository.CinemaRepository;
import com.cinema.ticket.system.repository.MovieRepository;
import com.cinema.ticket.system.repository.ScreeningRepository;
import com.cinema.ticket.system.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;

@Component
public class DataInitializer implements CommandLineRunner {
    
    @Autowired
    private CinemaRepository cinemaRepository;
    
    @Autowired
    private UserRepository userRepository;
    
    @Autowired
    private MovieRepository movieRepository;

    @Autowired
    private ScreeningRepository screeningRepository;

    @Autowired
    private PasswordEncoder passwordEncoder;
    
    @Override
    @Transactional
    public void run(String... args) throws Exception {
        ensureOptimisticLockVersions();

        // Initialize cinemas if not already present
        if (cinemaRepository.count() == 0) {
            Cinema cinema1 = new Cinema();
            cinema1.setName("Cinema Grand Plaza");
            cinema1.setRows(10);
            cinema1.setSeatsPerRow(15);
            cinemaRepository.save(cinema1);
            
            Cinema cinema2 = new Cinema();
            cinema2.setName("Cinema Royal");
            cinema2.setRows(8);
            cinema2.setSeatsPerRow(12);
            cinemaRepository.save(cinema2);
            
            Cinema cinema3 = new Cinema();
            cinema3.setName("Cinema Star");
            cinema3.setRows(12);
            cinema3.setSeatsPerRow(20);
            cinemaRepository.save(cinema3);
            
            Cinema cinema4 = new Cinema();
            cinema4.setName("Cinema Downtown");
            cinema4.setRows(6);
            cinema4.setSeatsPerRow(10);
            cinemaRepository.save(cinema4);
            
            Cinema cinema5 = new Cinema();
            cinema5.setName("Cinema Luxury");
            cinema5.setRows(5);
            cinema5.setSeatsPerRow(8);
            cinemaRepository.save(cinema5);
            
            System.out.println("✅ Initialized 5 cinemas in the database");
        }

        if (movieRepository.count() == 0) {
            Movie dune = new Movie();
            dune.setTitle("Dune: Part Two");
            dune.setDescription("Paul Atreides unites with the Fremen to unleash his destiny across Arrakis.");
            dune.setDurationMinutes(165);
            dune.setGenre("Sci-Fi");
            dune.setRating("PG-13");
            dune.setDirector("Denis Villeneuve");
            dune.setCast("Timothée Chalamet, Zendaya, Rebecca Ferguson");
            dune.setPosterUrl("https://example.com/posters/dune2.jpg");
            dune.setTrailerUrl("https://youtube.com/watch?v=dune2");
            dune.setReleaseDate(LocalDate.of(2024, 3, 1));
            dune.setIsActive(true);
            movieRepository.save(dune);

            Movie oppenheimer = new Movie();
            oppenheimer.setTitle("Oppenheimer");
            oppenheimer.setDescription("The story of J. Robert Oppenheimer and the creation of the atomic bomb.");
            oppenheimer.setDurationMinutes(180);
            oppenheimer.setGenre("Drama");
            oppenheimer.setRating("R");
            oppenheimer.setDirector("Christopher Nolan");
            oppenheimer.setCast("Cillian Murphy, Emily Blunt, Matt Damon");
            oppenheimer.setPosterUrl("https://example.com/posters/oppenheimer.jpg");
            oppenheimer.setTrailerUrl("https://youtube.com/watch?v=oppenheimer");
            oppenheimer.setReleaseDate(LocalDate.of(2023, 7, 21));
            oppenheimer.setIsActive(true);
            movieRepository.save(oppenheimer);

            Movie mario = new Movie();
            mario.setTitle("The Super Mario Bros. Movie");
            mario.setDescription("A plumber named Mario travels through the Mushroom Kingdom to save his brother.");
            mario.setDurationMinutes(92);
            mario.setGenre("Animation");
            mario.setRating("PG");
            mario.setDirector("Aaron Horvath & Michael Jelenic");
            mario.setCast("Chris Pratt, Anya Taylor-Joy, Jack Black");
            mario.setPosterUrl("https://example.com/posters/mario.jpg");
            mario.setTrailerUrl("https://youtube.com/watch?v=mario");
            mario.setReleaseDate(LocalDate.of(2023, 4, 5));
            mario.setIsActive(true);
            movieRepository.save(mario);

            System.out.println("✅ Seeded default movies (Dune, Oppenheimer, Mario)");
        }

        if (screeningRepository.count() == 0 && movieRepository.count() >= 2 && cinemaRepository.count() >= 1) {
            List<Movie> movies = movieRepository.findAll();
            List<Cinema> cinemas = cinemaRepository.findAll();

            int movieIndex = 0;
            int cinemaIndex = 0;
            for (int i = 0; i < Math.min(3, cinemas.size()); i++) {
                Screening screening = new Screening();
                screening.setCinema(cinemas.get(cinemaIndex % cinemas.size()));
                screening.setMovie(movies.get(movieIndex % movies.size()));
                screening.setStartDateTime(LocalDateTime.now().plusDays(i + 1).withHour(19).withMinute(0));
                screening.setTicketPrice(BigDecimal.valueOf(14.99 + i));
                screeningRepository.save(screening);
                movieIndex++;
                cinemaIndex++;
            }

            System.out.println("✅ Seeded default screenings using available movies and cinemas");
        }
        
        // Create admin user if not present
        if (!userRepository.existsByEmail("admin@gmail.com")) {
            User admin = new User();
            admin.setEmail("admin@gmail.com");
            admin.setPassword(passwordEncoder.encode("admin123"));
            admin.setFirstName("Admin");
            admin.setLastName("User");
            admin.setPhoneNumber("+1234567890");
            admin.setIsAdmin(true);
            userRepository.save(admin);
                  System.out.println("✅ Created admin user - Email: admin@gmail.com, Password: admin123");
        }

        if (!userRepository.existsByEmail("admin1@gmail.com")) {
            User admin = new User();
            admin.setEmail("admin1@gmail.com");
            admin.setPassword(passwordEncoder.encode("admin123"));
            admin.setFirstName("Admin");
            admin.setLastName("User");
            admin.setPhoneNumber("+1234567890");
            admin.setIsAdmin(true);
            userRepository.save(admin);
                  System.out.println("✅ Created admin user - Email: admin1@gmail.com, Password: admin123");
        }



        // Create sample customer user if not present
        if (!userRepository.existsByEmail("user@gmail.com")) {
            User customer = new User();
            customer.setEmail("user@gmail.com");
            customer.setPassword(passwordEncoder.encode("user123"));
            customer.setFirstName("Movie");
            customer.setLastName("Fan");
            customer.setPhoneNumber("+1987654321");
            customer.setIsAdmin(false);
            userRepository.save(customer);
            System.out.println("✅ Created demo user - Email: user@gmail.com, Password: user123");
        }
    }

    @Transactional
    protected void ensureOptimisticLockVersions() {
        int cinemaFixes = cinemaRepository.initializeMissingVersion();
        int movieFixes = movieRepository.initializeMissingVersion();
        if (cinemaFixes > 0 || movieFixes > 0) {
            System.out.printf("✅ Normalized version columns (cinemas=%d, movies=%d)%n", cinemaFixes, movieFixes);
        }
    }
}
