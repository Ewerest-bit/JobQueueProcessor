using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJobService
    {
        Task<List<JobDto>> GetAllJobsAsync();
        Task<JobDto> GetJobByIdAsync(int id);
        Task<JobDto> CreateJobAsync(CreateJobDto dto);
        Task DeleteJobAsync(int id);
        Task UpdateJobAsync(int id, UpdateJobDto dto);

    }

   
}
