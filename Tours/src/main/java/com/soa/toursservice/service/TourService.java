package com.soa.toursservice.service;

import com.soa.toursservice.dto.*;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.KeyPointRepository;
import com.soa.toursservice.repository.TourDurationRepository;
import com.soa.toursservice.repository.TourRepository;
import org.springframework.stereotype.Service;
import com.soa.toursservice.model.KeyPoint;
import com.soa.toursservice.model.TourDuration;
import java.util.Optional;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Comparator;
import com.soa.toursservice.client.PurchaseClient;
import com.soa.toursservice.messaging.TourPublishNatsPublisher;

import java.util.List;

@Service
public class TourService {

    private final TourRepository tourRepository;
    private final KeyPointRepository keyPointRepository;
    private final TourDurationRepository tourDurationRepository;
    private final PurchaseClient purchaseClient;
    private final TourPublishNatsPublisher tourPublishNatsPublisher;

    public TourService(TourRepository tourRepository,
                       KeyPointRepository keyPointRepository,
                       TourDurationRepository tourDurationRepository,
                       PurchaseClient purchaseClient,
                       TourPublishNatsPublisher tourPublishNatsPublisher)
    {
        this.tourRepository = tourRepository;
        this.keyPointRepository = keyPointRepository;
        this.tourDurationRepository = tourDurationRepository;
        this.purchaseClient = purchaseClient;
        this.tourPublishNatsPublisher = tourPublishNatsPublisher;
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
        tour.setDistanceInKm(0);

        return tourRepository.save(tour);
    }

    public List<Tour> getToursByAuthor(String authorUsername) {
        return tourRepository.findByAuthorUsername(authorUsername);
    }

    public KeyPoint addKeyPoint(Long tourId, String username, CreateKeyPointRequestDTO request){

        Optional<Tour> optionalTour = tourRepository.findById(tourId);
        List<KeyPoint> list = keyPointRepository.findFirstByTourIdOrderBySequenceDesc(tourId);

        if (optionalTour.isEmpty()) {
            throw new RuntimeException("Tour not found");
        }

        Tour tour = optionalTour.get();
        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can add key points only to your own tours");
        }

        KeyPoint keyPoint = new KeyPoint();

        keyPoint.setName(request.getName());
        keyPoint.setDescription(request.getDescription());
        keyPoint.setLatitude(request.getLatitude());
        keyPoint.setLongitude(request.getLongitude());
        keyPoint.setImageUrl(request.getImageUrl());


        int nextSequence = list.isEmpty()
                ? 1
                : list.get(0).getSequence() + 1;
        
        keyPoint.setSequence(nextSequence);
        keyPoint.setTour(tour);

        KeyPoint savedKeyPoint = keyPointRepository.save(keyPoint);

        List<KeyPoint> allPoints =
                keyPointRepository.findByTourIdOrderBySequenceAsc(tourId);

        if(allPoints.size() > 1) {

            KeyPoint previous = allPoints.get(allPoints.size() - 2);

            double distance = calculateDistance(
                    previous.getLatitude(),
                    previous.getLongitude(),
                    savedKeyPoint.getLatitude(),
                    savedKeyPoint.getLongitude()
            );

            tour.setDistanceInKm(
                    tour.getDistanceInKm() + distance
            );

            tourRepository.save(tour);
        }

