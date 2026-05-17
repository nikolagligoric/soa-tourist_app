package com.soa.toursservice.repository;

import com.soa.toursservice.model.KeyPoint;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface KeyPointRepository extends JpaRepository<KeyPoint, Long> {

    List<KeyPoint> findByTourId(Long tourId);
    List<KeyPoint> findByTourIdOrderBySequenceAsc(Long tourId);
    int countByTourId(Long tourId);

    List<KeyPoint> findFirstByTourIdOrderBySequenceDesc(Long tourId);
}