using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public JobStatus Status { get; set; }
        public JobType Type { get; set; }
        public string Payload { get; set; } = string.Empty;
        public string? Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
