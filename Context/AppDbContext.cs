using Microsoft.EntityFrameworkCore;
using TraineeAPI.Consumer.Entity;

namespace TraineeAPI.Consumer.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }

    public DbSet<ProcessingJob> ProcessingJobs { get; set; }

    public DbSet<SubmissionFile> SubmissionFiles { get; set; }

    public DbSet<Submission> Submissions { get;set;}
}