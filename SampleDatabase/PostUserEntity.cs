namespace SampleDatabase
{
    public class PostUserEntity
    {
        public int Id { get; set; }
        public PostEntity Post { get; set; }
        public UserEntity User { get; set; }
    }
}
