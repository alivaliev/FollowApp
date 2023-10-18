using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApi.Data;
using WebApi.Models;
using WebApi.Utility;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {


        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserController(AppDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        [HttpPost("follow/{userIdToFollow}")]
        public async Task<IActionResult> FollowUser(string userIdToFollow)
        {
            try
            {
                string userId = CurrentUser.FindUserId(HttpContext);
                var currentUser = _context.Users.FirstOrDefault(x => x.Id == userId).Id;
                if (currentUser == userIdToFollow)
                    return BadRequest("You can't follow yourself.");

                var followExists = _context.UserFollows.Where(uf =>
                    uf.FollowerUserId == currentUser && uf.FollowedUserId == userIdToFollow);

                if (followExists.Any())
                    return BadRequest("You are already following this user.");


                UserFollow userFollow = new UserFollow
                {
                    FollowerUserId = currentUser,
                    FollowedUserId = userIdToFollow,
                    IsAccepted = false
                };

                _context.UserFollows.Add(userFollow);
                await _context.SaveChangesAsync();

                return Ok("Follow request sent.");
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpPost("unfollow/{userIdToUnfollow}")]

        public async Task<IActionResult> UnfollowUser(string userIdToUnfollow)
        {
            try
            {


                string userId = CurrentUser.FindUserId(HttpContext);
                var currentUser = _context.Users.FirstOrDefault(x => x.Id == userId);
                var userFollow = await _context.UserFollows.FirstOrDefaultAsync(uf =>
                    uf.FollowerUserId == currentUser.Id && uf.FollowedUserId == userIdToUnfollow);

                if (userFollow == null)
                {
                    return BadRequest("You are not following this user.");
                }

                _context.UserFollows.Remove(userFollow);
                await _context.SaveChangesAsync();


                return Ok("Unfollowed successfully.");
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while processing your request.");
            }
        }


        [HttpPost("acceptfollow/{followerUserId}")]
        public async Task<IActionResult> AcceptFollowRequest(string followerUserId)
        {
            try
            {


                string userId = CurrentUser.FindUserId(HttpContext);
                var currentUser = _context.Users.FirstOrDefault(x => x.Id == userId).Id;
                var followRequest = _context.UserFollows.FirstOrDefault(uf =>
                    uf.FollowerUserId == followerUserId && uf.FollowedUserId == currentUser && !uf.IsAccepted);

                if (followRequest == null)
                {
                    return BadRequest("No pending follow request found from this user.");
                }

                followRequest.IsAccepted = true;
                await _context.SaveChangesAsync();

                return Ok("Follow request accepted.");

            }
            catch (Exception)
            {

                return BadRequest("An error occurred while processing your request.");
            }
        }


        [HttpGet("followrequests")]

        public IActionResult GetFollowRequests()
        {

            try
            {


                string userId = CurrentUser.FindUserId(HttpContext);
                var currentUser = _context.Users.FirstOrDefault(x => x.Id == userId).Id;
                // Find follow requests that are not accepted
                var pendingFollowRequests = _context.UserFollows
                    .Where(uf => uf.FollowedUserId == currentUser && !uf.IsAccepted)
                    .Select(uf => uf.Follower) // Select the users who sent the requests
                    .Select(x => new { Id = x.Id, UserName = x.UserName })
                    .ToList();

                return Ok(pendingFollowRequests);
            }
            catch (Exception)
            {

                return BadRequest("An error occurred while processing your request.");
            }
        }
    }
}