        return savedKeyPoint;
    }

    public List<KeyPoint> getKeyPointsForTour(Long tourId, String username) {

        Optional<Tour> optionalTour = tourRepository.findById(tourId);

        if (optionalTour.isEmpty()) {
            throw new RuntimeException("Tour not found");
        }

        Tour tour = optionalTour.get();

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can view key points only for your own tours");
        }

        return keyPointRepository.findByTourIdOrderBySequenceAsc(tourId);
    }
    
    public KeyPoint updateKeyPoint(Long tourId, Long keyPointId, String username, CreateKeyPointRequestDTO request) {
        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can update key points only for your own tours");
        }

        KeyPoint keyPoint = keyPointRepository.findById(keyPointId)
                .orElseThrow(() -> new RuntimeException("KeyPoint not found"));

        if (!keyPoint.getTour().getId().equals(tourId)) {
            throw new RuntimeException("KeyPoint does not belong to this tour");
        }

        keyPoint.setName(request.getName());
        keyPoint.setDescription(request.getDescription());
        keyPoint.setLatitude(request.getLatitude());
        keyPoint.setLongitude(request.getLongitude());
        keyPoint.setImageUrl(request.getImageUrl());

        return keyPointRepository.save(keyPoint);
    }

    public void deleteKeyPoint(Long tourId, Long keyPointId, String username) {
        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can delete key points only for your own tours");
        }

        KeyPoint keyPoint = keyPointRepository.findById(keyPointId)
                .orElseThrow(() -> new RuntimeException("KeyPoint not found"));

        if (!keyPoint.getTour().getId().equals(tourId)) {
            throw new RuntimeException("KeyPoint does not belong to this tour");
        }

        keyPointRepository.delete(keyPoint);
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
    
    public TourDuration addTourDuration(Long tourId,
            String username,
            CreateTourDurationRequestDTO request) {

    		Tour tour = tourRepository.findById(tourId)
    					.orElseThrow(() -> new RuntimeException("Tour not found"));

    		if (!tour.getAuthorUsername().equals(username)) {
    			throw new RuntimeException("You can add durations only to your own tours");
    		}

    		TourDuration duration = new TourDuration();

    		duration.setTransportType(request.getTransportType());
    		duration.setDurationMinutes(request.getDurationMinutes());
    		duration.setTour(tour);

    		return tourDurationRepository.save(duration);
    }
    
    public Tour publishTour(Long tourId, String username) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can publish only your own tours");
        }

        if (tour.getName() == null || tour.getName().isBlank()) {
            throw new RuntimeException("Tour name is required");
        }

        if (tour.getDescription() == null || tour.getDescription().isBlank()) {
            throw new RuntimeException("Tour description is required");
        }

        if (tour.getDifficulty() == null) {
            throw new RuntimeException("Tour difficulty is required");
        }

        if (tour.getTags() == null || tour.getTags().isBlank()) {
            throw new RuntimeException("Tour tags are required");
        }

        if (tour.getKeyPoints().size() < 2) {
            throw new RuntimeException("Tour must have at least 2 key points");
        }

        if (tour.getDurations().isEmpty()) {
            throw new RuntimeException("Tour must have at least one duration");
        }

        tour.setStatus(TourStatus.PUBLISHING);
        tour.setPublishedAt(null);

        try {
            Tour publishingTour = tourRepository.save(tour);

            tourPublishNatsPublisher.publishCreateBlogCommand(new TourPublishCommand(
                    publishingTour.getId(),
                    publishingTour.getName(),
                    publishingTour.getDescription(),
                    publishingTour.getAuthorUsername(),
                    "CreateTourBlog"
            ));

            return publishingTour;
        } catch (Exception ex) {
            tour.setStatus(TourStatus.DRAFT);
            tour.setPublishedAt(null);
            tourRepository.save(tour);

            throw new RuntimeException(
                    "Publishing tour failed because blog creation command could not be sent. Tour was returned to draft. Reason: "
                            + ex.getMessage()
            );
        }
    }
    
    public Tour archiveTour(Long tourId, String username) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can archive only your own tours");
        }

        if (tour.getStatus() != TourStatus.PUBLISHED) {
            throw new RuntimeException("Only published tours can be archived");
        }

        tour.setStatus(TourStatus.ARCHIVED);
        tour.setArchivedAt(LocalDateTime.now());

        return tourRepository.save(tour);
    }
    
    public Tour reactivateTour(Long tourId, String username) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (!tour.getAuthorUsername().equals(username)) {
            throw new RuntimeException("You can reactivate only your own tours");
        }

        if (tour.getStatus() != TourStatus.ARCHIVED) {
            throw new RuntimeException("Only archived tours can be reactivated");
        }

        tour.setStatus(TourStatus.PUBLISHED);
        tour.setArchivedAt(null);

        return tourRepository.save(tour);
    }
    
    public List<TourPreviewDTO> getPublishedToursForTourists() {

        List<Tour> publishedTours = tourRepository.findByStatus(TourStatus.PUBLISHED);

        List<TourPreviewDTO> result = new ArrayList<>();

        for (Tour tour : publishedTours) {

            TourPreviewDTO dto = new TourPreviewDTO();

            dto.setId(tour.getId());
            dto.setName(tour.getName());
            dto.setDescription(tour.getDescription());
            dto.setPrice(tour.getPrice());
            dto.setDistanceInKm(tour.getDistanceInKm());

            KeyPoint firstKeyPoint = tour.getKeyPoints()
                    .stream()
                    .sorted(Comparator.comparing(KeyPoint::getSequence))
                    .findFirst()
                    .orElse(null);

            dto.setFirstKeyPoint(firstKeyPoint);

            result.add(dto);
        }

        return result;
    }

    public TourDetailsDTO getTourDetails(Long tourId, String touristUsername, String token) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (tour.getStatus() != TourStatus.PUBLISHED) {
            throw new RuntimeException("Tour is not published");
        }

        boolean purchased = purchaseClient.hasPurchased(tourId, touristUsername);

        if (!purchased) {
            throw new RuntimeException("You must purchase the tour to see full details");
        }

        TourDetailsDTO dto = new TourDetailsDTO();

        dto.setId(tour.getId());
        dto.setName(tour.getName());
        dto.setDescription(tour.getDescription());
        dto.setPrice(tour.getPrice());
        dto.setDistanceInKm(tour.getDistanceInKm());

        dto.setDurations(tour.getDurations());
        dto.setReviews(tour.getReviews());

        List<KeyPoint> sortedKeyPoints = tour.getKeyPoints()
                .stream()
                .sorted(Comparator.comparing(KeyPoint::getSequence))
                .toList();

        dto.setKeyPoints(sortedKeyPoints);

        return dto;
    }

    public TourPurchaseInfoDto getTourPurchaseInfo(Long tourId) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        return new TourPurchaseInfoDto(
                tour.getId(),
                tour.getName(),
                tour.getPrice(),
                tour.getStatus().name()
        );
    }
}
