package com.soa.toursservice.dto;

public class TourSlotReply {

    private Long tourId;
    private String type;
    private String failureReason;

    public TourSlotReply() {
    }

    public TourSlotReply(Long tourId, String type, String failureReason) {
        this.tourId = tourId;
        this.type = type;
        this.failureReason = failureReason;
    }

    public Long getTourId() {
        return tourId;
    }

    public void setTourId(Long tourId) {
        this.tourId = tourId;
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    public String getFailureReason() {
        return failureReason;
    }

    public void setFailureReason(String failureReason) {
        this.failureReason = failureReason;
    }
}
