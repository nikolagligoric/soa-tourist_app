package com.soa.toursservice.dto;

import com.soa.toursservice.model.KeyPoint;
import com.soa.toursservice.model.TourDuration;
import com.soa.toursservice.model.TourReview;

import java.util.List;

public class TourDetailsDTO {

    private Long id;

    private String name;

    private String description;

    private double price;

    private double distanceInKm;

    private List<TourDuration> durations;

    private List<TourReview> reviews;

    private List<KeyPoint> keyPoints;

    public TourDetailsDTO() {
    }

    public Long getId() {
        return id;
    }

    public void setId(Long id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public double getDistanceInKm() {
        return distanceInKm;
    }

    public void setDistanceInKm(double distanceInKm) {
        this.distanceInKm = distanceInKm;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getDescription() {
        return description;
    }

    public double getPrice() {
        return price;
    }

    public List<TourDuration> getDurations() {
        return durations;
    }

    public List<TourReview> getReviews() {
        return reviews;
    }

    public List<KeyPoint> getKeyPoints() {
        return keyPoints;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public void setPrice(double price) {
        this.price = price;
    }

    public void setDurations(List<TourDuration> durations) {
        this.durations = durations;
    }

    public void setReviews(List<TourReview> reviews) {
        this.reviews = reviews;
    }

    public void setKeyPoints(List<KeyPoint> keyPoints) {
        this.keyPoints = keyPoints;
    }
}