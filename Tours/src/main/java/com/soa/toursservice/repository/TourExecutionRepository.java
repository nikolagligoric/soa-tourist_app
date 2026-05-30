package com.soa.toursservice.repository;

import com.soa.toursservice.model.TourExecution;
import com.soa.toursservice.model.TourExecutionStatus;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface TourExecutionRepository extends JpaRepository<TourExecution, Long> {

    Optional<TourExecution> findByTouristUsernameAndStatus(
            String touristUsername,
            TourExecutionStatus status
    );

    boolean existsByTouristUsernameAndTourIdAndStatus(
            String touristUsername,
            Long tourId,
            TourExecutionStatus status
    );
}