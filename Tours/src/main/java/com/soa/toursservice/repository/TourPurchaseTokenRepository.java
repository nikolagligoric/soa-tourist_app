package com.soa.toursservice.repository;

import com.soa.toursservice.model.TourPurchaseToken;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface TourPurchaseTokenRepository extends JpaRepository<TourPurchaseToken, Long> {

    List<TourPurchaseToken> findByTouristUsername(String touristUsername);

    Optional<TourPurchaseToken> findByTouristUsernameAndTourId(String touristUsername, Long tourId);

    boolean existsByTouristUsernameAndTourId(String touristUsername, Long tourId);
}