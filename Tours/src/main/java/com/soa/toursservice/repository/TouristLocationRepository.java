package com.soa.toursservice.repository;

import com.soa.toursservice.model.TouristLocation;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface TouristLocationRepository extends JpaRepository<TouristLocation, Long> {
    Optional<TouristLocation> findByUsername(String username);
}