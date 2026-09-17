package com.soa.toursservice.dto;

public class CheckTourExecutionLocationResponseDTO {

    private boolean keyPointReached;
    private Long keyPointId;
    private String keyPointName;
    private String message;

    public CheckTourExecutionLocationResponseDTO() {
    }

    public CheckTourExecutionLocationResponseDTO(boolean keyPointReached,
                                                 Long keyPointId,
                                                 String keyPointName,
                                                 String message) {
        this.keyPointReached = keyPointReached;
        this.keyPointId = keyPointId;
        this.keyPointName = keyPointName;
        this.message = message;
    }

    public boolean isKeyPointReached() {
        return keyPointReached;
    }

    public void setKeyPointReached(boolean keyPointReached) {
        this.keyPointReached = keyPointReached;
    }

    public Long getKeyPointId() {
        return keyPointId;
    }

    public void setKeyPointId(Long keyPointId) {
        this.keyPointId = keyPointId;
    }

    public String getKeyPointName() {
        return keyPointName;
    }

    public void setKeyPointName(String keyPointName) {
        this.keyPointName = keyPointName;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }
}