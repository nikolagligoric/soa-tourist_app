package com.soa.toursservice.service;

import com.soa.toursservice.dto.CreateTourRequestDTO;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.KeyPointRepository;
import com.soa.toursservice.repository.TourRepository;
import org.springframework.stereotype.Service;
import com.soa.toursservice.dto.CreateKeyPointRequestDTO;
import com.soa.toursservice.model.KeyPoint;
import java.util.Optional;

import java.util.List;

@Service
public class TourService {

    private final TourRepository tourRepository;
    private final KeyPointRepository keyPointRepository;

    public TourService(TourRepository tourRepository, KeyPointRepository keyPointRepository) {
        this.tourRepository = tourRepository;
        this.keyPointRepository = keyPointRepository;
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

    public KeyPoint addKeyPoint(Long tourId, CreateKeyPointRequestDTO request) {

        Optional<Tour> optionalTour = tourRepository.findById(tourId);

        if (optionalTour.isEmpty()) {
            throw new RuntimeException("Tour not found");
        }

        Tour tour = optionalTour.get();

        KeyPoint keyPoint = new KeyPoint();

        keyPoint.setName(request.getName());
        keyPoint.setDescription(request.getDescription());
        keyPoint.setLatitude(request.getLatitude());
        keyPoint.setLongitude(request.getLongitude());
        keyPoint.setImageUrl(request.getImageUrl());

        keyPoint.setTour(tour);

        return keyPointRepository.save(keyPoint);
    }

    public List<KeyPoint> getKeyPointsForTour(Long tourId) {
        return keyPointRepository.findByTourId(tourId);
    }
}