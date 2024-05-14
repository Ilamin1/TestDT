using TestDT.Application.Constants;
using TestDT.Application.Models.Base;
using TestDT.Application.Models.Enums;

namespace TestDT.Api.Builders;

public class BaseResponseModelBuilder<TEntity>
{
    private string _message = string.Empty;

    private StatusType _statusCode;

    private TEntity _data = default!;

    /// <summary>
    /// Sets the response to indicate a successful operation.
    /// </summary>
    /// <param name="message">The success message (optional).</param>
    /// <returns>The updated instance of the response builder.</returns>
    public BaseResponseModelBuilder<TEntity> WithSuccess(string message = ResponseConstants.SuccessMessage)
    {
        _message = message;
        _statusCode = StatusType.Success;
        return this;
    }

    /// <summary>
    /// Sets the response to indicate an error.
    /// </summary>
    /// <param name="message">The error message (optional).</param>
    /// <returns>The updated instance of the response builder.</returns>
    public BaseResponseModelBuilder<TEntity> WithError(string message = ResponseConstants.ErrorMessage)
    {
        _message = message;
        _statusCode = StatusType.Failed;
        return this;
    }

    /// <summary>
    /// Sets the response with a custom status and message.
    /// </summary>
    /// <param name="message">The message to be set in the response (optional).</param>
    /// <param name="status">The status to be set in the response (optional).</param>
    /// <returns>The updated instance of the response builder.</returns>
    public BaseResponseModelBuilder<TEntity> WithStatus(string message = ResponseConstants.SuccessMessage, StatusType status = StatusType.Success)
    {
        _message = message;
        _statusCode = status;
        return this;
    }

    /// <summary>
    /// Sets the data to be included in the response.
    /// </summary>
    /// <param name="data">The data to be set in the response.</param>
    /// <returns>The updated instance of the response builder.</returns>
    public BaseResponseModelBuilder<TEntity> WithData(TEntity data)
    {
        _data = data;
        return this;
    }

    /// <summary>
    /// Builds the final response object using the configured properties.
    /// </summary>
    /// <returns>The constructed instance of the response object.</returns>
    public BaseResponseModel<TEntity> Build() =>
        new(_statusCode, _message, _data);
}