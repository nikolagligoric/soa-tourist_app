package com.soa.toursservice.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;

@Entity
public class KeyPoint {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String name;
    private String description;

    private double latitude;
    private double longitude;

    private String imageUrl;

    @ManyToOne
    @JoinColumn(name = "tour_id", nullable = false)
    @JsonIgnore
    private Tour tour;

    public KeyPoint() {
    }

    public KeyPoint(String name, String description, double latitude, double longitude, String imageUrl, Tour tour) {
        this.name = name;
        this.description = description;
        this.latitude = latitude;
        this.longitude = longitude;
        this.imageUrl = imageUrl;
        this.tour = tour;
    }

    public Long getId() {
        return id;
    }

    public String getName() {
        return name;
    }

    public String getDescription() {
        return description;
    }

    public double getLatitude() {
        return latitude;
    }

    public double getLongitude() {
        return longitude;
    }

    public String getImageUrl() {
        return imageUrl;
    }

    public Tour getTour() {
        return tour;
    }

    public void setId(Long id) {
        this.id = id;
    }

    public void setName(String name) {
        this.name = name;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public void setLatitude(double latitude) {
        this.latitude = latitude;
    }

    public void setLongitude(double longitude) {
        this.longitude = longitude;
    }

    public void setImageUrl(String imageUrl) {
        this.imageUrl = imageUrl;
    }

    public void setTour(Tour tour) {
        this.tour = tour;
    }
}