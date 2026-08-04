using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JobService : IJobService
    {
        public readonly IJobRepository _jobRepository;
        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<JobDto> CreateJobAsync(CreateJobDto dto)
        {
            var job = new Job
            {
                Type = dto.Type,
                Payload = dto.Payload,
                Status = JobStatus.Pending
            };

            await _jobRepository.CreateJobAsync(job);
            return new JobDto
            {
                Id = job.Id,
                Status = job.Status,
                Payload = job.Payload,
                Type = job.Type,
                Result = job.Result,
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt
            };
        }

        public async Task DeleteJobAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                throw new NotFoundException($"Job with id {id} was not found.");
            await _jobRepository.DeleteJobAsync(id);
        }

        public async Task<List<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAllJobsAsync();
            var dtos = new List<JobDto>();
            foreach (var job in jobs)
            {
                dtos.Add( new JobDto
                {
                    Id = job.Id,
                    Type = job.Type,
                    Payload = job.Payload,
                    Status = job.Status,
                    Result = job.Result,
                    CreatedAt = job.CreatedAt,
                    CompletedAt = job.CompletedAt
                });

            }
            return dtos;
        }

        public async Task<JobDto> GetJobByIdAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                throw new NotFoundException($"Job with id {id} was not found.");
            var dto = new JobDto
            { 
                Id = job.Id,
                Type = job.Type,
                Payload = job.Payload,
                Status = job.Status,
                Result = job.Result,
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt
            };
            return dto;

        }

        public async Task UpdateJobAsync(int id, UpdateJobDto dto)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                throw new NotFoundException($"Job with id {id} was not found.");
            if (job.Status != JobStatus.Pending)
                throw new InvalidOperationException("Cannot update a job that is already running.");
            job.Payload = dto.Payload;

            await _jobRepository.UpdateJobAsync(job);
            
        }
    }
}
