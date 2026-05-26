package com.soa.toursservice.repository;

import com.soa.toursservice.model.TourDuration;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface TourDurationRepository extends JpaRepository<TourDuration, Long> {

    List<TourDuration> findByTourId(Long tourId);
}