using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(AppDbContext context) : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet] // GET: api/Members
        public async Task<ActionResult<IReadOnlyList<AppUser>>> GetMembers()
        {
            var members = await context.Users.ToListAsync();

            return Ok(members);
        }

        [HttpGet("{id}")] // GET: api/Members/{id}
        public async Task<ActionResult<AppUser>> GetMember(string id)
        {
            var member = await context.Users.FindAsync(id);

            if (member == null) return NotFound();

            return Ok(member);
        }
    }
}
