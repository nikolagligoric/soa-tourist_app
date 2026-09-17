package com.soa.toursservice.messaging;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.soa.toursservice.dto.TourPublishCommand;
import io.nats.client.Connection;
import io.nats.client.Nats;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

@Component
public class TourPublishNatsPublisher {

    private final ObjectMapper objectMapper = new ObjectMapper();
    private final String natsUrl;
    private final String commandSubject;

    public TourPublishNatsPublisher(@Value("${nats.url}") String natsUrl,
                                    @Value("${nats.tour-publish-command-subject}") String commandSubject) {
        this.natsUrl = natsUrl;
        this.commandSubject = commandSubject;
    }

    public void publishCreateBlogCommand(TourPublishCommand command) {
        try (Connection connection = Nats.connect(natsUrl)) {
            byte[] payload = objectMapper.writeValueAsBytes(command);
            connection.publish(commandSubject, payload);
            connection.flush(java.time.Duration.ofSeconds(2));
        } catch (Exception ex) {
            throw new RuntimeException("Could not publish tour blog creation command: " + ex.getMessage(), ex);
        }
    }
}
