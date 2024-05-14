using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestDT.Api.Builders;
using TestDT.Application.Models.Base;
using TestDT.Application.Models.Mediators.Commands;

namespace TestDT.Api.Controllers;

[ApiController]
[Route("/api")]
public class DataUploadController(ISender mediator) : ControllerBase
{
    [HttpPost("/upload")]
    public async Task<BaseResponseModel<object>> UploadFile(IFormFile formFile)
    {
        var modelBuilder = new BaseResponseModelBuilder<object>();
        try
        {
            var fileStream = formFile.OpenReadStream();
            await mediator.Send(new DataUploadCommand(fileStream));
            return modelBuilder.WithSuccess().Build();
        }
        catch (Exception ex)
        {
            return modelBuilder.WithError(ex.Message).Build();
        }
    }
}