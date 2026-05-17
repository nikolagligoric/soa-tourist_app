package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CreateKeyPointRequestDTO;
import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.model.KeyPoint;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.service.TourService;

import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/tours")
public class TourController {

    private final TourService tourService;

    public TourController(TourService tourService) {
        this.tourService = tourService;
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