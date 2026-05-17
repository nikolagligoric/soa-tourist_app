package com.soa.toursservice.dto;

public class SetTouristLocationDTO {
    private double latitude;
    private double longitude;

    public SetTouristLocationDTO() {}

    public double getLatitude() { return latitude; }
    public double getLongitude() { return longitude; }
    public void setLatitude(double latitude) { this.latitude = latitude; }
    public void setLongitude(double longitude) { this.longitude = longitude; }
}