using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Services
{
    public interface IJsonCodeWriter
    {
        string Write(IEnumerable<ObjectModel> objectModels);
    }
}