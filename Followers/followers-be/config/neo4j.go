package config

import (
	"context"
	"log"
	"os"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

var Driver neo4j.DriverWithContext

func ConnectNeo4j() {
	uri := os.Getenv("NEO4J_URI")
	username := os.Getenv("NEO4J_USERNAME")
	password := os.Getenv("NEO4J_PASSWORD")

	if uri == "" {
		uri = "neo4j://localhost:7687"
	}
	if username == "" {
		username = "neo4j"
	}
	if password == "" {
		password = "password"
	}

	var err error

	Driver, err = neo4j.NewDriverWithContext(
		uri,
		neo4j.BasicAuth(username, password, ""),
	)

	if err != nil {
		log.Fatalf("Failed to create Neo4j driver: %v", err)
	}

	err = Driver.VerifyConnectivity(context.Background())

	if err != nil {
		log.Fatalf("Neo4j not reachable: %v", err)
	}

	log.Println("Connected to Neo4j")
}
