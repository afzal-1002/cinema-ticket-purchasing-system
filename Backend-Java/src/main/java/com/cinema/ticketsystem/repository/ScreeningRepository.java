package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.Screening;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface ScreeningRepository extends JpaRepository<Screening, Long> {
    List<Screening> findAllByOrderByStartDateTimeAsc();
}
