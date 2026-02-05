package com.cinema.ticketsystem.service.implementation;

import com.cinema.ticketsystem.dto.*;
import com.cinema.ticketsystem.mapper.ScreeningDetailMapper;
import com.cinema.ticketsystem.mapper.ScreeningMapper;
import com.cinema.ticketsystem.model.Cinema;
import com.cinema.ticketsystem.model.Movie;
import com.cinema.ticketsystem.model.Screening;
import com.cinema.ticketsystem.repository.MovieRepository;
import com.cinema.ticketsystem.repository.ScreeningRepository;
import com.cinema.ticketsystem.service.CinemaService;
import com.cinema.ticketsystem.service.ScreeningService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class ScreeningServiceImpl implements ScreeningService {
    
    private final ScreeningRepository screeningRepository;
    private final CinemaService cinemaService;
    private final MovieRepository movieRepository;
    private final ScreeningMapper screeningMapper;
    private final ScreeningDetailMapper screeningDetailMapper;
    
    public List<ScreeningDTO> getAllScreenings() {
        return screeningRepository.findAllByOrderByStartDateTimeAsc().stream()
                .map(screeningMapper::toDTO)
                .collect(Collectors.toList());
    }
    
    public ScreeningDetailDTO getScreeningWithSeats(Long id) {
        if (id==null) {
            throw new RuntimeException("Screening ID cannot be null");
        }
        Screening screening = screeningRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Screening not found"));
        
        List<SeatDTO> seats = createSeatsForScreening(screening);
        return screeningDetailMapper.toDTO(screening, seats);
    }
    
    private List<SeatDTO> createSeatsForScreening(Screening screening) {
    List<SeatDTO> seats = new ArrayList<>();
    Cinema cinema = screening.getCinema();
    
    for (int row = 1; row <= cinema.getRows(); row++) {
        for (int number = 1; number <= cinema.getSeatsPerRow(); number++) {
            SeatDTO seat = new SeatDTO();
            seat.setRow(row);
            seat.setSeat(number);
            seat.setIsReserved(isSeatReserved(screening, row, number));
            seats.add(seat);
        }
    }
    return seats;
}
    
    private boolean isSeatReserved(Screening screening, int row, int number) {
        return screening.getReservations().stream()
                .anyMatch(reservation -> reservation.getRow() == row 
                        && reservation.getSeat() == number);
    }
    
    @Transactional
    public ScreeningDTO createScreening(CreateScreeningRequest request) {
        Cinema cinema = cinemaService.getCinemaById(request.getCinemaId());
        Long movieId = request.getMovieId();
        if (movieId == null) {
            throw new RuntimeException("Movie ID cannot be null");
        }
        Movie movie = movieRepository.findById(movieId)
                .orElseThrow(() -> new RuntimeException("Movie not found"));
        
        Screening screening = new Screening();
        screening.setCinema(cinema);
        screening.setMovie(movie);
        screening.setStartDateTime(request.getStartDateTime());
        screening.setTicketPrice(request.getTicketPrice());
        
        Screening savedScreening = screeningRepository.save(screening);
        return screeningMapper.toDTO(savedScreening);
    }
    
    @Transactional
    public void deleteScreening(Long id) {
        if (id == null) {
            throw new RuntimeException("Screening ID cannot be null");
        }
        Screening screening = screeningRepository.findById(id)
                .orElseThrow(() -> new RuntimeException("Screening not found"));
        
        // Cascade delete will automatically remove reservations

        if (screening == null) {
            throw new RuntimeException("Screening ID cannot be null");
        }
        screeningRepository.delete(screening);
    }
}
