using Microsoft.AspNetCore.Mvc;
using Hangfire;
using MyFirstApi.Services;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackgroundController(IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    [HttpGet("test-background-job")]
    public IActionResult TestBackgroundJob()
    {
        var jobId = backgroundJobClient.Enqueue("critical",() => Console.WriteLine("Hello, World!"));
        return Ok(jobId);
    }

    [HttpGet("test-background-job2")]
    public IActionResult TestBackgroundJob2()
    {
        var jobId = backgroundJobClient.Enqueue(() => Console.WriteLine("Hello, World! 2"));
        return Ok(jobId);
    }

    [HttpGet("test-background-job3")]
    public IActionResult TestBackgroundJob3()
    {
        var jobId = backgroundJobClient.Schedule(() => Console.WriteLine("Hello, World! 3"),
       // TimeSpan.FromSeconds(10)     
        DateTimeOffset.UtcNow.AddSeconds(10)
        );
        return Ok(jobId);
    }
   // [HttpGet("test-background-job4")]
    //public IActionResult TestBackgroundJob4()
    //{
     //   RecurringJob.AddOrUpdate("test-background-job4", () => Console.WriteLine("Hello, World! 4"), Cron.Minutely);
     //   return Ok();
    //}
    [HttpGet("test-background-job5")]
    public IActionResult TestBackgroundJob5()
    {
        var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Hello, World! 5"));
        BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Hello, World! 6"));
        BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Hello, World! 7"),
            JobContinuationOptions.OnAnyFinishedState);
        return Ok(jobId);
    }
}
