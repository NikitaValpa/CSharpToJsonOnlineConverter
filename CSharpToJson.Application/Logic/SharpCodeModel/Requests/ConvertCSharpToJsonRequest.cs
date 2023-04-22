using CSharpToJson.API.Models;
using CSharpToJson.Application.Services;
using CSharpToJson.Domain.Models;
using Mediator;

namespace CSharpToJson.Application.Logic.SharpCodeModel.Requests;

public class ConvertCSharpToJsonRequest : IRequest<JsonCodeViewModel>
{
    public CSharpCodeModel Request { get; set; }
}

public class ConvertCSharpToJsonRequestHandler : IRequestHandler<ConvertCSharpToJsonRequest, JsonCodeViewModel>
{
    private readonly ICodeAnalyzer _codeAnalyzer;
    private readonly ICodeWriter _codeWriter;

    public ConvertCSharpToJsonRequestHandler(ICodeAnalyzer codeAnalyzer, ICodeWriter codeWriter)
    {
        _codeAnalyzer = codeAnalyzer;
        _codeWriter = codeWriter;
    }

    public ValueTask<JsonCodeViewModel> Handle(ConvertCSharpToJsonRequest request, CancellationToken cancellationToken)
    {
        var analyzeRes = _codeAnalyzer.Analyze(request.Request.Code);

        if (!string.IsNullOrEmpty(analyzeRes.error))
        {
            return ValueTask.FromResult(new JsonCodeViewModel
            {
                Errors = analyzeRes.error
            });
        }

        var generatedJsonStr = _codeWriter.Write(analyzeRes.parsedTree);

        return ValueTask.FromResult(new JsonCodeViewModel
        {
            Json = generatedJsonStr
        });
    }
}