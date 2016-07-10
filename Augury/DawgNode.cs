using System.Collections.Generic;
using System.Linq;
using System.Text;

//https://gist.github.com/smhanov/94230b422c2100ae4218

namespace Augury
{
    internal class DawgNode
    {
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(TerminalNode ? "1" : "0");
            foreach (var edge in Children)
            {
                result.Append(edge.Key + edge.Value.ToString());
            }

            return " " + result;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as DawgNode;
            if (other == null)
            {
                return false;
            }

            return ToString() == other.ToString();
        }
        
        public int Id { get; protected set; }
        public int Traversal(ref int low, ref int high, ICollection<DawgNode> nodes)
        {
            if (Id != 0)
            {
                return 0;
            }

            if (TerminalNode)
            {
                Id = --low;
            }
            else
            {
                Id = ++high;
            }

            var result = 0;
            nodes.Add(this);
            foreach(var child in Children)
            {
                result += 1 + child.Value.Traversal(ref low, ref high, nodes);
            }

            return result;
        }

        public bool TerminalNode { get; set; }
        public IDictionary<char, DawgNode> Children = new Dictionary<char, DawgNode>();
        
        public IOrderedEnumerable<KeyValuePair<char, DawgNode>> SortedChildren
        {
            get { return Children.OrderBy(e => e.Key); }
        }        
    }
}
