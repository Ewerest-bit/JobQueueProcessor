using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
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
    }
}
