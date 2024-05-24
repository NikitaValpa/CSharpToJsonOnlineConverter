using Microsoft.CodeAnalysis.CSharp;

namespace CSharpToJson.Domain.Models
{
    public class ObjectModel : ICloneable
    {
        public ObjectModel(ObjectModel parent = null)
        {
            Children = new List<ObjectModel>();
            Parent = parent;
        }

        public List<ObjectModel> Children { get; set; }

        public ObjectModel Parent { get; set; }

        /// <summary>
        /// The syntatic kind of the token, ClassDeclaration or PropertyDeclaration
        /// </summary>
        public SyntaxKind TokenType { get; set; }

        /// <summary>
        /// The syntatic kind of the node, GenericName, PredefinedType, ArrayType
        /// </summary>
        public SyntaxKind NodeType { get; set; }

        /// <summary>
        /// Class name in case the type is a class or Property Name in case the type is a property
        /// </summary>
        public string SyntaxName { get; set; }

        /// <summary>
        /// The type of the property inside of the class
        /// </summary>
        public string PropertyType { get; set; } // int, byte[], string etc..

        /// <summary>
        /// What is the type in the generic list or array ? byte, string, int etc...
        /// </summary>
        public string GenericType { get; set; }

        public bool IsCycleReference { get; set; }

        public object Clone()
        {
            return new ObjectModel(Parent)
            {
                Children = new List<ObjectModel>(Children),
                TokenType = TokenType,
                NodeType = NodeType,
                SyntaxName = new string(SyntaxName),
                PropertyType = new string(PropertyType),
                GenericType = new string(GenericType),
                IsCycleReference = IsCycleReference
            };
        }
    }
}
