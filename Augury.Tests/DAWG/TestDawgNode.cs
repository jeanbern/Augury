using System.Collections.Generic;
using System.Text;

namespace Augury.Test.DAWG
{
    public class TestDawgNode : DawgNode, ITestDawg
    {
        public TestDawgNode(DawgNode node)
        {
            Id = node.Id;
            TerminalNode = node.TerminalNode;
            Children = node.Children;
        }

        public ICollection<string> AllWords()
        {
            var allWords = new HashSet<string>();
            TraverseNode(allWords, new StringBuilder(), this);
            return allWords;
        }

        public int EdgeCount
        {
            get
            {
                var collection = new HashSet<DawgNode>();
                int low = 0, high = 0;
                return Traversal(ref low, ref high, collection, new HashSet<char>());
            }
        }

        public int NodeCount
        {
            get
            {
                var collection = new HashSet<DawgNode>();
                int low = 0, high = 0;
                Traversal(ref low, ref high, collection, new HashSet<char>());
                return collection.Count;
            }
        }

        private static void TraverseNode(ISet<string> words, StringBuilder builder, DawgNode node)
        {
            foreach (var child in node.Children)
            {
                builder.Append(child.Key);
                if (child.Value.TerminalNode)
                {
                    words.Add(builder.ToString());
                }

                TraverseNode(words, builder, child.Value);
                builder.Length -= 1;
            }
        }
    }
}
