package com.soa.toursservice.client;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.*;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestTemplate;

@Component
public class PurchaseClient {

    private final RestTemplate restTemplate;
    private final String purchaseServiceUrl;

    public PurchaseClient(RestTemplate restTemplate,
                          @Value("${purchase.service.url}") String purchaseServiceUrl) {
        this.restTemplate = restTemplate;
        this.purchaseServiceUrl = purchaseServiceUrl;
    }

    public boolean hasPurchased(Long tourId, String token) {
        HttpHeaders headers = new HttpHeaders();
        headers.setBearerAuth(token);

        HttpEntity<Void> entity = new HttpEntity<>(headers);

        ResponseEntity<Boolean> response = restTemplate.exchange(
                purchaseServiceUrl + "/api/cart/has-purchased/" + tourId,
                HttpMethod.GET,
                entity,
                Boolean.class
        );

        return Boolean.TRUE.equals(response.getBody());
    }
}