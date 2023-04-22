using System.Text;
using CSharpToJson.Application.Common;
using CSharpToJson.Domain.Enums;
using CSharpToJson.Domain.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CSharpToJson.Application.Services
{
    public class CSharpCodeWriter : ICodeWriter
    {
        private readonly StringBuilder _builder;
        private readonly bool _useAssignmentStatements;
        private readonly ILogger<CSharpCodeWriter> _logger;
        private List<ObjectModel> _objectModels;

        public CSharpCodeWriter(ILogger<CSharpCodeWriter> logger, Dictionary<string, string> settings = null)
        {
            _logger = logger;
            _builder = new StringBuilder();

            if (settings == null) return;

            foreach (var item in settings)
            {
                _useAssignmentStatements = item.Key == SettingTypes.UseAssignmentStatements.ToString() && bool.Parse(item.Value);
            }
        }

        public string Write(IEnumerable<ObjectModel> objectModels)
        {
            _objectModels = objectModels.ToList();
            foreach (var classObject in _objectModels)
            {
                WriteClassMember(classObject);
                BuilderWriteClassClosing();
            }

            return _builder.ToString();
        }

        private void WriteClassMember(ObjectModel classObject)
        {

            var accessorClassName = classObject.SyntaxName.ToLowerInvariant();

            BuilderWriteClass(classObject.SyntaxName, accessorClassName);

            if (!classObject.Children.Any()) return;

            foreach (var child in classObject.Children)
            {
                var accessorList = new List<string> {accessorClassName};
                WritePropertyMember(child, accessorList);
            }
        }

        private void WritePropertyMember(ObjectModel propertyModel, List<string> accessorList, ObjectModel propertyClassObject = null)
        {
            if (propertyClassObject != null && propertyClassObject.TokenType == SyntaxKind.ClassDeclaration) // Handle class properties
            {
                var hasChildren = propertyClassObject.Children != null && propertyClassObject.Children.Any();
                BuilderWriteClassMember(propertyModel.SyntaxName, propertyModel.PropertyType, hasChildren, accessorList);

                if (!hasChildren) return;

                accessorList.Add(propertyModel.SyntaxName);
                foreach (var child in propertyClassObject.Children)
                {
                    var propertyClass = _objectModels.FirstOrDefault(p => p.SyntaxName == child.PropertyType);
                    WritePropertyMember(child, accessorList, propertyClass);
                }
                    
                BuilderWriteChildClassClosing();
            }
            else if (propertyModel.TokenType == SyntaxKind.PropertyDeclaration)
            {
                try
                {
                    switch (propertyModel.NodeType)
                    {
                        case SyntaxKind.ArrayType: // Handle Arrays
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapArray(propertyModel), accessorList);
                            break;
                        case SyntaxKind.GenericName: // Handle Lists
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapObject(propertyModel), accessorList);
                            break;
                        case SyntaxKind.PredefinedType: // Any Non Generic Property : bool
                        case SyntaxKind.NullableType: // Any Nullabe Type : Datetime?
                            BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.Map(propertyModel.PropertyType), accessorList);
                            break;
                        case SyntaxKind.IdentifierName: // Classes, Guid, Boolean, String
                            {
                                try // Boolean, Guid, String classes
                                {
                                    BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.Map(propertyModel.PropertyType), accessorList);
                                }
                                catch (Exception) // Custom Classes
                                {
                                    var checkIfTypeExist = _objectModels.Exists(p => p.SyntaxName == propertyModel.PropertyType);
                                    var propertyClass = _objectModels.FirstOrDefault(p => p.SyntaxName == propertyModel.PropertyType);
                                    if (checkIfTypeExist)
                                    {
                                        WritePropertyMember(propertyModel, accessorList, propertyClass);
                                    }
                                    else
                                    {
                                        BuilderWriteMember(propertyModel.SyntaxName, CSharpTypesMapping.MapObject(propertyModel), accessorList);
                                    }
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException(
                                $"propertyModel.NodeType {propertyModel.NodeType} not handled in WritePropertyMember, CSharpCodeWriter");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error by json write proceeding");
                    _builder.AppendFormat("// Unknown Property : {0} {1}", propertyModel.SyntaxName, Environment.NewLine);
                }
            }
        }

        private void BuilderWriteMember(string syntaxName, string typeExample, List<string> accessorList = null)
        {
            if (_useAssignmentStatements)
            {
                if (accessorList == null) return;

                var compoundedAccessorNames = 
                    accessorList.Aggregate(string.Empty, (current, accessor) => current + (accessor + "."));

                _builder.AppendFormat("{0}{1} = {2};{3}", compoundedAccessorNames, syntaxName, typeExample, Environment.NewLine);
            }
            else
            {
                _builder.AppendFormat("{0} = {1},{2}", syntaxName, typeExample, Environment.NewLine);
            }
        }

        private void BuilderWriteClassMember(string syntaxName, string accessorClassName, bool hasChildren, List<string> accessorList = null)
        {
            if (_useAssignmentStatements)
            {
                if (accessorList == null) return;

                var compoundedAccessorNames = accessorList
                    .Aggregate(string.Empty, (current, accessor) => current + (accessor + "."));

                compoundedAccessorNames += syntaxName;
                _builder.AppendFormat("{0} = new {1}(){2} {3}", compoundedAccessorNames, accessorClassName, ";", Environment.NewLine);
            }
            else
            {
                _builder.AppendFormat("{0} = new {1}(){2} {3}", syntaxName, accessorClassName, hasChildren ? "{" : ",", Environment.NewLine);
            }
        }

        private void BuilderWriteClass(string syntaxName, string accessorClassName)
        {
            _builder.AppendFormat(_useAssignmentStatements ? "{0} {1} = new {0}();{2}" : "{0} {1} = new {0}() {{ {2}",
                syntaxName, accessorClassName, Environment.NewLine);
        }

        private void BuilderWriteChildClassClosing()
        {
            if (!_useAssignmentStatements)
            {
                _builder.AppendFormat("}}, {0}", Environment.NewLine);
            }
        }

        private void BuilderWriteClassClosing()
        {
            if (!_useAssignmentStatements)
            {
                _builder.AppendLine("};");
            }
            _builder.AppendLine();
        }
    }
}
