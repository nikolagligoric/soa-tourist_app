package com.soa.toursservice.service;

import com.soa.toursservice.model.OrderItem;
import com.soa.toursservice.model.ShoppingCart;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.ShoppingCartRepository;
import com.soa.toursservice.repository.TourRepository;
import org.springframework.stereotype.Service;
import com.soa.toursservice.model.TourPurchaseToken;
import com.soa.toursservice.repository.TourPurchaseTokenRepository;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
public class ShoppingCartService {

    private final ShoppingCartRepository shoppingCartRepository;
    private final TourRepository tourRepository;
    private final TourPurchaseTokenRepository tourPurchaseTokenRepository;

    public ShoppingCartService(ShoppingCartRepository shoppingCartRepository,
                               TourRepository tourRepository, TourPurchaseTokenRepository tourPurchaseTokenRepository) {
        this.shoppingCartRepository = shoppingCartRepository;
        this.tourRepository = tourRepository;
        this.tourPurchaseTokenRepository = tourPurchaseTokenRepository;
    }

    public ShoppingCart addTourToCart(String touristUsername, Long tourId) {

        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (tour.getStatus() != TourStatus.PUBLISHED) {
            throw new RuntimeException("Only published tours can be added to cart");
        }

        ShoppingCart cart = shoppingCartRepository
                .findByTouristUsername(touristUsername)
                .orElseGet(() -> {
                    ShoppingCart newCart = new ShoppingCart();
                    newCart.setTouristUsername(touristUsername);
                    newCart.setTotalPrice(0);
                    return shoppingCartRepository.save(newCart);
                });

        boolean alreadyInCart = cart.getItems()
                .stream()
                .anyMatch(item -> item.getTourId().equals(tourId));

        if (alreadyInCart) {
            throw new RuntimeException("Tour is already in cart");
        }

        OrderItem item = new OrderItem();
        item.setTourId(tour.getId());
        item.setTourName(tour.getName());
        item.setPrice(tour.getPrice());
        item.setShoppingCart(cart);

        cart.getItems().add(item);

        recalculateTotalPrice(cart);

        return shoppingCartRepository.save(cart);
    }

    private void recalculateTotalPrice(ShoppingCart cart) {
        double total = cart.getItems()
                .stream()
                .mapToDouble(OrderItem::getPrice)
                .sum();

        cart.setTotalPrice(total);
    }

    public ShoppingCart getCart(String touristUsername) {

        return shoppingCartRepository
                .findByTouristUsername(touristUsername)
                .orElseGet(() -> {
                    ShoppingCart newCart = new ShoppingCart();
                    newCart.setTouristUsername(touristUsername);
                    newCart.setTotalPrice(0);
                    return shoppingCartRepository.save(newCart);
                });
    }

    public ShoppingCart removeItemFromCart(String touristUsername, Long itemId) {

        ShoppingCart cart = shoppingCartRepository
                .findByTouristUsername(touristUsername)
                .orElseThrow(() -> new RuntimeException("Shopping cart not found"));

        OrderItem itemToRemove = cart.getItems()
                .stream()
                .filter(item -> item.getId().equals(itemId))
                .findFirst()
                .orElseThrow(() -> new RuntimeException("Item not found in cart"));

        cart.getItems().remove(itemToRemove);

        recalculateTotalPrice(cart);

        return shoppingCartRepository.save(cart);
    }

    public List<TourPurchaseToken> checkout(String touristUsername) {

        ShoppingCart cart = shoppingCartRepository
                .findByTouristUsername(touristUsername)
                .orElseThrow(() -> new RuntimeException("Shopping cart not found"));

        if (cart.getItems().isEmpty()) {
            throw new RuntimeException("Shopping cart is empty");
        }

        List<TourPurchaseToken> tokens = new ArrayList<>();

        for (OrderItem item : cart.getItems()) {

            boolean alreadyPurchased = tourPurchaseTokenRepository
                    .existsByTouristUsernameAndTourId(touristUsername, item.getTourId());

            if (!alreadyPurchased) {
                TourPurchaseToken purchaseToken = new TourPurchaseToken();

                purchaseToken.setTouristUsername(touristUsername);
                purchaseToken.setTourId(item.getTourId());
                purchaseToken.setToken(UUID.randomUUID().toString());
                purchaseToken.setPurchasedAt(LocalDateTime.now());

                tokens.add(tourPurchaseTokenRepository.save(purchaseToken));
            }
        }

        cart.getItems().clear();
        cart.setTotalPrice(0);

        shoppingCartRepository.save(cart);

        return tokens;
    }
}