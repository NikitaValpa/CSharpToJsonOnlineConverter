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

                var clonedChildren = new List<ObjectModel>(propertyClassObject.Children.Count);

                propertyClassObject.Children.ForEach(x =>
                {
                    clonedChildren.Add((ObjectModel)x.Clone());
                });

                propertyModel.Children = clonedChildren;

                propertyModel.Children.ForEach(x =>
                {
                    x.Parent = propertyModel;
                });

                CheckChildOnCycleReference(propertyModel.Children, propertyModel);

                accessorList.Add(propertyModel.SyntaxName);

                for (var i = 0; i < propertyModel.Children.Count; i++)
                {
                    if (propertyModel.Children[i].IsCycleReference)
                    {
                        BuilderWriteCycleMember(propertyModel.Children[i].SyntaxName, CalculateIsLastObject(i));
                        continue;
                    }

                    var propertyClass = _objectModels.FirstOrDefault(p => p.SyntaxName == propertyModel.Children[i].PropertyType);
                    WritePropertyMember(propertyModel.Children[i], accessorList, propertyClass, CalculateIsLastObject(i));
                }

                BuilderWriteChildClassClosing(isLastObject);

                bool CalculateIsLastObject(int iterator) => iterator == propertyModel.Children.Count - 1;
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

        private void BuilderWriteCycleMember(string syntaxName, bool isLastObject = false)
        {
            _builder.Append($"\"{syntaxName}\": \"*is cycle dependency*\"{(isLastObject ? string.Empty : ",")}{Environment.NewLine}");
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

        private void CheckChildOnCycleReference(List<ObjectModel> child, ObjectModel parent)
        {
            var cycleProperties = child.Where(x =>
            {
                return parent.TokenType switch
                {
                    SyntaxKind.ClassDeclaration => x.PropertyType == parent.SyntaxName,
                    SyntaxKind.PropertyDeclaration => x.PropertyType == parent.PropertyType,
                    _ => false
                };
            }).ToList();

            if (cycleProperties.Any())
            {
                cycleProperties.ForEach(x => x.IsCycleReference = true);
            }

            if (parent.Parent != null)
            {
                CheckChildOnCycleReference(child, parent.Parent);
            }
        }
    }
}
