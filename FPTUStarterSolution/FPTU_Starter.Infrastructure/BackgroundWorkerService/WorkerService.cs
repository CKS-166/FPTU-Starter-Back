using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Domain.Entity;
using FPTU_Starter.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FPTU_Starter.Domain.Enum.ProjectEnum;

namespace FPTU_Starter.Infrastructure.BackgroundWorkerService
{
    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;
        private  MyDbContext _myDb = new MyDbContext();

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Project> projects = _myDb.Projects.ToList();
                foreach (var project in projects)
                {
                    DateTime today = DateTime.Today;
                    if (project.StartDate == today)
                    {
                        if (project.ProjectStatus == ProjectStatus.Pending)
                        {
                            project.ProjectStatus = ProjectStatus.Failed;
                        }
                    }
                }
                _myDb.SaveChanges();    
                _logger.LogInformation("Hello World at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken); // Chờ 10 giây trước khi thực hiện lại
            }
        }
    }
}
