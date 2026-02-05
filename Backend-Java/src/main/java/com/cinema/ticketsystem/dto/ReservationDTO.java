package com.cinema.ticketsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class ReservationDTO {
    private Long id;
    private Long screeningId;
    private String movieTitle;
    private LocalDateTime startDateTime;
    private String cinemaName;
    private Integer row;
    private Integer seat;
    private LocalDateTime createdAt;
}
