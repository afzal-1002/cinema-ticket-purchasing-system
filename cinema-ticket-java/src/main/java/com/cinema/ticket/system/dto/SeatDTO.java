package com.cinema.ticket.system.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class SeatDTO {
    private Integer row;
    private Integer seat;
    private Boolean isReserved;
    private Long userId;
}
