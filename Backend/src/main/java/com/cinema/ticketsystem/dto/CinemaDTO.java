package com.cinema.ticketsystem.dto;

import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CinemaDTO {
    private Long id;
    @NotBlank
    @Size(max = 100)
    private String name;
    @NotNull
    @Min(1)
    private Integer rows;
    @NotNull
    @Min(1)
    private Integer seatsPerRow;
    
    public Integer getTotalSeats() {
        if (rows == null || seatsPerRow == null) {
            return null;
        }
        return rows * seatsPerRow;
    }
}
