using System.Collections.Generic;
using Models.Enums;

namespace Models.Entities
{
    public class UserGroup
    {
        public int Id { get; set; }
        public UserGroupCode Code { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
