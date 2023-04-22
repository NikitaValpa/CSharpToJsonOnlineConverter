using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Services;

public interface ICodeAnalyzer
{
    public (IEnumerable<ObjectModel> parsedTree, string error) Analyze(string input);
}