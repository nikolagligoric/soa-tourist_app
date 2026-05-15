package com.soa.toursservice.controller;

import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.service.TourService;
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
    public Tour createTour(@RequestBody CreateTourRequestDTO request) {
        return tourService.createTour(request);
    }

    @GetMapping("/author/{authorUsername}")
    public List<Tour> getToursByAuthor(@PathVariable String authorUsername) {
        return tourService.getToursByAuthor(authorUsername);
    }
}