package handlers

import (
	"followers-be/services"
	"net/http"

	"github.com/gin-gonic/gin"
)

// Follow godoc
// @Summary Follow user
// @Description Authenticated user follows another user
// @Tags followers
// @Security BearerAuth
// @Param username path string true "Username to follow"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /api/followers/{username}/follow [post]
func Follow(c *gin.Context) {

	followingUsername := c.Param("username")

	followerUsername := c.MustGet("username").(string)

	err := services.FollowUser(followerUsername, followingUsername)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "User followed successfully",
	})
}

// Unfollow godoc
// @Summary Unfollow user
// @Description Authenticated user unfollows another user
// @Tags followers
// @Security BearerAuth
// @Param username path string true "Username to unfollow"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /api/followers/{username}/unfollow [delete]
func Unfollow(c *gin.Context) {
	followingUsername := c.Param("username")
	followerUsername := c.MustGet("username").(string)

	err := services.UnfollowUser(followerUsername, followingUsername)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": "User unfollowed successfully",
	})
}

// CheckFollowing godoc
// @Summary Check following
// @Description Checks if authenticated user follows another user
// @Tags followers
// @Security BearerAuth
// @Param username path string true "Username to check"
// @Success 200 {object} map[string]bool
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /api/followers/check/{username} [get]
func CheckFollowing(c *gin.Context) {
	followingUsername := c.Param("username")
	followerUsername := c.MustGet("username").(string)

	isFollowing, err := services.IsFollowing(followerUsername, followingUsername)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"isFollowing": isFollowing,
	})
}

// GetRecommendations godoc
// @Summary Get recommendations
// @Description Get follower recommendations
// @Tags followers
// @Security BearerAuth
// @Success 200 {array} string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /api/followers/recommendations [get]
func GetRecommendations(c *gin.Context) {

	username := c.MustGet("username").(string)

	recommendations, err := services.GetRecommendations(username)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, recommendations)
}

// GetFollowing godoc
// @Summary Get following users
// @Description Get users that authenticated user follows
// @Tags followers
// @Security BearerAuth
// @Success 200 {array} string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Router /api/followers/following [get]
func GetFollowing(c *gin.Context) {
	username := c.MustGet("username").(string)

	following, err := services.GetFollowing(username)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{
			"error": err.Error(),
		})
		return
	}

	c.JSON(http.StatusOK, following)
}
