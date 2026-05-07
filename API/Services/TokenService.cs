using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using API.Entities;
using API.Interfaces;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    /// <summary>
    ///  根據提供的 AppUser 生成 JWT Token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string CreateToken(AppUser user)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor tokenDescriptor = CreateTokenDescriptor(user);
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

    /// <summary>
    ///  創建簽名憑證
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private SigningCredentials CreateSigningCredentials()
    {
        // 從配置中獲取 TokenKey，並檢查其是否存在和長度是否足夠
        string tokenKey = config["TokenKey"] ?? throw new Exception("TokenKey is not configured");
        if (tokenKey.Length < 64) throw new Exception("TokenKey must be at least 64 characters long");

        // 將 TokenKey 轉換為對稱安全密鑰
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenKey));

        // 使用 HMAC SHA512 演算法創建簽名憑證
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);

        return creds;
    }

    /// <summary>
    /// 根據提供的 AppUser 創建一組聲明
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private static List<Claim> CreateClaims(AppUser user)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        ];

        return claims;
    }

    /// <summary>
    /// 根據提供的 AppUser 創建 SecurityTokenDescriptor
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private SecurityTokenDescriptor CreateTokenDescriptor(AppUser user)
    {
        SigningCredentials creds = CreateSigningCredentials();
        List<Claim> claims = CreateClaims(user);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        return tokenDescriptor;
    }
}
