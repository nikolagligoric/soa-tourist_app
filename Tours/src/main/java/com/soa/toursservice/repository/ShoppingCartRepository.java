package com.soa.toursservice.repository;

import com.soa.toursservice.model.ShoppingCart;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface ShoppingCartRepository extends JpaRepository<ShoppingCart, Long> {

    Optional<ShoppingCart> findByTouristUsername(String touristUsername);
}