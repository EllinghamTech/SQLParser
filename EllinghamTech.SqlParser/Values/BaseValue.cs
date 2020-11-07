using System.Collections.Generic;
using EllinghamTech.SqlParser.AstNodes;
using EllinghamTech.SqlParser.Tokens;

namespace EllinghamTech.SqlParser.Values
{
    public class BaseValue : BaseToken, INode
    {
        public bool IsFinal { get; }
        public List<INode> ChildNodes { get; }
    }
}
