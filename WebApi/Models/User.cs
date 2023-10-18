using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{

    public class User : IdentityUser
    {
        private List<Photo> _photos = new List<Photo>();
        public IReadOnlyCollection<Photo> Photos => _photos.AsReadOnly();
        public List<UserFollow> Follower { get; set; } 
        public List<UserFollow> Followed { get; set; } 
    }
}
