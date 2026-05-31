package com.soa.toursservice.messaging;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.soa.toursservice.dto.TourSlotCommand;
import com.soa.toursservice.dto.TourSlotReply;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.TourRepository;
import io.nats.client.Connection;
import io.nats.client.Dispatcher;
import io.nats.client.Message;
import io.nats.client.Nats;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.nio.charset.StandardCharsets;

@Component
public class TourSlotCommandSubscriber {

    private final ObjectMapper objectMapper = new ObjectMapper();
    private final TourRepository tourRepository;
    private final String natsUrl;
    private final String commandSubject;
    private Connection connection;

    public TourSlotCommandSubscriber(TourRepository tourRepository,
                                     @Value("${nats.url}") String natsUrl,
                                     @Value("${nats.tour-slots-command-subject}") String commandSubject) {
        this.tourRepository = tourRepository;
        this.natsUrl = natsUrl;
        this.commandSubject = commandSubject;
    }

    @PostConstruct
    public void subscribe() {
        try {
            connection = Nats.connect(natsUrl);
            Dispatcher dispatcher = connection.createDispatcher(this::handleCommand);
            dispatcher.subscribe(commandSubject);
        } catch (Exception ex) {
            throw new RuntimeException("Could not subscribe to tour slot commands.", ex);
        }
    }

    private void handleCommand(Message message) {
        TourSlotReply reply;

        try {
            TourSlotCommand command = objectMapper.readValue(
                    new String(message.getData(), StandardCharsets.UTF_8),
                    TourSlotCommand.class
            );

            if ("ReserveTourSlot".equals(command.getType())) {
                reserveSlot(command.getTourId());
                reply = new TourSlotReply(command.getTourId(), "TourSlotReserved", null);
            } else if ("RollbackTourSlotReservation".equals(command.getType())) {
                rollbackSlot(command.getTourId());
                reply = new TourSlotReply(command.getTourId(), "TourSlotReservationRolledBack", null);
            } else {
                reply = new TourSlotReply(command.getTourId(), "TourSlotNotReserved", "Unknown command type");
            }
        } catch (Exception ex) {
            reply = new TourSlotReply(null, "TourSlotNotReserved", ex.getMessage());
        }

        publishReply(message.getReplyTo(), reply);
    }

    private void reserveSlot(Long tourId) {
        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        if (tour.getStatus() != TourStatus.PUBLISHED) {
            throw new RuntimeException("Only published tours can be purchased");
        }

        if (tour.getAvailableSlots() <= 0) {
            throw new RuntimeException("No available slots for tour");
        }

        tour.setAvailableSlots(tour.getAvailableSlots() - 1);
        tourRepository.save(tour);
    }

    private void rollbackSlot(Long tourId) {
        Tour tour = tourRepository.findById(tourId)
                .orElseThrow(() -> new RuntimeException("Tour not found"));

        tour.setAvailableSlots(tour.getAvailableSlots() + 1);
        tourRepository.save(tour);
    }

    private void publishReply(String replyTo, TourSlotReply reply) {
        if (replyTo == null || replyTo.isBlank()) {
            return;
        }

        try {
            connection.publish(replyTo, objectMapper.writeValueAsBytes(reply));
        } catch (Exception ignored) {
        }
    }

    @PreDestroy
    public void close() {
        if (connection != null) {
            try {
                connection.close();
            } catch (InterruptedException ex) {
                Thread.currentThread().interrupt();
            }
        }
    }
}
