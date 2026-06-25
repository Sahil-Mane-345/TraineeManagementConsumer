using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TraineeAPI.Consumer.Context;
using TraineeAPI.Consumer.Entity;
using TraineeAPI.Consumer.Models;

namespace TraineeAPI.Consumer.Services;

public class FileProcessingService : IFileProcessingService
{

    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    public FileProcessingService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    public async Task FileProcessingAsync(SubmissionProcessingRequestDto submissionProcessingRequestDto, IReadOnlyBasicProperties basicProperties)
    {
        SubmissionFile? submissionFile = null;
        ProcessingJob? processingJob = null;

        try
        {
            
            submissionFile = await _context.SubmissionFiles.FirstOrDefaultAsync( f => f.Id == submissionProcessingRequestDto.SubmissionFileId);

            if(submissionFile == null)
            {
                throw new Exception("File Metadat Not found");
            }

            processingJob = await _context.ProcessingJobs.FirstOrDefaultAsync( p => p.CorrelationId == basicProperties.CorrelationId);

            if(processingJob == null)
            {
                throw new Exception("Processing Job data not found");
            }
            if(processingJob.Status != "Queued")
            {
                throw new Exception("Process Job is not queued");
            }
            DateTime startTime = DateTime.UtcNow;

            string  FilePath = Path.Combine(_configuration["FilePaths:SubmissionFilePath"]!, submissionFile.GeneratedFileName);
            if (!File.Exists(FilePath))
            {
                throw new Exception("File does not exists");
            }

            processingJob.Status = "Processing";
            
            await _context.SaveChangesAsync();
            await Task.Delay(10000);
            await using var stream = File.OpenRead(FilePath);

            var hash = await SHA256.HashDataAsync(stream);

            string checksum = Convert.ToHexString(hash);

            submissionFile.CheckSum = checksum;

            processingJob.StartedTime = startTime;
            processingJob.CompletedTime = DateTime.UtcNow;

            processingJob.Status = "Completed";

            await _context.SaveChangesAsync();
        }
        catch (Exception ex )
        {
            if(processingJob!.Attempts < 1)
            {
            processingJob!.ErrorSummary = ex.Message;
            processingJob!.Attempts += 1 ;
            processingJob!.Status = "Failed";
            }
            processingJob.Status = "Failed";
            await _context.SaveChangesAsync();  
            throw;
        }

    }
}