using System;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Workers
{
	public class JobWorker : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<JobWorker> _logger;
		private const int BatchSize = 5;
		private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

		public JobWorker(IServiceScopeFactory scopeFactory, ILogger<JobWorker> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try{
					using (var scope = _scopeFactory.CreateScope())
					{
						var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

						var jobs = await jobRepository.GetAllPendingJobAsync(BatchSize);

						foreach (var job in jobs)
						{
							try
							{
								_logger.LogInformation("Processing job {JobId}", job.Id);
								job.Status = JobStatus.Running;
								await jobRepository.UpdateJobAsync(job);

								await Task.Delay(Interval, stoppingToken);
								job.Status = JobStatus.Completed;
								job.CompletedAt = DateTime.UtcNow;
								await jobRepository.UpdateJobAsync(job);
									_logger.LogInformation("Job {JobId} completed", job.Id); 
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "Failed to process job {JobId}", job.Id);
							}
						}

					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to fetch pending jobs batch");
				}
				finally
				{
                    await Task.Delay(Interval, stoppingToken);
                }
			}
		}
	}
}