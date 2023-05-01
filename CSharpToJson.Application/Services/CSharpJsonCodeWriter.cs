using System.Text;
using CSharpToJson.Application.Common;
using CSharpToJson.Domain.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CSharpToJson.Application.Services
{
    public class CSharpJsonCodeWriter : IJsonCodeWriter
    {
        private StringBuilder _builder;
        private List<ObjectModel> _objectModels;
        private List<string> _errors;
        private readonly ILogger<CSharpJsonCodeWriter> _logger;

        public CSharpJsonCodeWriter(ILogger<CSharpJsonCodeWriter> logger)
        {
            _logger = logger;
        }

        public (string json, string errors) Write(IEnumerable<ObjectModel> objectModels)
        {
            _builder = new StringBuilder();
            _errors = new List<string>();
            _objectModels = objectModels.ToList();

            _builder.AppendLine("{");

            for (var i = 0; i < _objectModels.Count; i++)
            {
                WriteClassMember(_objectModels[i]);
                BuilderWriteClassClosing(i == _objectModels.Count-1);
                _builder.AppendLine();
            }

            _builder.AppendLine("}");

            if (_errors.Any())
            {
                return (null, string.Join(string.Empty, _errors.Distinct()));
            }

            return (_builder.ToString(), null);
        }

        private void WriteClassMember(ObjectModel classObject)
        {
            BuilderWriteClass(classObject.SyntaxName);

            if (!classObject.Children.Any()) return;

            for (var i = 0; i < classObject.Children.Count; i++)
            {
                WritePropertyMember(classObject.Children[i], new List<string> { classObject.SyntaxName }, 
                    isLastObject: i == classObject.Children.Count - 1);
            }
        }

        private void WritePropertyMember(ObjectModel propertyModel, List<string> accessorList, ObjectModel propertyClassObject = null,
            bool isLastObject = false)
        {
            if (propertyClassObject is {TokenType: SyntaxKind.ClassDeclaration}) // Handle class properties
            {
                var hasChildren = propertyClassObject.Children != null && propertyClassObject.Children.Any();
                BuilderWriteClass(propertyModel.SyntaxName);

                if (!hasChildren) return;

                accessorList.Add(propertyModel.SyntaxName);

                for (var i = 0; i < propertyClassObject.Children.Count; i++)
                {
                    var propertyClass = _objectModels.FirstOrDefault(p => p.SyntaxName == propertyClassObject.Children[i].PropertyType);
                    WritePropertyMember(propertyClassObject.Children[i], accessorList, propertyClass, i == propertyClassObject.Children.Count - 1);
                }

                BuilderWriteChildClassClosing(isLastObject);
            }
            else if (propertyModel.TokenType == SyntaxKind.PropertyDeclaration)
            {
                try
                {
                    switch (propertyModel.NodeType)
                    {
                        case SyntaxKind.ArrayType: // Handle Arrays
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapArray(propertyModel), isLastObject);
                            break;
                        case SyntaxKind.GenericName: // Handle Lists
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapObject(propertyModel), isLastObject);
                            break;
                        case SyntaxKind.PredefinedType: // Any Non Generic Property : bool
                        case SyntaxKind.NullableType: // Any Nullabe Type : Datetime?
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.Map(propertyModel.PropertyType), isLastObject);
                            break;
                        case SyntaxKind.IdentifierName: // Classes, Guid, Boolean, String
                            {
                                try // Boolean, Guid, String classes
                                {
                                    BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.Map(propertyModel.PropertyType), isLastObject);
                                }
                                catch (Exception) // Custom Classes
                                {
                                    var checkIfTypeExist = _objectModels.Exists(p => p.SyntaxName == propertyModel.PropertyType);
                                    var propertyClass = _objectModels.FirstOrDefault(p => p.SyntaxName == propertyModel.PropertyType);
                                    if (checkIfTypeExist)
                                    {
                                        WritePropertyMember(propertyModel, accessorList, propertyClass, isLastObject);
                                    }
                                    else
                                    {
                                        BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapObject(propertyModel), isLastObject);
                                    }
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException(
                                $"propertyModel.NodeType {propertyModel.NodeType} not handled in WritePropertyMember, CSharpJsonCodeWriter");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error by json write proceeding");
                    _errors.Add($"Unknown type \"{propertyModel.PropertyType}\" of property: {propertyModel.SyntaxName} {Environment.NewLine}");
                }
            }
        }

        private void BuilderWriteMember(string syntaxName, string typeExample, bool isLastObject = false)
        {
            _builder.Append($"\"{syntaxName}\": {typeExample ?? "null"}{(isLastObject ? string.Empty : ",")}{Environment.NewLine}");
        }

        private void BuilderWriteClass(string objectName)
        {
            _builder.Append($"\"{objectName}\": {{ {Environment.NewLine}");
        }

        private void BuilderWriteChildClassClosing(bool isLastObject = false)
        {
            _builder.Append($"}}{(isLastObject ? string.Empty : ",")}{Environment.NewLine}");
        }

        private void BuilderWriteClassClosing(bool isLastObject = false)
        {
            _builder.AppendLine($"}}{(isLastObject ? string.Empty : ",")}");
        }
    }
}
