FROM golang:1.26 AS build
WORKDIR /src

COPY . .

RUN go mod download
RUN go build -o followers-be .

FROM debian:bookworm-slim AS final
WORKDIR /app

COPY --from=build /src/followers-be .

EXPOSE 8082

CMD ["./followers-be"]