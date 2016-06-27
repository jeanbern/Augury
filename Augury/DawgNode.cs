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

        public ICollection<DawgNode> GetNodes()
        {
            var visited = new HashSet<DawgNode>();
            NodesReachable(visited);
            return visited;
        }

        public bool TerminalNode { get; set; }
        public IDictionary<char, DawgNode> Children = new Dictionary<char, DawgNode>();

        public int NodesReachable()
        {
            return NodesReachable(new HashSet<DawgNode>());
        }

        public IReadOnlyCollection<string> StringsReachable()
        {
            var results = new List<string>();
            StringsReachable(new StringBuilder(), results);
            return results;
        }

        public IOrderedEnumerable<KeyValuePair<char, DawgNode>> SortedChildren
        {
            get { return Children.OrderBy(e => e.Key); }
        }

        private int NodesReachable(HashSet<DawgNode> visited)
        {
            if (visited == null)
            {
                visited = new HashSet<DawgNode>();
            }

            if (!visited.Add(this))
            {
                return 0;
            }

            return 1 + Children.Aggregate(0, (i, pair) => i + pair.Value.NodesReachable(visited));
        }

        private void StringsReachable(StringBuilder soFar, ICollection<string> values)
        {
            foreach (var child in Children)
            {
                soFar.Append(child.Key);
                if (child.Value.TerminalNode)
                {
                    values.Add(soFar.ToString());
                }

                child.Value.StringsReachable(soFar, values);
                soFar.Length--;
            }
        }
    }
}
