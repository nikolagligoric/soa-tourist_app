package com.soa.toursservice;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.web.client.RestTemplate;
import com.soa.toursservice.grpc.TourExecutionGrpcService;
import io.grpc.Server;
import io.grpc.ServerBuilder;
import org.springframework.boot.CommandLineRunner;

@SpringBootApplication
public class ToursApplication {

	public static void main(String[] args) {
		SpringApplication.run(ToursApplication.class, args);
	}

	@Bean
	public RestTemplate restTemplate() {
		return new RestTemplate();
	}

	@Bean
	public CommandLineRunner grpcServerRunner(TourExecutionGrpcService tourExecutionGrpcService) {
		return args -> {
			Server server = ServerBuilder
					.forPort(9091)
					.addService(tourExecutionGrpcService)
					.build()
					.start();

			System.out.println("Tours gRPC server started on port 9091");

			Runtime.getRuntime().addShutdownHook(new Thread(server::shutdown));
		};
	}
}
