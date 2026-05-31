package com.soa.toursservice.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import jakarta.persistence.*;

import java.time.LocalDateTime;

@Entity
public class TourExecutionKeyPoint {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private Long keyPointId;

    private LocalDateTime reachedAt;

    @ManyToOne
    @JoinColumn(name = "tour_execution_id")
    @JsonIgnore
    private TourExecution tourExecution;

    public TourExecutionKeyPoint() {
    }

    public Long getId() {
        return id;
    }

    public Long getKeyPointId() {
        return keyPointId;
    }

    public void setKeyPointId(Long keyPointId) {
        this.keyPointId = keyPointId;
    }

    public LocalDateTime getReachedAt() {
        return reachedAt;
    }

    public void setReachedAt(LocalDateTime reachedAt) {
        this.reachedAt = reachedAt;
    }

    public TourExecution getTourExecution() {
        return tourExecution;
    }

    public void setTourExecution(TourExecution tourExecution) {
        this.tourExecution = tourExecution;
    }
}