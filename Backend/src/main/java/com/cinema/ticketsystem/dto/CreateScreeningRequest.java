package com.cinema.ticketsystem.dto;

import jakarta.validation.constraints.NotNull;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreateScreeningRequest {
    
    @NotNull(message = "Cinema ID is required")
    private Long cinemaId;
    
    @NotNull(message = "Movie ID is required")
    private Long movieId;
    
    @NotNull(message = "Start date time is required")
    private LocalDateTime startDateTime;
    
    @NotNull(message = "Ticket price is required")
    private java.math.BigDecimal ticketPrice;
}
