using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Services
{
    public interface ICodeWriter
    {
        string Write(IEnumerable<ObjectModel> objectModels);
    }
}