package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CreateKeyPointRequestDTO;
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
}