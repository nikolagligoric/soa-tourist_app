package com.soa.toursservice.controller;

import com.soa.toursservice.model.ShoppingCart;
import com.soa.toursservice.service.ShoppingCartService;

import org.springframework.security.core.Authentication;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;
import com.soa.toursservice.model.TourPurchaseToken;

import java.util.List;

@RestController
@RequestMapping("/api/cart")
public class ShoppingCartController {

    private final ShoppingCartService shoppingCartService;

    public ShoppingCartController(ShoppingCartService shoppingCartService) {
        this.shoppingCartService = shoppingCartService;
    }

    @PostMapping("/add/{tourId}")
    public ShoppingCart addTourToCart(@PathVariable Long tourId,
                                      Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can use shopping cart");
        }

        return shoppingCartService.addTourToCart(username, tourId);
    }

    @GetMapping
    public ShoppingCart getCart(Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can view shopping cart");
        }

        return shoppingCartService.getCart(username);
    }

    @DeleteMapping("/items/{itemId}")
    public ShoppingCart removeItemFromCart(@PathVariable Long itemId,
                                           Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can remove items from shopping cart");
        }

        return shoppingCartService.removeItemFromCart(username, itemId);
    }

    @PostMapping("/checkout")
    public List<TourPurchaseToken> checkout(Authentication authentication) {

        Jwt jwt = (Jwt) authentication.getPrincipal();

        String username = jwt.getClaim("username");
        String role = jwt.getClaim("role");

        if (!"Tourist".equals(role)) {
            throw new RuntimeException("Only tourists can checkout");
        }

        return shoppingCartService.checkout(username);
    }
}