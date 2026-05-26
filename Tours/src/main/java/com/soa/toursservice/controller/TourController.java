package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CreateKeyPointRequestDTO;
import com.soa.toursservice.dto.CreateTourDurationRequestDTO;
import com.soa.toursservice.model.TourDuration;
import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.dto.CreateTourReviewRequest;
import com.soa.toursservice.model.KeyPoint;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourReview;
import com.soa.toursservice.service.TourReviewService;
import com.soa.toursservice.service.TourService;

import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;
import com.soa.toursservice.dto.TourPreviewDTO;
import java.util.List;

@RestController
@RequestMapping("/api/tours")
public class TourController {

    private final TourService tourService;
    private final TourReviewService tourReviewService;

    public TourController(TourService tourService,
                          TourReviewService tourReviewService) {
        this.tourService = tourService;
        this.tourReviewService = tourReviewService;
    }

    @PostMapping
    public Tour createTour(Authentication authentication,
                           @RequestBody CreateTourRequestDTO request) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can create tours");
        }

        request.setAuthorUsername(username);

        return tourService.createTour(request);
    }

    @GetMapping("/author/{authorUsername}")
    public List<Tour> getToursByAuthor(@PathVariable String authorUsername) {
        return tourService.getToursByAuthor(authorUsername);
    }

    @PostMapping("/{tourId}/keypoints")
    public KeyPoint addKeyPoint(@PathVariable Long tourId,
                                Authentication authentication,
                                @RequestBody CreateKeyPointRequestDTO request) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can add key points");
        }

        return tourService.addKeyPoint(tourId, username, request);
    }

    @GetMapping("/{tourId}/keypoints")
    public List<KeyPoint> getKeyPointsForTour(@PathVariable Long tourId,
                                              Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can view key points");
        }

        return tourService.getKeyPointsForTour(tourId, username);
    }

    @PostMapping("/{tourId}/reviews")
    public TourReview createReview(@PathVariable Long tourId,
                                   Authentication authentication,
                                   @RequestBody CreateTourReviewRequest request) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can leave reviews");
        }

        return tourReviewService.createReview(tourId, request, username);
    }

    @GetMapping("/{tourId}/reviews")
    public List<TourReview> getReviewsForTour(@PathVariable Long tourId) {
        return tourReviewService.getReviewsForTour(tourId);
    }
    
    @DeleteMapping("/{tourId}/reviews/{reviewId}")
    public String deleteReview(@PathVariable Long tourId,
                               @PathVariable Long reviewId,
                               Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can delete reviews");
        }

        tourReviewService.deleteReview(reviewId, username);

        return "Review deleted successfully.";
    }
    
    

    @PutMapping("/{tourId}/keypoints/{keyPointId}")
    public KeyPoint updateKeyPoint(@PathVariable Long tourId,
                                   @PathVariable Long keyPointId,
                                   Authentication authentication,
                                   @RequestBody CreateKeyPointRequestDTO request) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can update key points");
        }

        return tourService.updateKeyPoint(tourId, keyPointId, username, request);
    }

    @DeleteMapping("/{tourId}/keypoints/{keyPointId}")
    public void deleteKeyPoint(@PathVariable Long tourId,
                               @PathVariable Long keyPointId,
                               Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can delete key points");
        }

        tourService.deleteKeyPoint(tourId, keyPointId, username);
    }
    @PostMapping("/{tourId}/durations")
    public TourDuration addDuration(@PathVariable Long tourId,
                                    Authentication authentication,
                                    @RequestBody CreateTourDurationRequestDTO request) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can add durations");
        }

        return tourService.addTourDuration(tourId, username, request);
    }
    @PutMapping("/{tourId}/publish")
    public Tour publishTour(@PathVariable Long tourId,
                            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can publish tours");
        }

        return tourService.publishTour(tourId, username);
    }
    
    @PutMapping("/{tourId}/archive")
    public Tour archiveTour(@PathVariable Long tourId,
                            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can archive tours");
        }

        return tourService.archiveTour(tourId, username);
    }
    
    @PutMapping("/{tourId}/reactivate")
    public Tour reactivateTour(@PathVariable Long tourId,
                               Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Guide".equals(role)) {
            throw new RuntimeException("Only guides can reactivate tours");
        }

        return tourService.reactivateTour(tourId, username);
    }
    
    @GetMapping("/published")
    public List<TourPreviewDTO> getPublishedToursForTourists(Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can view published tours");
        }

        return tourService.getPublishedToursForTourists();
    }
}