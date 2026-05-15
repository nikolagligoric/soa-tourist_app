package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.service.TourService;
import org.springframework.web.bind.annotation.*;
import com.soa.toursservice.dto.CreateKeyPointRequestDTO;
import com.soa.toursservice.model.KeyPoint;
import com.soa.toursservice.security.JwtService;

import java.util.List;

@RestController
@RequestMapping("/api/tours")
public class TourController {

    private final TourService tourService;
    private final JwtService jwtService;

    public TourController(TourService tourService, JwtService jwtService) {
        this.tourService = tourService;
        this.jwtService = jwtService;
    }

    @PostMapping
    public Tour createTour(@RequestHeader("Authorization") String authorizationHeader,
            @RequestBody CreateTourRequestDTO request) {

        String token = extractToken(authorizationHeader);

        String username = jwtService.extractUsername(token);
        String role = jwtService.extractRole(token);

        if (!role.equals("Guide")) {
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
    public KeyPoint addKeyPoint(
            @PathVariable Long tourId,
            @RequestHeader("Authorization") String authorizationHeader,
            @RequestBody CreateKeyPointRequestDTO request) {

        String token = extractToken(authorizationHeader);

        String username = jwtService.extractUsername(token);
        String role = jwtService.extractRole(token);

        if (!role.equals("Guide")) {
            throw new RuntimeException("Only guides can add key points");
        }

        return tourService.addKeyPoint(tourId, username, request);
    }

    @GetMapping("/{tourId}/keypoints")
    public List<KeyPoint> getKeyPointsForTour(@PathVariable Long tourId,
            @RequestHeader("Authorization") String authorizationHeader) {

        String token = extractToken(authorizationHeader);

        String username = jwtService.extractUsername(token);
        String role = jwtService.extractRole(token);

        if (!role.equals("Guide")) {
            throw new RuntimeException("Only guides can view key points");
        }

        return tourService.getKeyPointsForTour(tourId, username);
    }

    private String extractToken(String authorizationHeader) {

        if (authorizationHeader == null || !authorizationHeader.startsWith("Bearer ")) {
            throw new RuntimeException("Missing or invalid Authorization header");
        }

        return authorizationHeader.substring(7);
    }
}