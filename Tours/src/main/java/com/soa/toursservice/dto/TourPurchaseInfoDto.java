package com.soa.toursservice.dto;

public class TourPurchaseInfoDto {

    private Long id;
    private String name;
    private double price;
    private String status;

    public TourPurchaseInfoDto(Long id, String name, double price, String status) {
        this.id = id;
        this.name = name;
        this.price = price;
        this.status = status;
    }

    public Long getId() {
        return id;
    }

    public String getName() {
        return name;
    }

    public double getPrice() {
        return price;
    }

    public String getStatus() {
        return status;
    }
}