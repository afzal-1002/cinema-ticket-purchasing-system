package com.cinema.ticketsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CinemaDTO {
    private Long id;
    private String name;
    private Integer rows;
    private Integer seatsPerRow;
    
    public Integer getTotalSeats() {
        return rows * seatsPerRow;
    }
}
