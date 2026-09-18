package main

import (
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"net/http"
	"os"
	"strings"
	"time"

	"followers-be/config"
	_ "followers-be/docs"
	"followers-be/handlers"
	"followers-be/middleware"

	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"

	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"
)

// @title Followers API
// @version 1.0
// @description API for following users
// @host localhost:8082
// @BasePath /
// @securityDefinitions.apikey BearerAuth
// @in header
// @name Authorization

func generateCorrelationID() string {
	bytes := make([]byte, 16)

	_, err := rand.Read(bytes)
	if err != nil {
		return time.Now().UTC().Format("20060102150405.000000000")
	}

	return hex.EncodeToString(bytes)
}

func main() {
	config.ConnectNeo4j()

	instanceID := os.Getenv("INSTANCE_ID")

	if strings.TrimSpace(instanceID) == "" {
		instanceID = "unknown"
	}

	router := gin.New()

	// Correlation ID middleware
	router.Use(func(c *gin.Context) {
		const headerName = "X-Correlation-ID"

		correlationID := c.GetHeader(headerName)

		if strings.TrimSpace(correlationID) == "" {
			correlationID = generateCorrelationID()
		}

		c.Request.Header.Set(headerName, correlationID)
		c.Header(headerName, correlationID)
		c.Set("correlationId", correlationID)

		c.Next()
	})

	// Structured HTTP logging
	router.Use(func(c *gin.Context) {
		startTime := time.Now()

		c.Next()

		latency := time.Since(startTime)
		statusCode := c.Writer.Status()

		level := "INFO"

		if statusCode >= 500 {
			level = "ERROR"
		} else if statusCode >= 400 {
			level = "WARNING"
		}

		correlationID, _ := c.Get("correlationId")

		var logException interface{} = nil

		if exceptionValue, exists := c.Get("exception"); exists {
			logException = fmt.Sprintf("%v", exceptionValue)
		}

		logEntry := map[string]interface{}{
			"timestamp":     startTime.UTC().Format(time.RFC3339Nano),
			"serviceName":   "Followers",
			"instanceId":    instanceID,
			"level":         level,
			"correlationId": correlationID,
			"method":        c.Request.Method,
			"path":          c.Request.URL.Path,
			"statusCode":    statusCode,
			"latencyMs":     float64(latency.Microseconds()) / 1000,
			"clientIp":      c.ClientIP(),
			"message":       "HTTP request",
			"exception":     logException,
		}

		data, _ := json.Marshal(logEntry)

		println(string(data))
	})

	// Custom recovery - cuva exception u context-u
	router.Use(gin.CustomRecovery(func(
		c *gin.Context,
		recovered interface{},
	) {
		c.Set(
			"exception",
			fmt.Sprintf("%v", recovered),
		)

		c.AbortWithStatus(
			http.StatusInternalServerError,
		)
	}))

	router.Use(cors.New(cors.Config{
		AllowOrigins: []string{
			"http://localhost:4200",
		},
		AllowMethods: []string{
			"GET",
			"POST",
			"PUT",
			"DELETE",
			"OPTIONS",
		},
		AllowHeaders: []string{
			"Origin",
			"Content-Type",
			"Authorization",
			"X-Correlation-ID",
		},
		ExposeHeaders: []string{
			"Content-Length",
			"X-Correlation-ID",
		},
		AllowCredentials: true,
	}))

	router.GET("/ping", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"message": "followers service works",
		})
	})

	router.GET(
		"/swagger/*any",
		ginSwagger.WrapHandler(swaggerFiles.Handler),
	)

	followers := router.Group("/api/followers")

	followers.Use(middleware.AuthMiddleware())

	{
		followers.POST(
			"/:username/follow",
			handlers.Follow,
		)

		followers.DELETE(
			"/:username/unfollow",
			handlers.Unfollow,
		)

		followers.GET(
			"/check/:username",
			handlers.CheckFollowing,
		)

		followers.GET(
			"/recommendations",
			handlers.GetRecommendations,
		)

		followers.GET(
			"/following",
			handlers.GetFollowing,
		)

		followers.GET(
			"/followers",
			handlers.GetFollowers,
		)
	}

	router.Run(":8082")
}
