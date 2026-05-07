using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using API.Entities;

namespace API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<AppUser> Users { get; set; }

    /// <summary>
    /// 註冊一個新用戶
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    public async Task<AppUser> AddUserAsync(RegisterDto registerDto)
    {
        // 使用 HMACSHA512 來創建新用戶的密碼哈希和鹽值
        using HMACSHA512 hmac = new();

        // 將註冊資訊填入
        AppUser user = new()
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        // 將新用戶添加到資料庫
        Users.Add(user);

        // 保存更改到資料庫
        await SaveChangesAsync();

        return user;
    }
}
