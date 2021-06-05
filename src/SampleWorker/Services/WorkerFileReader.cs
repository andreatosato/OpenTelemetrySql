using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SampleWorker.Models;

namespace SampleWorker.Services
{
    public static class WorkerFileReader
    {
        private static readonly ActivitySource source = new("Reader");
        public static async IAsyncEnumerable<User> ReadData(string filePath)
        {
            string[] lines;
            using var activityRead = source.StartActivity("ReadLines");
            {
                //activityRead.SetParentId(Worker.importerActivity.ParentId);
                lines = await System.IO.File.ReadAllLinesAsync(filePath);
                activityRead.Stop();
            }
            foreach (string line in lines)
            {
                await Task.Delay(100);
                using var activitySplit = source.StartActivity("Split");
                {
                    //activitySplit.SetParentId(Worker.importerActivity.ParentId);
                    activitySplit.AddTag("rowData", line);

                    var userSplit = line.Split(";");
                    yield return new()
                    {
                        Name = userSplit[0],
                        Surname = userSplit[1],
                        CurrentVote = new Vote()
                        {
                            VoteValue = int.Parse(userSplit[2]),
                            Date = DateTime.Parse(userSplit[3])
                        }
                    };
                    activitySplit.Stop();
                }
            }
        }
    }
}
