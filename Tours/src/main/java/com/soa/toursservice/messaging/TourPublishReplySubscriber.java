package com.soa.toursservice.messaging;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.soa.toursservice.dto.TourPublishReply;
import com.soa.toursservice.model.Tour;
import com.soa.toursservice.model.TourStatus;
import com.soa.toursservice.repository.TourRepository;
import io.nats.client.Connection;
import io.nats.client.Dispatcher;
import io.nats.client.Nats;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;

@Component
public class TourPublishReplySubscriber {

    private final ObjectMapper objectMapper = new ObjectMapper();
    private final TourRepository tourRepository;
    private final String natsUrl;
    private final String replySubject;
    private Connection connection;

    public TourPublishReplySubscriber(TourRepository tourRepository,
                                      @Value("${nats.url}") String natsUrl,
                                      @Value("${nats.tour-publish-reply-subject}") String replySubject) {
        this.tourRepository = tourRepository;
        this.natsUrl = natsUrl;
        this.replySubject = replySubject;
    }

    @PostConstruct
    public void subscribe() {
        try {
            connection = Nats.connect(natsUrl);
            Dispatcher dispatcher = connection.createDispatcher(message -> handleReply(message.getData()));
            dispatcher.subscribe(replySubject);
        } catch (Exception ex) {
            throw new RuntimeException("Could not subscribe to tour publish replies.", ex);
        }
    }

    private void handleReply(byte[] payload) {
        try {
            TourPublishReply reply = objectMapper.readValue(
                    new String(payload, StandardCharsets.UTF_8),
                    TourPublishReply.class
            );

            Tour tour = tourRepository.findById(reply.getTourId()).orElse(null);

            if (tour == null || tour.getStatus() != TourStatus.PUBLISHING) {
                return;
            }

            if ("BlogCreated".equals(reply.getType())) {
                tour.setStatus(TourStatus.PUBLISHED);
                tour.setPublishedAt(LocalDateTime.now());
            } else {
                tour.setStatus(TourStatus.DRAFT);
                tour.setPublishedAt(null);
            }

            tourRepository.save(tour);
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
