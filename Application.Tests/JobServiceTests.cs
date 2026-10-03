using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests
{
    public class JobServiceTests
    {
        [Fact]
        public async Task GetJobByIdAsync_JobDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();
            jobRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Job?)null);
            var jobService = new JobService(jobRepositoryMock.Object);
            //Act
            //Assert

            await Assert.ThrowsAsync<NotFoundException>(async () => await jobService.GetJobByIdAsync(999));
        }
        [Fact]
        public async Task GetJobByIdAsync_JobExists_ReturnJob()
        {
            //Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var existingJob = new Job
            {
                Id = 1,
                Status = JobStatus.Pending,
                Type = JobType.SendEmail,
                Payload = "Test Payload",
                CreatedAt = DateTime.UtcNow,
            };
            jobRepositoryMock.Setup(repo => repo.GetByIdAsync(existingJob.Id))
                .ReturnsAsync(existingJob);
            var jobService = new JobService(jobRepositoryMock.Object);

            //Act
            var result = await jobService.GetJobByIdAsync(existingJob.Id);
            //Assert
            Assert.Equal(existingJob.Id, result.Id);
            Assert.Equal(existingJob.Status, result.Status);
            Assert.Equal(existingJob.Payload, result.Payload);
        }

        [Fact]
        public async Task DeleteJobAsync_JobDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            jobRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Job?)null);

            var jobService = new JobService(jobRepositoryMock.Object);

            //Act&Assert
            await Assert.ThrowsAsync<NotFoundException>(() => jobService.DeleteJobAsync(999));
        }
        [Fact]
        public async Task UpdateJobAsync_JobDoesNotExist_ThrowsNotFoundException()
        {
            //Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();
            var dto = new UpdateJobDto { Payload = "New Payload" };
            jobRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Job?)null);

            var jobService = new JobService(jobRepositoryMock.Object);
            //Act&Assert
            await Assert.ThrowsAsync<NotFoundException>(() => jobService.UpdateJobAsync(999, dto));
        }
        [Fact]
        public async Task UpdateJobAsync_JobIsNotPending_ThrowsInvalidOperationException()
        {
            //Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var existingJob = new Job
            {
                Id = 1,
                Status = JobStatus.Running,
                Type = JobType.SendEmail,
                Payload = "Test Payload",
                CreatedAt = DateTime.UtcNow,
            };
            var dto = new UpdateJobDto { Payload = "New Payload" };

            jobRepositoryMock.Setup(repo => repo.GetByIdAsync(existingJob.Id))
                .ReturnsAsync(existingJob);


            var jobService = new JobService(jobRepositoryMock.Object);

            //Act&Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => jobService.UpdateJobAsync(existingJob.Id, dto));
        }
        [Fact]
        public async Task UpdateJobAsync_JobIsPending_UpdatesPayload()
        {
            // Arrange
            var jobRepositoryMock = new Mock<IJobRepository>();

            var existingJob = new Job
            {
                Id = 1,
                Status = JobStatus.Pending,        
                Type = JobType.SendEmail,
                Payload = "Old Payload",
                CreatedAt = DateTime.UtcNow,
            };

            jobRepositoryMock
                .Setup(repo => repo.GetByIdAsync(existingJob.Id))
                .ReturnsAsync(existingJob);

            jobRepositoryMock
                .Setup(repo => repo.UpdateJobAsync(It.IsAny<Job>()))
                .Returns(Task.CompletedTask);

            var dto = new UpdateJobDto { Payload = "New Payload" };
            var jobService = new JobService(jobRepositoryMock.Object);

            // Act
            await jobService.UpdateJobAsync(existingJob.Id, dto);

            // Assert
            Assert.Equal("New Payload", existingJob.Payload);   

            jobRepositoryMock.Verify(
                repo => repo.UpdateJobAsync(existingJob),
                Times.Once);                                     
        }
        [Fact]
        public async Task CreateJobAsync_ValidDto_CreatesJobWithPendingStatus()
        {
            //Arrange

            var jobRepositoryMock = new Mock<IJobRepository>();

            Job? capturedJob = null;
            jobRepositoryMock.Setup(repo => repo.CreateJobAsync(It.IsAny<Job>())).Callback<Job>(j => capturedJob = j).Returns(Task.CompletedTask);

            var dto = new CreateJobDto
            {
                Type = JobType.SendEmail,
                Payload = "Hello"
            };

            var jobService = new JobService(jobRepositoryMock.Object);

            //Act
            var result = await jobService.CreateJobAsync(dto);

            //Assert

            Assert.NotNull(capturedJob);
            Assert.Equal(JobStatus.Pending, capturedJob!.Status);   
            Assert.Equal(JobType.SendEmail, capturedJob.Type);
            Assert.Equal("Hello", capturedJob.Payload);

            Assert.Equal(JobStatus.Pending, result.Status);
            Assert.Equal(JobType.SendEmail, result.Type);
            Assert.Equal("Hello", result.Payload);

        }
    }
}
