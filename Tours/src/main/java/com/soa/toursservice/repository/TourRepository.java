package com.soa.toursservice.repository;

import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface TourRepository extends JpaRepository<Tour, Long> {

    List<Tour> findByAuthorUsername(String authorUsername);
    List<Tour> findByStatus(TourStatus status);
}