namespace WebApi.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
