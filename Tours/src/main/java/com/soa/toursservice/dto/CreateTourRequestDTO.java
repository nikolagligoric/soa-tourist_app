package com.soa.toursservice.dto;

import com.soa.toursservice.model.TourDifficulty;

public class CreateTourRequestDTO {

    private String name;

    private String description;

    private TourDifficulty difficulty;

    private String tags;

    private int availableSlots;

    private String authorUsername;

    public CreateTourRequestDTO() {
    }

    public String getName() {
        return name;
    }

    public String getDescription() {
        return description;
    }

    public TourDifficulty getDifficulty() {
        return difficulty;
    }

    public String getTags() {
        return tags;
    }

    public int getAvailableSlots() {
        return availableSlots;
    }

    public String getAuthorUsername() {
        return authorUsername;
    }

    public void setName(String name) {
        this.name = name;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public void setDifficulty(TourDifficulty difficulty) {
        this.difficulty = difficulty;
    }

    public void setTags(String tags) {
        this.tags = tags;
    }

    public void setAvailableSlots(int availableSlots) {
        this.availableSlots = availableSlots;
    }

    public void setAuthorUsername(String authorUsername) {
        this.authorUsername = authorUsername;
    }
}
