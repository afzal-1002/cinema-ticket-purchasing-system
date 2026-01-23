package com.cinema.ticketsystem.dto;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreateReservationRequest {
    
    @NotNull(message = "Screening ID is required")
    private Long screeningId;
    
    @NotNull(message = "Row is required")
    @Min(value = 1, message = "Row must be at least 1")
    private Integer row;
    
    @NotNull(message = "Seat is required")
    @Min(value = 1, message = "Seat must be at least 1")
    private Integer seat;
}
