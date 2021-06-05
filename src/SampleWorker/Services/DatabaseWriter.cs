using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using SampleWorker.Models;

namespace SampleWorker.Services
{
    public static class DatabaseWriter
    {
        public static ActivitySource source = new ActivitySource("Database");

        public async static Task InsertUserAsync(User user)
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnection"));
            connection.Open();

            await Task.Delay(100);
            using var activityUser = source.StartActivity($"InsertUser-{user.Name}");
            {
                //activityUser.SetParentId(Worker.importerActivity.ParentId);
                user.CurrentVote.IdUser = await connection.QueryFirstAsync<int>(@"INSERT INTO [dbo].[User] ([Name] ,[Surname]) VALUES(@Name, @Surname) 
                select SCOPE_IDENTITY()", new { user.Name, user.Surname });
                activityUser.AddTag("IdUser", user.CurrentVote.IdUser);
                activityUser.Stop();
            }

            using var activityVote = source.StartActivity($"Vote-{user.CurrentVote.VoteValue}");
            {
                //activityVote.SetParentId(Worker.importerActivity.ParentId);
                await connection.ExecuteAsync(@"INSERT INTO Vote(VoteValue, Date, IdUser) values(@VoteValue, @Date, @IdUser) 
                select SCOPE_IDENTITY()", new { user.CurrentVote.VoteValue, user.CurrentVote.Date, user.CurrentVote.IdUser });
                activityVote.Stop();
            }
        }
    }
}
