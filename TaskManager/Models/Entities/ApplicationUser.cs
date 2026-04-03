using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TaskManager.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}