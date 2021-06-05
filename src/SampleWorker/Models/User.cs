using System;
using Dapper.Contrib.Extensions;

namespace SampleWorker.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public Vote CurrentVote { get; set; }

    }

    [Table("Vote")]
    public class Vote
    {
        public int IdUser { get; set; }
        public DateTime Date { get; set; }
        public int VoteValue { get; set; }
    }
}
