package main

import (
	"followers-be/config"
	"followers-be/handlers"
	"followers-be/middleware"

	_ "followers-be/docs"

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
	}

	router.Run(":8082")
}
