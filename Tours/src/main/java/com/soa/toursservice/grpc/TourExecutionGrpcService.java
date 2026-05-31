package com.soa.toursservice.grpc;

import com.soa.toursservice.model.TourExecution;
import com.soa.toursservice.service.TourExecutionService;
import io.grpc.stub.StreamObserver;
import org.springframework.stereotype.Service;

@Service
public class TourExecutionGrpcService extends TourExecutionRpcServiceGrpc.TourExecutionRpcServiceImplBase {

    private final TourExecutionService tourExecutionService;

    public TourExecutionGrpcService(TourExecutionService tourExecutionService) {
        this.tourExecutionService = tourExecutionService;
    }

    @Override
    public void startTour(
            StartTourRequest request,
            StreamObserver<StartTourResponse> responseObserver) {

        TourExecution execution = tourExecutionService.startTour(
                request.getTourId(),
                request.getUsername(),
                request.getToken()
        );

        StartTourResponse response =
                StartTourResponse.newBuilder()
                        .setId(execution.getId())
                        .setTourId(execution.getTourId())
                        .setTouristUsername(execution.getTouristUsername())
                        .setStatus(execution.getStatus().name())
                        .setStartLatitude(execution.getStartLatitude())
                        .setStartLongitude(execution.getStartLongitude())
                        .setStartedAt(execution.getStartedAt().toString())
                        .setLastActivityAt(execution.getLastActivityAt().toString())
                        .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }
}