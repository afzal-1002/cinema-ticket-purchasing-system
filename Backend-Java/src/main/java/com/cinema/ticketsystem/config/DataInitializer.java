package com.cinema.ticketsystem.config;

import com.cinema.ticketsystem.model.Cinema;
import com.cinema.ticketsystem.model.User;
import com.cinema.ticketsystem.repository.CinemaRepository;
import com.cinema.ticketsystem.repository.UserRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;

@Component
public class DataInitializer implements CommandLineRunner {
    
    @Autowired
    private CinemaRepository cinemaRepository;
    
    @Autowired
    private UserRepository userRepository;
    
    @Autowired
    private PasswordEncoder passwordEncoder;
    
    @Override
    public void run(String... args) throws Exception {
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
        
        // Create admin user if not present
        if (!userRepository.existsByEmail("admin@cinema.com")) {
            User admin = new User();
            admin.setEmail("admin@cinema.com");
            admin.setPassword(passwordEncoder.encode("Admin123!"));
            admin.setFirstName("Admin");
            admin.setLastName("User");
            admin.setPhoneNumber("+1234567890");
            admin.setIsAdmin(true);
            userRepository.save(admin);
            
            System.out.println("✅ Created admin user - Email: admin@cinema.com, Password: Admin123!");
        }
    }
}
