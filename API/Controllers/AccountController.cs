using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController(
        [FromServices] AppDbContext context,
        [FromServices] ITokenService tokenService)
        : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            ActionResult<UserDto> result;

            if (await EmailExists(registerDto.Email))
            {
                result = BadRequest("Email is already taken");
            }
            else
            {
                AppUser user = await context.AddUserAsync(registerDto);
                UserDto userDto = user.ToDto(tokenService);

                result = Ok(userDto);
            }

            return result;
        }

        /// <summary>
        /// 檢查 Email 是否已存在
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<bool> EmailExists(string email)
        {
            // 從資料庫中檢查是否存在與提供的 Email 匹配的用戶，忽略大小寫
            bool emailExists = await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());

            return emailExists;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            ActionResult<UserDto> result;

            // 從資料庫中根據提供的 Email 查找用戶
            AppUser? user = await context.Users.SingleOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                result = Unauthorized("Invalid email or password");
            }
            else if (!VerifyPasswordAsync(user, loginDto.Password))
            {
                result = Unauthorized("Invalid email or password");
            }
            else
            {
                UserDto userDto = user.ToDto(tokenService);

                result = Ok(userDto);
            }

            return result;
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static bool VerifyPasswordAsync(AppUser user, string password)
        {
            // 使用 HMACSHA512 來驗證密碼，使用用戶的 PasswordSalt 作為密鑰
            using HMACSHA512 hmac = new(user.PasswordSalt);

            // 計算提供的密碼的哈希值
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // 比較計算出的哈希值與用戶存儲的 PasswordHash 是否匹配
            bool passwordMatches = computedHash.SequenceEqual(user.PasswordHash);

            return passwordMatches;
        }
    }
}
