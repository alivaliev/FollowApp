using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Utility;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public PhotoController(AppDbContext context, UserManager<User> userManager, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile photoFile)
        {
            try
            {


                if (photoFile == null || photoFile.Length == 0)
                    return BadRequest("Invalid file.");

                string userId = CurrentUser.FindUserId(HttpContext);
                string currentUser = _context.Users.FirstOrDefault(x => x.Id == userId).Id;

                var photo = new Photo
                {
                    PhotoUrl = await Upload(photoFile),
                    UserId = currentUser
                };

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return Ok("Photo uploaded successfully.");
            }
            catch (Exception)
            {

                return BadRequest("An error occurred while processing your request.");
            }
        }




        [HttpGet("feed")]
        [ProducesResponseType(typeof(List<UserInfoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotoFeed()
        {
            try
            {
                string userId = CurrentUser.FindUserId(HttpContext);
                string currentUser = _context.Users.FirstOrDefault(x => x.Id == userId).Id;
                var followers = GetFollowerIds(currentUser);
                var photos = GetPhotos(followers);
                return Ok(photos);
            }
            catch (Exception)
            {

                return BadRequest("An error occurred while processing your request.");
            }
        }

        #region write 

        private async Task<string> Upload(IFormFile photoFile)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + photoFile.FileName;
            var photoStorageSettings = _config["PhotoStorageSettings:LocalPath"];
            string filePath = Path.Combine(photoStorageSettings, uniqueFileName); // Replace with your URL logic
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(fileStream);
            }
            string photoUrl = "photos/" + uniqueFileName;
            return photoUrl;
        }

        #endregion

        #region read



        private List<string> GetFollowerIds(string userId)
        {
            var followerIds = _context.UserFollows
              .Where(uf => uf.FollowerUserId == userId && uf.IsAccepted)
              .Select(uf => uf.FollowedUserId)
              .ToList();
            followerIds.Add(userId);
            return followerIds;
        }

        private List<UserInfoDto> GetPhotos(List<string> userIds)
        {
            var photos = _context.Photos
               .Include(x => x.User)
               .Where(p => userIds.Contains(p.UserId))
               .Select(x => new UserInfoDto
               {
                   UserName = x.User.UserName,
                   PhotoUrl = x.PhotoUrl
               })
               .ToList();
            return photos;

        }

        #endregion

    }
}
