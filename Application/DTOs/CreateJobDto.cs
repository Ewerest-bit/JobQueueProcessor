using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateJobDto
    {
        public JobType Type { get; set; }
        public string Payload { get; set; } = string.Empty;
    }
}
