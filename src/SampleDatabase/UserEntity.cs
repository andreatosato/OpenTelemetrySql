using System.Collections.Generic;

namespace SampleDatabase
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public HashSet<PostUserEntity> PostUsers { get; set; }
    }
}
