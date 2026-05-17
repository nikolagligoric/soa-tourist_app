package com.soa.toursservice.controller;

import com.soa.toursservice.dto.SetTouristLocationDTO;
import com.soa.toursservice.model.TouristLocation;
import com.soa.toursservice.service.TouristLocationService;

import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/location")
public class TouristLocationController {

    private final TouristLocationService touristLocationService;

    public TouristLocationController(TouristLocationService touristLocationService) {
        this.touristLocationService = touristLocationService;
    }

    @PostMapping
    public TouristLocation setLocation(Authentication authentication,
                                       @RequestBody SetTouristLocationDTO request) {
        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can set location");
        }

        return touristLocationService.setLocation(username, request);
    }

    @GetMapping
    public TouristLocation getLocation(Authentication authentication) {
        Jwt jwt = (Jwt) authentication.getPrincipal();
        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can get location");
        }

        return touristLocationService.getLocation(username);
    }
}