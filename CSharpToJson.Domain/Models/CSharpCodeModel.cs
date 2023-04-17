using System.ComponentModel.DataAnnotations;

namespace CSharpToJson.Domain.Models;

public class CSharpCodeModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "C# code is required")]
    public string Code { get; set; }
}