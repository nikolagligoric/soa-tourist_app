package com.soa.toursservice.service;

import com.soa.toursservice.client.PurchaseClient;
import com.soa.toursservice.dto.CheckTourExecutionLocationResponseDTO;
import com.soa.toursservice.model.*;
import com.soa.toursservice.repository.KeyPointRepository;
import com.soa.toursservice.repository.TourExecutionKeyPointRepository;
import com.soa.toursservice.repository.TourExecutionRepository;
import com.soa.toursservice.repository.TourRepository;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;

@Service
public class TourExecutionService {

    private static final double KEY_POINT_RADIUS_IN_KM = 0.1; // 100 metara

    private final TourRepository tourRepository;
    private final KeyPointRepository keyPointRepository;
    private final TourExecutionRepository tourExecutionRepository;
    private final TourExecutionKeyPointRepository tourExecutionKeyPointRepository;
    private final TouristLocationService touristLocationService;
    private final PurchaseClient purchaseClient;

    public TourExecutionService(TourRepository tourRepository,
                                KeyPointRepository keyPointRepository,
                                TourExecutionRepository tourExecutionRepository,
                                TourExecutionKeyPointRepository tourExecutionKeyPointRepository,
                                TouristLocationService touristLocationService,
                                PurchaseClient purchaseClient) {
        this.tourRepository = tourRepository;
        this.keyPointRepository = keyPointRepository;
        this.tourExecutionRepository = tourExecutionRepository;
        this.tourExecutionKeyPointRepository = tourExecutionKeyPointRepository;
        this.touristLocationService = touristLocationService;
        this.purchaseClient = purchaseClient;
    }

    public TourExecution startTour(Long tourId, String touristUsername, String token) {
        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (tour.getStatus() != TourStatus.PUBLISHED && tour.getStatus() != TourStatus.ARCHIVED) {
            throw new RuntimeException("Only published or archived tours can be started");
        }

        boolean purchased = purchaseClient.hasPurchased(tourId, touristUsername);

        if (!purchased) {
            throw new RuntimeException("You must purchase the tour before starting it");
        }

        boolean alreadyActive =
                tourExecutionRepository
                        .existsByTouristUsernameAndStatus(
                                touristUsername,
                                TourExecutionStatus.ACTIVE
                        );

        if (alreadyActive) {
            throw new RuntimeException("You already have an active tour");
        }

        TouristLocation location = touristLocationService.getLocation(touristUsername);

        if (location == null) {
            throw new RuntimeException("Tourist location is not set");
        }

        LocalDateTime now = LocalDateTime.now();

        TourExecution execution = new TourExecution();
        execution.setTourId(tourId);
        execution.setTouristUsername(touristUsername);
        execution.setStatus(TourExecutionStatus.ACTIVE);
        execution.setStartedAt(now);
        execution.setLastActivityAt(now);
        execution.setStartLatitude(location.getLatitude());
        execution.setStartLongitude(location.getLongitude());

        return tourExecutionRepository.save(execution);
    }

    public CheckTourExecutionLocationResponseDTO checkLocation(Long executionId, String touristUsername) {
        TourExecution execution = tourExecutionRepository.findById(executionId)
                .orElseThrow(() -> new RuntimeException("Tour execution not found"));

        if (!execution.getTouristUsername().equals(touristUsername)) {
            throw new RuntimeException("You can check only your own tour execution");
        }

        if (execution.getStatus() != TourExecutionStatus.ACTIVE) {
            throw new RuntimeException("Tour execution is not active");
        }

        TouristLocation location = touristLocationService.getLocation(touristUsername);

        if (location == null) {
            throw new RuntimeException("Tourist location is not set");
        }

        List<KeyPoint> keyPoints = keyPointRepository
                .findByTourIdOrderBySequenceAsc(execution.getTourId());

        execution.setLastActivityAt(LocalDateTime.now());

        for (KeyPoint keyPoint : keyPoints) {
            boolean alreadyReached = tourExecutionKeyPointRepository
                    .existsByTourExecutionIdAndKeyPointId(execution.getId(), keyPoint.getId());

            if (alreadyReached) {
                continue;
            }

            double distance = calculateDistance(
                    location.getLatitude(),
                    location.getLongitude(),
                    keyPoint.getLatitude(),
                    keyPoint.getLongitude()
            );

            if (distance <= KEY_POINT_RADIUS_IN_KM) {
                TourExecutionKeyPoint reachedKeyPoint = new TourExecutionKeyPoint();
                reachedKeyPoint.setKeyPointId(keyPoint.getId());
                reachedKeyPoint.setReachedAt(LocalDateTime.now());
                reachedKeyPoint.setTourExecution(execution);

                execution.getCompletedKeyPoints().add(reachedKeyPoint);

                tourExecutionRepository.save(execution);

                return new CheckTourExecutionLocationResponseDTO(
                        true,
                        keyPoint.getId(),
                        keyPoint.getName(),
                        "Key point reached"
                );
            }
        }

        tourExecutionRepository.save(execution);

        return new CheckTourExecutionLocationResponseDTO(
                false,
                null,
                null,
                "No key point reached"
        );
    }

    public TourExecution completeTour(Long executionId, String touristUsername) {
        TourExecution execution = getActiveExecutionForUser(executionId, touristUsername);

        execution.setStatus(TourExecutionStatus.COMPLETED);
        execution.setCompletedAt(LocalDateTime.now());
        execution.setLastActivityAt(LocalDateTime.now());

        return tourExecutionRepository.save(execution);
    }

    public TourExecution abandonTour(Long executionId, String touristUsername) {
        TourExecution execution = getActiveExecutionForUser(executionId, touristUsername);

        execution.setStatus(TourExecutionStatus.ABANDONED);
        execution.setAbandonedAt(LocalDateTime.now());
        execution.setLastActivityAt(LocalDateTime.now());

        return tourExecutionRepository.save(execution);
    }

    public TourExecution getActiveExecution(String touristUsername) {
        return tourExecutionRepository
                .findByTouristUsernameAndStatus(touristUsername, TourExecutionStatus.ACTIVE)
                .orElse(null);
    }

    private TourExecution getActiveExecutionForUser(Long executionId, String touristUsername) {
        TourExecution execution = tourExecutionRepository.findById(executionId)
                .orElseThrow(() -> new RuntimeException("Tour execution not found"));

        if (!execution.getTouristUsername().equals(touristUsername)) {
            throw new RuntimeException("You can update only your own tour execution");
        }

        if (execution.getStatus() != TourExecutionStatus.ACTIVE) {
            throw new RuntimeException("Tour execution is not active");
        }

        return execution;
    }

    private double calculateDistance(double lat1, double lon1,
                                     double lat2, double lon2) {
        final int R = 6371;

        double latDistance = Math.toRadians(lat2 - lat1);
        double lonDistance = Math.toRadians(lon2 - lon1);

        double a =
                Math.sin(latDistance / 2) * Math.sin(latDistance / 2)
                        + Math.cos(Math.toRadians(lat1))
                        * Math.cos(Math.toRadians(lat2))
                        * Math.sin(lonDistance / 2)
                        * Math.sin(lonDistance / 2);

        double c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

        return R * c;
    }
}