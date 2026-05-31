package com.soa.toursservice.repository;

import com.soa.toursservice.model.TourExecutionKeyPoint;
import org.springframework.data.jpa.repository.JpaRepository;

public interface TourExecutionKeyPointRepository extends JpaRepository<TourExecutionKeyPoint, Long> {

    boolean existsByTourExecutionIdAndKeyPointId(Long tourExecutionId, Long keyPointId);
}