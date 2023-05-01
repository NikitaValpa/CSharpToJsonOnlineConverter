using CSharpToJson.Application.Services;
using CSharpToJson.Domain.Models;
using Mediator;

namespace CSharpToJson.Application.Logic.SharpCodeModel.Commands;

public class ConvertCSharpToJsonCommand : ICommand<JsonCodeViewModel>
{
    public CSharpCodeModel Request { get; set; }
}

public class ConvertCSharpToJsonCommandHandler : ICommandHandler<ConvertCSharpToJsonCommand, JsonCodeViewModel>
{
    private readonly ICodeAnalyzer _codeAnalyzer;
    private readonly IJsonCodeWriter _jsonCodeWriter;

    public ConvertCSharpToJsonCommandHandler(ICodeAnalyzer codeAnalyzer, IJsonCodeWriter jsonCodeWriter)
    {
        _codeAnalyzer = codeAnalyzer;
        _jsonCodeWriter = jsonCodeWriter;
    }

    public ValueTask<JsonCodeViewModel> Handle(ConvertCSharpToJsonCommand command, CancellationToken cancellationToken)
    {
        var analyzeRes = _codeAnalyzer.Analyze(command.Request.Code);

        if (!string.IsNullOrEmpty(analyzeRes.error))
        {
            return ValueTask.FromResult(new JsonCodeViewModel
            {
                Errors = analyzeRes.error
            });
        }

        var generatedJsonRes = _jsonCodeWriter.Write(analyzeRes.parsedTree);

        if (!string.IsNullOrEmpty(generatedJsonRes.errors))
        {
            return ValueTask.FromResult(new JsonCodeViewModel
            {
                Errors = generatedJsonRes.errors
            });
        }

        return ValueTask.FromResult(new JsonCodeViewModel
        {
            Json = generatedJsonRes.json
        });
    }
}