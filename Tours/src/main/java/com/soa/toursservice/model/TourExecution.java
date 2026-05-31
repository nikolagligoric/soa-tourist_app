package com.soa.toursservice.model;

import jakarta.persistence.*;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

@Entity
public class TourExecution {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private Long tourId;

    private String touristUsername;

    @Enumerated(EnumType.STRING)
    private TourExecutionStatus status;

    private LocalDateTime startedAt;

    private LocalDateTime completedAt;

    private LocalDateTime abandonedAt;

    private LocalDateTime lastActivityAt;

    private double startLatitude;

    private double startLongitude;

    @OneToMany(mappedBy = "tourExecution",
            cascade = CascadeType.ALL,
            orphanRemoval = true)
    private List<TourExecutionKeyPoint> completedKeyPoints = new ArrayList<>();

    public TourExecution() {
    }

    public Long getId() {
        return id;
    }

    public Long getTourId() {
        return tourId;
    }

    public void setTourId(Long tourId) {
        this.tourId = tourId;
    }

    public String getTouristUsername() {
        return touristUsername;
    }

    public void setTouristUsername(String touristUsername) {
        this.touristUsername = touristUsername;
    }

    public TourExecutionStatus getStatus() {
        return status;
    }

    public void setStatus(TourExecutionStatus status) {
        this.status = status;
    }

    public LocalDateTime getStartedAt() {
        return startedAt;
    }

    public void setStartedAt(LocalDateTime startedAt) {
        this.startedAt = startedAt;
    }

    public LocalDateTime getCompletedAt() {
        return completedAt;
    }

    public void setCompletedAt(LocalDateTime completedAt) {
        this.completedAt = completedAt;
    }

    public LocalDateTime getAbandonedAt() {
        return abandonedAt;
    }

    public void setAbandonedAt(LocalDateTime abandonedAt) {
        this.abandonedAt = abandonedAt;
    }

    public LocalDateTime getLastActivityAt() {
        return lastActivityAt;
    }

    public void setLastActivityAt(LocalDateTime lastActivityAt) {
        this.lastActivityAt = lastActivityAt;
    }

    public double getStartLatitude() {
        return startLatitude;
    }

    public void setStartLatitude(double startLatitude) {
        this.startLatitude = startLatitude;
    }

    public double getStartLongitude() {
        return startLongitude;
    }

    public void setStartLongitude(double startLongitude) {
        this.startLongitude = startLongitude;
    }

    public List<TourExecutionKeyPoint> getCompletedKeyPoints() {
        return completedKeyPoints;
    }

    public void setCompletedKeyPoints(List<TourExecutionKeyPoint> completedKeyPoints) {
        this.completedKeyPoints = completedKeyPoints;
    }
}