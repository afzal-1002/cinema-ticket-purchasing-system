package com.cinema.ticket.system;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableAsync;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
@EnableAsync
public class CinemaTicketSystem {

	public static void main(String[] args) {
		SpringApplication.run(CinemaTicketSystem.class, args);
		System.out.println("""
                
                ╔════════════════════════════════════════════════════════════╗
                ║   Cinema Ticket Purchasing System - Spring Boot Backend    ║
                ║                                                            ║
                ║   Server running at: http://localhost:8080                 ║
                ║   H2 Console: http://localhost:8080/h2-console             ║
                ║                                                            ║
                ║   Default Admin Credentials:                               ║
                ║   Email: admin@gmail.com                                   ║
                ║   Password: admin123                                       ║
                ║                                                            ║
                ║   Default User Credentials:                                ║
                ║   Email: user@gmail.com                                    ║
                ║   Password: user123                                        ║
                ╚════════════════════════════════════════════════════════════╝
                """);
	}
}
