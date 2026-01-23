package com.cinema.ticketsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class ScreeningDetailDTO {
    private Long id;
    private Long cinemaId;
    private String cinemaName;
    private Long movieId;
    private String movieTitle;
    private LocalDateTime startDateTime;
    private BigDecimal ticketPrice;
    private Integer rows;
    private Integer seatsPerRow;
    private List<SeatDTO> seats;
}
