package com.soa.toursservice.service;

import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.TourRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class TourService {

    private final TourRepository tourRepository;

    public TourService(TourRepository tourRepository) {
        this.tourRepository = tourRepository;
    }

    public Tour createTour(CreateTourRequestDTO request) {

        Tour tour = new Tour();

        tour.setName(request.getName());
        tour.setDescription(request.getDescription());
        tour.setDifficulty(request.getDifficulty());
        tour.setTags(request.getTags());
        tour.setAuthorUsername(request.getAuthorUsername());

        tour.setStatus(TourStatus.DRAFT);
        tour.setPrice(0);

        return tourRepository.save(tour);
    }

    public List<Tour> getToursByAuthor(String authorUsername) {
        return tourRepository.findByAuthorUsername(authorUsername);
    }
}