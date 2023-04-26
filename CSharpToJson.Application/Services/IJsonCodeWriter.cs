using CSharpToJson.Domain.Models;

namespace CSharpToJson.Application.Services
{
    public interface IJsonCodeWriter
    {
        (string json, string errors) Write(IEnumerable<ObjectModel> objectModels);
    }
}