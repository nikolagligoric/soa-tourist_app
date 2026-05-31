package com.soa.toursservice.dto;

public class CreateTourBlogRequest {

    private Long tourId;
    private String tourName;
    private String tourDescription;
    private String authorUsername;

    public CreateTourBlogRequest() {
    }

    public CreateTourBlogRequest(Long tourId,
                                 String tourName,
                                 String tourDescription,
                                 String authorUsername) {
        this.tourId = tourId;
        this.tourName = tourName;
        this.tourDescription = tourDescription;
        this.authorUsername = authorUsername;
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
}
