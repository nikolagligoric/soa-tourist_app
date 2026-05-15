package services

import (
	"context"
	"errors"
	"followers-be/config"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

func FollowUser(followerUsername string, followingUsername string) error {
	if followerUsername == followingUsername {
		return errors.New("You cannot follow yourself")
	}

	ctx := context.Background()

	session := config.Driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	_, err := session.ExecuteWrite(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		query := `
			MERGE (follower:User {username: $followerUsername})
			MERGE (following:User {username: $followingUsername})
			MERGE (follower)-[:FOLLOWS]->(following)
			RETURN follower, following
		`

		params := map[string]any{
			"followerUsername":  followerUsername,
			"followingUsername": followingUsername,
		}

		result, err := tx.Run(ctx, query, params)

		if err != nil {
			return nil, err
		}

		return result.Consume(ctx)
	})

	return err
}

func UnfollowUser(followerUsername string, followingUsername string) error {
	ctx := context.Background()

	session := config.Driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	_, err := session.ExecuteWrite(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		query := `
			MATCH (follower:User {username: $followerUsername})-[r:FOLLOWS]->(following:User {username: $followingUsername})
			DELETE r
		`

		params := map[string]any{
			"followerUsername":  followerUsername,
			"followingUsername": followingUsername,
		}

		result, err := tx.Run(ctx, query, params)

		if err != nil {
			return nil, err
		}

		return result.Consume(ctx)
	})

	return err
}

func IsFollowing(followerUsername string, followingUsername string) (bool, error) {
	ctx := context.Background()

	session := config.Driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	result, err := session.ExecuteRead(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		query := `
			MATCH (follower:User {username: $followerUsername})-[r:FOLLOWS]->(following:User {username: $followingUsername})
			RETURN count(r) > 0 AS isFollowing
		`

		params := map[string]any{
			"followerUsername":  followerUsername,
			"followingUsername": followingUsername,
		}

		result, err := tx.Run(ctx, query, params)
		if err != nil {
			return false, err
		}

		record, err := result.Single(ctx)
		if err != nil {
			return false, err
		}

		value, _ := record.Get("isFollowing")
		return value.(bool), nil
	})

	if err != nil {
		return false, err
	}

	return result.(bool), nil
}

func GetRecommendations(username string) ([]string, error) {
	ctx := context.Background()

	session := config.Driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	result, err := session.ExecuteRead(ctx, func(tx neo4j.ManagedTransaction) (any, error) {

		query := `
			MATCH (u:User {username: $username})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(recommended:User)
			WHERE NOT (u)-[:FOLLOWS]->(recommended)
			AND u.username <> recommended.username
			RETURN DISTINCT recommended.username AS username
		`

		params := map[string]any{
			"username": username,
		}

		rows, err := tx.Run(ctx, query, params)

		if err != nil {
			return nil, err
		}

		var recommendations []string

		for rows.Next(ctx) {
			record := rows.Record()

			value, _ := record.Get("username")

			recommendations = append(recommendations, value.(string))
		}

		return recommendations, nil
	})

	if err != nil {
		return nil, err
	}

	return result.([]string), nil
}

func GetFollowing(username string) ([]string, error) {
	ctx := context.Background()

	session := config.Driver.NewSession(ctx, neo4j.SessionConfig{})
	defer session.Close(ctx)

	result, err := session.ExecuteRead(ctx, func(tx neo4j.ManagedTransaction) (any, error) {
		query := `
			MATCH (u:User {username: $username})-[:FOLLOWS]->(following:User)
			RETURN following.username AS username
		`

		params := map[string]any{
			"username": username,
		}

		rows, err := tx.Run(ctx, query, params)

		if err != nil {
			return nil, err
		}

		var following []string

		for rows.Next(ctx) {
			record := rows.Record()
			value, _ := record.Get("username")
			following = append(following, value.(string))
		}

		return following, nil
	})

	if err != nil {
		return nil, err
	}

	return result.([]string), nil
}
