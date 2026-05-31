package com.soa.toursservice.client;

import com.soa.toursservice.dto.CreateTourBlogRequest;
import com.soa.toursservice.dto.TourBlogCreatedResponse;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestTemplate;

@Component
public class BlogClient {

    private final RestTemplate restTemplate;
    private final String blogServiceUrl;

    public BlogClient(RestTemplate restTemplate,
                      @Value("${blog.service.url}") String blogServiceUrl) {
        this.restTemplate = restTemplate;
        this.blogServiceUrl = blogServiceUrl;
    }

    public TourBlogCreatedResponse createTourAnnouncementBlog(CreateTourBlogRequest request) {
        return restTemplate.postForObject(
                blogServiceUrl + "/api/blogs/tour-announcement",
                request,
                TourBlogCreatedResponse.class
        );
    }
}
