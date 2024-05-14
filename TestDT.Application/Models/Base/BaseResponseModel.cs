using TestDT.Application.Models.Enums;

namespace TestDT.Application.Models.Base;

public class BaseResponseModel<TEntity>
{
    public BaseResponseModel(StatusType status, string message, TEntity data)
    {
        this.Type = status;
        this.Message = message;
        this.Data = data;
    }

    public StatusType Type { get; set; }
    
    public string Message { get; set; }
    
    public TEntity Data { get; set; }
}