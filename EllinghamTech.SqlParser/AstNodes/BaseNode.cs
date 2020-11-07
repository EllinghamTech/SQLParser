using System.Collections.Generic;

namespace EllinghamTech.SqlParser.AstNodes
{
    public abstract class BaseNode : INode
    {
        public bool IsFinal { get; protected set; } = false;
        public string Raw { get; set; }
        public List<INode> ChildNodes { get; protected set; } = new List<INode>();

        public BaseNode()
        {

        }
    }
}
