using System.Collections.Generic;

namespace EllinghamTech.SqlParser.AstNodes
{
    public interface INode
    {
        /// <summary>
        /// This node is a "leaf" node - it contains no sub-nodes.
        /// </summary>
        bool IsFinal { get; }
        string Raw { get; set; }
        List<INode> ChildNodes { get; }
    }
}
