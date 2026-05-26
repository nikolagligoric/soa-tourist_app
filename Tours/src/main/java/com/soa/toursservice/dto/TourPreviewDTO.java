package com.soa.toursservice.dto;

import com.soa.toursservice.model.KeyPoint;

public class TourPreviewDTO {

    private Long id;

    private String name;

    private String description;

    private double distanceInKm;

    private double price;

    private KeyPoint firstKeyPoint;

    public TourPreviewDTO() {
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

    public KeyPoint getFirstKeyPoint() {
        return firstKeyPoint;
    }

    public void setFirstKeyPoint(KeyPoint firstKeyPoint) {
        this.firstKeyPoint = firstKeyPoint;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public void setPrice(double price) {
        this.price = price;
    }
}