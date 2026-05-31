package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CheckTourExecutionLocationResponseDTO;
import com.soa.toursservice.model.TourExecution;
import com.soa.toursservice.service.TourExecutionService;
import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/tour-executions")
public class TourExecutionController {

    private final TourExecutionService tourExecutionService;

    public TourExecutionController(TourExecutionService tourExecutionService) {
        this.tourExecutionService = tourExecutionService;
    }

    @PostMapping("/{tourId}/start")
    public TourExecution startTour(
            @PathVariable Long tourId,
            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can start tours");
        }

        String token = jwt.getTokenValue();

        return tourExecutionService.startTour(
                tourId,
                username,
                token
        );
    }

    @PostMapping("/{executionId}/check-location")
    public CheckTourExecutionLocationResponseDTO checkLocation(
            @PathVariable Long executionId,
            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can check locations");
        }

        return tourExecutionService.checkLocation(
                executionId,
                username
        );
    }

    @PostMapping("/{executionId}/complete")
    public TourExecution completeTour(
            @PathVariable Long executionId,
            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can complete tours");
        }

        return tourExecutionService.completeTour(
                executionId,
                username
        );
    }

    @PostMapping("/{executionId}/abandon")
    public TourExecution abandonTour(
            @PathVariable Long executionId,
            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can abandon tours");
        }

        return tourExecutionService.abandonTour(
                executionId,
                username
        );
    }

    @GetMapping("/active")
    public TourExecution getActiveExecution(
            Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can view active execution");
        }

        return tourExecutionService.getActiveExecution(username);
    }
}