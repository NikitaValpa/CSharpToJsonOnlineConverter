using CSharpToJson.API.Models;
using CSharpToJson.Domain.Models;
using Mediator;

namespace CSharpToJson.Application.Logic.SharpCodeModel.Requests;

public class ConvertCSharpToJsonRequest : IRequest<JsonCodeViewModel>
{
    public CSharpCodeModel Request { get; set; }
}

public class ConvertCSharpToJsonRequestHandler : IRequestHandler<ConvertCSharpToJsonRequest, JsonCodeViewModel>
{
    public ValueTask<JsonCodeViewModel> Handle(ConvertCSharpToJsonRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new JsonCodeViewModel
        {
            Json = request.Request.Code,
            Errors = "errors"
        });
    }
}