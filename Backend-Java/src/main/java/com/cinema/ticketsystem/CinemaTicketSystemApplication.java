package com.cinema.ticketsystem;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableAsync;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
@EnableAsync
public class CinemaTicketSystemApplication {

    public static void main(String[] args) {
        SpringApplication.run(CinemaTicketSystemApplication.class, args);
        System.out.println("\n" +
                "╔════════════════════════════════════════════════════════════╗\n" +
                "║   Cinema Ticket Purchasing System - Spring Boot Backend    ║\n" +
                "║                                                            ║\n" +
                "║   Server running at: http://localhost:8080                 ║\n" +
                "║   H2 Console: http://localhost:8080/h2-console             ║\n" +
                "║                                                            ║\n" +
                "║   Default Admin Credentials:                               ║\n" +
                "║   Email: admin@cinema.com                                  ║\n" +
                "║   Password: Admin123!                                      ║\n" +
                "╚════════════════════════════════════════════════════════════╝\n");
    }
}
