using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            // 檢查 Email 是否已存在
            if (await EmailExists(registerDto.Email)) return BadRequest("Email is already taken");

            // 使用 HMACSHA512 來加密密碼
            using var hmac = new HMACSHA512();

            // 創建新的 AppUser 實例，並將註冊資訊填入
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            // 將新用戶添加到資料庫並保存
            context.Users.Add(user);
            // 保存更改到資料庫
            await context.SaveChangesAsync();

            // 返回 UserDto，包含用戶資訊和 JWT Token
            return user.ToDto(tokenService);
        }

        /// <summary>
        /// 檢查資料庫中是否已存在相同 Email 的用戶
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<bool> EmailExists(string email)
        {
            return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            // 從資料庫中查找與提供的 Email 匹配的用戶
            var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);

            // 如果找不到用戶，返回未授權的響應
            if (user == null) return Unauthorized("Invalid email or password");

            // 使用 HMACSHA512 來驗證密碼，使用用戶的 PasswordSalt 作為密鑰
            using var hmac = new HMACSHA512(user.PasswordSalt);

            // 計算提供的密碼的哈希值
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // 比較計算出的哈希值與資料庫中存儲的 PasswordHash 是否匹配
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid email or password");
            }

            // 如果驗證成功，返回 UserDto，包含用戶資訊和 JWT Token
            return user.ToDto(tokenService);
        }
    }
}
