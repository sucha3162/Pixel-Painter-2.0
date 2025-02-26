using Microsoft.AspNetCore.Mvc;
using MyTestVueApp.Server.Entities;
using Google.Apis.Oauth2.v2.Data;

namespace MyTestVueApp.Server.Interfaces
{
    public interface ILoginService
    {
        public Task<Userinfo> GetUserId(string code);
        public Task<Artist> SignupActions(string subId, string email);
        public Task<bool> UpdateUsername(string newUsername, string subId);
        public Task<Artist> GetUserBySubId(string SubId);
        Task DeleteArtist(int artistId);
        public Task<bool> IsUserAdmin(string userId);

        public IEnumerable<Artist> GetAllArtists();
    }
}
