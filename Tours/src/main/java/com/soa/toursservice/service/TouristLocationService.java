package com.soa.toursservice.service;

import com.soa.toursservice.dto.SetTouristLocationDTO;
import com.soa.toursservice.model.TouristLocation;
import com.soa.toursservice.repository.TouristLocationRepository;
import org.springframework.stereotype.Service;

@Service
public class TouristLocationService {

    private final TouristLocationRepository touristLocationRepository;

    public TouristLocationService(TouristLocationRepository touristLocationRepository) {
        this.touristLocationRepository = touristLocationRepository;
    }

    public TouristLocation setLocation(String username, SetTouristLocationDTO request) {
        TouristLocation location = touristLocationRepository
                .findByUsername(username)
                .orElse(new TouristLocation());

        location.setUsername(username);
        location.setLatitude(request.getLatitude());
        location.setLongitude(request.getLongitude());

        return touristLocationRepository.save(location);
    }

    public TouristLocation getLocation(String username) {
        return touristLocationRepository
                .findByUsername(username)
                .orElse(null);
    }
}