using DotnetExam.Data.Models.Common;

namespace DotnetExam.Data.Models;

public class Note : EntityBase
{
    public string? Message { get; set; }
}