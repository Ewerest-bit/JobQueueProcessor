using System;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Workers
{
	public class JobWorker : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<JobWorker> _logger;
		private readonly BooleanGenerator _booleanGenerator = new BooleanGenerator();
		private readonly WorkerSettings _settings;

		public JobWorker(IServiceScopeFactory scopeFactory, ILogger<JobWorker> logger, IOptions<WorkerSettings> options)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
			_settings = options.Value;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try{
					using (var scope = _scopeFactory.CreateScope())
					{
						var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

						var jobs = await jobRepository.GetAllPendingJobAsync(_settings.BatchSize);

                        foreach (var job in jobs)
						{
							try
							{
								_logger.LogInformation("Processing job {JobId}", job.Id);
								job.Status = JobStatus.Running;
								await jobRepository.UpdateJobAsync(job);

								await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
								if (_booleanGenerator.IsFailed())
                                {
									job.Status = JobStatus.Failed;
									job.Result = "Simulated failure";
								}
								else {
									job.Status = JobStatus.Completed;
								}
								job.CompletedAt = DateTime.UtcNow;
								await jobRepository.UpdateJobAsync(job);
                                _logger.LogInformation("Job {JobId} finished with status {Status}", job.Id, job.Status);
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
                    await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
                }
			}
		}
	}

	public class BooleanGenerator
	{
		Random rnd;

		public BooleanGenerator()
		{
			rnd = new Random();
		}

		public bool IsFailed()
		{
			return rnd.Next(1, 101) <= 20;
		}
	}

}