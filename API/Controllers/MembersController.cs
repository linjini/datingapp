using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Data;
using API.Entities;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(
        [FromServices] AppDbContext context)
        : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AppUser>>> GetMembers()
        {
            ActionResult<IReadOnlyList<AppUser>> result;

            // 從資料庫中獲取所有用戶
            List<AppUser> members = await context.Users.ToListAsync();

            result = Ok(members);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetMember([FromRoute] string id)
        {
            ActionResult<AppUser> result;

            // 從資料庫中根據提供的 id 查找用戶
            AppUser? member = await context.Users.FindAsync(id);

            if (member == null)
            {
                result = NotFound("Member not found");
            }
            else
            {
                result = Ok(member);
            }

            return result;
        }
    }
}
