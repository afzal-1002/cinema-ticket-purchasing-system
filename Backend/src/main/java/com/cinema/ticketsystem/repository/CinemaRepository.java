package com.cinema.ticketsystem.repository;

import com.cinema.ticketsystem.model.Cinema;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

@Repository
public interface CinemaRepository extends JpaRepository<Cinema, Long> {

	@Modifying
	@Query("update Cinema c set c.version = 0 where c.version is null")
	int initializeMissingVersion();
}
