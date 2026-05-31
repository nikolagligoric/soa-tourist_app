package com.soa.toursservice.client;

import com.soa.toursservice.grpc.purchase.HasPurchasedTourRequest;
import com.soa.toursservice.grpc.purchase.PurchaseRpcServiceGrpc;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

@Component
public class PurchaseClient {

    private final PurchaseRpcServiceGrpc.PurchaseRpcServiceBlockingStub purchaseStub;

    public PurchaseClient(
            @Value("${purchase.grpc.host:purchase}") String purchaseGrpcHost,
            @Value("${purchase.grpc.port:8080}") int purchaseGrpcPort
    ) {
        ManagedChannel channel = ManagedChannelBuilder
                .forAddress(purchaseGrpcHost, purchaseGrpcPort)
                .usePlaintext()
                .build();

        this.purchaseStub = PurchaseRpcServiceGrpc.newBlockingStub(channel);
    }

    public boolean hasPurchased(Long tourId, String username) {
        HasPurchasedTourRequest request = HasPurchasedTourRequest.newBuilder()
                .setUsername(username)
                .setTourId(tourId)
                .build();

        return purchaseStub.hasPurchasedTour(request).getHasPurchased();
    }
}