using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SampleWorker.Services;

namespace SampleWorker
{
    public partial class Worker : BackgroundService
    {
        public static ActivitySource activitySource = new ActivitySource("SampleWorker");
        public static Activity importerActivity;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            importerActivity = activitySource.StartActivity("Importer");
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "workerfile.txt");
                List<Task> userTasks = new List<Task>();
                await foreach (var u in WorkerFileReader.ReadData(filePath))
                {
                    await DatabaseWriter.InsertUserAsync(u);
                }
            }

            await Task.CompletedTask;
        }
    }
}
