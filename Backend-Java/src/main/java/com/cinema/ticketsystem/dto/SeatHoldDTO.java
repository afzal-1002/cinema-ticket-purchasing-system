package com.cinema.ticketsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDateTime;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class SeatHoldDTO {
    private Long id;
    private Long userId;
    private Long screeningId;
    private Integer row;
    private Integer seat;
    private LocalDateTime heldAt;
    private LocalDateTime expiresAt;
    private Boolean isActive;
}
