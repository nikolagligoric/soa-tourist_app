package main

import (
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
func main() {
	config.ConnectNeo4j()

	router := gin.Default()

	router.Use(cors.New(cors.Config{
		AllowOrigins:     []string{"http://localhost:4200"}, // Dozvoli tvoj Angular front
		AllowMethods:     []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowHeaders:     []string{"Origin", "Content-Type", "Authorization"}, // Obavezno Authorization zbog Bearer tokena!
		ExposeHeaders:    []string{"Content-Length"},
		AllowCredentials: true,
	}))

	router.GET("/ping", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"message": "followers service works",
		})
	})

	router.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler))

	followers := router.Group("/api/followers")
	followers.Use(middleware.AuthMiddleware())
	{
		followers.POST("/:username/follow", handlers.Follow)
		followers.DELETE("/:username/unfollow", handlers.Unfollow)
		followers.GET("/check/:username", handlers.CheckFollowing)
		followers.GET("/recommendations", handlers.GetRecommendations)
		followers.GET("/following", handlers.GetFollowing)
		followers.GET("/followers", handlers.GetFollowers)
	}

	router.Run(":8082")
}
