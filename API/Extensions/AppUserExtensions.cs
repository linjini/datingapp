using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
    /// <summary>
    /// 將 AppUser 轉換為 UserDto
    /// </summary>
    /// <param name="user"></param>
    /// <param name="tokenService"></param>
    /// <returns></returns>
    public static UserDto ToDto(this AppUser user, ITokenService tokenService)
    {
        UserDto userDto = new()
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Token = tokenService.CreateToken(user)
        };

        return userDto;
    }
}
