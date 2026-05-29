package com.soa.toursservice.dto;

import com.soa.toursservice.model.TransportType;

public class CreateTourDurationRequestDTO {

    private TransportType transportType;

    private int durationMinutes;

    public CreateTourDurationRequestDTO() {
    }

    public TransportType getTransportType() {
        return transportType;
    }

    public void setTransportType(TransportType transportType) {
        this.transportType = transportType;
    }

    public int getDurationMinutes() {
        return durationMinutes;
    }

    public void setDurationMinutes(int durationMinutes) {
        this.durationMinutes = durationMinutes;
    }
}