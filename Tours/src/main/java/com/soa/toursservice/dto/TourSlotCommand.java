package com.soa.toursservice.dto;

public class TourSlotCommand {

    private Long tourId;
    private String touristUsername;
    private String type;

    public TourSlotCommand() {
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

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }
}
