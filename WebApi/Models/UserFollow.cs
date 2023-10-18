namespace WebApi.Models
{
    public class UserFollow
    {
        public int Id { get; set; }
        public string FollowerUserId { get; set; }
        public string FollowedUserId { get; set; }
        public bool IsAccepted { get; set; }

        public User Follower { get; set; }
        public User Followed { get; set; }
    }
}
