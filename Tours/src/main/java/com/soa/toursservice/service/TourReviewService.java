package com.soa.toursservice.service;

import com.soa.toursservice.dto.CreateTourReviewRequest;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourReview;
import com.soa.toursservice.repository.TourRepository;
import com.soa.toursservice.repository.TourReviewRepository;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.util.List;

@Service
public class TourReviewService {

    private final TourReviewRepository tourReviewRepository;
    private final TourRepository tourRepository;

    public TourReviewService(TourReviewRepository tourReviewRepository,
                             TourRepository tourRepository) {
        this.tourReviewRepository = tourReviewRepository;
        this.tourRepository = tourRepository;
    }

    public TourReview createReview(Long tourId,
                                   CreateTourReviewRequest request,
                                   String touristUsername) {

        if (request.getRating() < 1 || request.getRating() > 5) {
            throw new IllegalArgumentException("Rating must be between 1 and 5.");
        }

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found."));

        TourReview review = new TourReview();
        review.setTour(tour);
        review.setRating(request.getRating());
        review.setComment(request.getComment());
        review.setTouristUsername(touristUsername);
        review.setVisitDate(request.getVisitDate());
        review.setCommentDate(LocalDate.now());
        review.setImageUrls(request.getImageUrls());

        return tourReviewRepository.save(review);
    }

    public List<TourReview> getReviewsForTour(Long tourId) {
        return tourReviewRepository.findByTourId(tourId);
    }
    
    public void deleteReview(Long reviewId, String username) {

        TourReview review = tourReviewRepository.findById(reviewId)
                .orElseThrow(() -> new RuntimeException("Review not found."));

        if (!review.getTouristUsername().equals(username)) {
            throw new RuntimeException("You can only delete your own reviews.");
        }

        review.getImageUrls().clear();

        tourReviewRepository.save(review);

        tourReviewRepository.delete(review);
    }
}