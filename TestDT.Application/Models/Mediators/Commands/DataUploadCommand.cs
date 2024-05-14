using MediatR;

namespace TestDT.Application.Models.Mediators.Commands;

public class DataUploadCommand : IRequest<Unit>
{
    public DataUploadCommand(Stream fileStream)
    {
        this.FileStream = fileStream;
    }
    
    public Stream FileStream { get; set; }
}