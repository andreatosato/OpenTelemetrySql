using System.Collections.Generic;

namespace SampleDatabase
{
    public class BlogEntity
    {
        public int Id { get; set; }
        public HashSet<PostUserEntity> PostUsers { get; set; }
    }
}
