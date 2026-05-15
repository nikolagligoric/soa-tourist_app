package com.soa.toursservice.repository;

import com.soa.toursservice.model.KeyPoint;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface KeyPointRepository extends JpaRepository<KeyPoint, Long> {

    List<KeyPoint> findByTourId(Long tourId);
}