package com.soa.toursservice.dto;

public class TourPublishCommand {

    private Long tourId;
    private String tourName;
    private String tourDescription;
    private String authorUsername;
    private String type;

    public TourPublishCommand() {
    }

    public TourPublishCommand(Long tourId,
                              String tourName,
                              String tourDescription,
                              String authorUsername,
                              String type) {
        this.tourId = tourId;
        this.tourName = tourName;
        this.tourDescription = tourDescription;
        this.authorUsername = authorUsername;
        this.type = type;
    }

    public Long getTourId() {
        return tourId;
    }

    public void setTourId(Long tourId) {
        this.tourId = tourId;
    }

    public String getTourName() {
        return tourName;
    }

    public void setTourName(String tourName) {
        this.tourName = tourName;
    }

    public String getTourDescription() {
        return tourDescription;
    }

    public void setTourDescription(String tourDescription) {
        this.tourDescription = tourDescription;
    }

    public String getAuthorUsername() {
        return authorUsername;
    }

    public void setAuthorUsername(String authorUsername) {
        this.authorUsername = authorUsername;
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }
}
