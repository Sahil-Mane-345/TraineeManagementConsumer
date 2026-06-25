using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TraineeAPI.Consumer.Constants;
using TraineeAPI.Consumer.Context;
using TraineeAPI.Consumer.Entity;
using TraineeAPI.Consumer.Models;

namespace TraineeAPI.Consumer.Services;

public class FileProcessingService : IFileProcessingService
{

    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    private readonly ILogger<FileProcessingService> _logger;
    public FileProcessingService(AppDbContext context, IConfiguration configuration, ILogger<FileProcessingService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task FileProcessingAsync(SubmissionProcessingRequestDto submissionProcessingRequestDto, IReadOnlyBasicProperties basicProperties)
    {
        SubmissionFile? submissionFile = null;
        ProcessingJob? processingJob = null;

        try
        {
            await Task.Delay(5000);
            
            submissionFile = await _context.SubmissionFiles.FirstOrDefaultAsync( f => f.Id == submissionProcessingRequestDto.SubmissionFileId);

            if(submissionFile == null)
            {
                throw new Exception("File Metadat Not found");
            }

            processingJob = await _context.ProcessingJobs.FirstOrDefaultAsync(p => p.MessageId == basicProperties.MessageId) ?? throw new Exception("Processing Job data not found");
            
            if (processingJob.Status == "Processing" || processingJob.Status == "Completed")
            {
                throw new Exception("Process Job is not queued or failed");
            }
            DateTime startTime = DateTime.UtcNow;

            string  FilePath = Path.Combine(_configuration["FilePaths:SubmissionFilePath"]!, submissionFile.GeneratedFileName);
            if (!File.Exists(FilePath))
            {
                throw new Exception("File does not exists");
            }

            processingJob.Status = "Processing";
            
            await _context.SaveChangesAsync();

            
            await using var stream = File.OpenRead(FilePath);

            var hash = await SHA256.HashDataAsync(stream);

            string checksum = Convert.ToHexString(hash);

            submissionFile.CheckSum = checksum;

            processingJob.StartedTime = startTime;
            processingJob.CompletedTime = DateTime.UtcNow;
            processingJob.Attempts += 1;
            processingJob.Status = "Completed";
            _logger.LogInformation("File checksumed");
            await _context.SaveChangesAsync();
        }
        catch (Exception ex )
        {
            _logger.LogError(ex.Message);
            processingJob!.ErrorSummary = ex.Message;
            processingJob!.Attempts += 1 ;
            processingJob.Status = "Failed";
            await _context.SaveChangesAsync();  
            throw new MaxAttemptException(processingJob.Attempts , ex.Message);
        }

    }
}