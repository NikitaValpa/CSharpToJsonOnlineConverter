using CSharpToJson.Domain.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CSharpToJson.Application.Services
{
    public class CSharpCodeAnalyzer : ICodeAnalyzer
    {
        private readonly ILogger<CSharpCodeAnalyzer> _logger;

        public CSharpCodeAnalyzer(ILogger<CSharpCodeAnalyzer> logger)
        {
            _logger = logger;
        }

        public (IEnumerable<ObjectModel> parsedTree, string error) Analyze(string input)
        {
            var objectModelList = new List<ObjectModel>();

            try
            {
                var tree = CSharpSyntaxTree.ParseText(input);
                var root = tree.GetCompilationUnitRoot();

                foreach (var child in root.ChildNodes())
                {
                    if (child.Kind() != SyntaxKind.ClassDeclaration) continue;

                    var parent = new ObjectModel();
                    foreach (var item in child.ChildTokens())
                    {
                        if (item.Kind() != SyntaxKind.IdentifierToken) continue;

                        parent.TokenType = SyntaxKind.ClassDeclaration;
                        parent.SyntaxName = item.Value.ToString();
                        break;
                    }

                    if (child.ChildNodes().Any())
                    {
                        AnalyzeChildNodes(child, parent);
                    }

                    objectModelList.Add(parent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error by analyze proceeding");
                return (null, "Your input code is invalid");
            }

            return (objectModelList, null);
        }

        private void AnalyzeChildNodes(SyntaxNode node, ObjectModel parent)
        {
            foreach (var child in node.ChildNodes())
            {
                if (child.Kind() == SyntaxKind.ClassDeclaration)
                {
                    var subParent = new ObjectModel(parent);
                    foreach (var item in child.ChildTokens())
                    {
                        if (item.Kind() != SyntaxKind.IdentifierToken) continue;

                        subParent.TokenType = SyntaxKind.ClassDeclaration;
                        subParent.SyntaxName = item.Value.ToString();
                        break;
                    }

                    if (child.ChildNodes().Any())
                    {
                        AnalyzeChildNodes(child, subParent);
                    }

                    parent.Children.Add(subParent);
                }
                else if (child.Kind() == SyntaxKind.PropertyDeclaration)
                {
                    var property = new ObjectModel(parent);
                    foreach (var item in child.ChildTokens())
                    {
                        if (item.Kind() != SyntaxKind.IdentifierToken) continue;

                        property.TokenType = SyntaxKind.PropertyDeclaration;
                        property.SyntaxName = item.Value.ToString();
                        break;
                    }

                    foreach (var item in child.ChildNodes())
                    {
                        if (item.Kind() == SyntaxKind.PredefinedType // int, bool, string
                            || item.Kind() == SyntaxKind.NullableType // Nullable kinds
                            || item.Kind() == SyntaxKind.IdentifierName) // Classes, Guid, Boolean, String
                        {
                            property.PropertyType = item.ToString();
                            property.NodeType = item.Kind();
                            break;
                        }

                        if (item.Kind() != SyntaxKind.GenericName && // Lists, Dictionaries
                            item.Kind() != SyntaxKind.ArrayType) continue; // Arrays

                        // get the generic type inside the array, or list
                        foreach (var genericItem in item.ChildNodes())
                        {
                            if (genericItem.Kind() != SyntaxKind.PredefinedType 
                                && genericItem.Kind() != SyntaxKind.NullableType) continue;

                            property.GenericType = genericItem.ToString();
                            break;
                        }
                        property.PropertyType = item.ToString();
                        property.NodeType = item.Kind();
                        break;
                    }
                    parent.Children.Add(property);
                }
            }
        }
    }
}
