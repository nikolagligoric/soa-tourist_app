package com.soa.toursservice.dto;

import com.soa.toursservice.model.TourDifficulty;

public class CreateTourRequestDTO {

    private String name;

    private String description;

    private TourDifficulty difficulty;

    private String tags;

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

    public void setAuthorUsername(String authorUsername) {
        this.authorUsername = authorUsername;
    }
}