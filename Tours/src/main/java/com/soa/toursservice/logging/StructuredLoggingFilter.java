package com.soa.toursservice.logging;

import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;
import java.time.Instant;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.UUID;

@Component
public class StructuredLoggingFilter extends OncePerRequestFilter {

    private final ObjectMapper objectMapper = new ObjectMapper();

    @Override
    protected void doFilterInternal(
            HttpServletRequest request,
            HttpServletResponse response,
            FilterChain filterChain)
            throws ServletException, IOException {

        long startTime = System.nanoTime();
        Instant requestStartedAt = Instant.now();

        String correlationId =
                request.getHeader("X-Correlation-ID");

        if (correlationId == null || correlationId.isBlank()) {
            correlationId = UUID.randomUUID().toString();
        }

        request.setAttribute(
                "correlationId",
                correlationId);

        response.setHeader(
                "X-Correlation-ID",
                correlationId);

        Exception caughtException = null;

        try {
            filterChain.doFilter(request, response);
        }
        catch (ServletException | IOException ex) {
            caughtException = ex;
            throw ex;
        }
        catch (RuntimeException ex) {
            caughtException = ex;
            throw ex;
        }
        finally {
            double latencyMs =
                    (System.nanoTime() - startTime) / 1_000_000.0;

            int statusCode = caughtException != null
                    ? HttpServletResponse.SC_INTERNAL_SERVER_ERROR
                    : response.getStatus();

            String level;

            if (caughtException != null || statusCode >= 500) {
                level = "ERROR";
            } else if (statusCode >= 400) {
                level = "WARNING";
            } else {
                level = "INFO";
            }

            String exception = null;

            if (caughtException != null) {
                exception =
                        caughtException.getClass().getSimpleName()
                                + ": "
                                + caughtException.getMessage();
            }

            Map<String, Object> logEntry =
                    new LinkedHashMap<>();

            logEntry.put(
                    "timestamp",
                    requestStartedAt.toString());

            logEntry.put(
                    "serviceName",
                    "Tours");

            logEntry.put(
                    "level",
                    level);

            logEntry.put(
                    "correlationId",
                    correlationId);

            logEntry.put(
                    "method",
                    request.getMethod());

            logEntry.put(
                    "path",
                    request.getRequestURI());

            logEntry.put(
                    "statusCode",
                    statusCode);

            logEntry.put(
                    "latencyMs",
                    Math.round(latencyMs * 1000.0) / 1000.0);

            logEntry.put(
                    "message",
                    "HTTP request");

            logEntry.put(
                    "exception",
                    exception);

            System.out.println(
                    objectMapper.writeValueAsString(logEntry));
        }
    }
}