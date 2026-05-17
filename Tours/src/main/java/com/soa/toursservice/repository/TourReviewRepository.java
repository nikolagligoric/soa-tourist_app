package com.soa.toursservice.repository;

import com.soa.toursservice.model.TourReview;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface TourReviewRepository extends JpaRepository<TourReview, Long> {

    List<TourReview> findByTourId(Long tourId);
}