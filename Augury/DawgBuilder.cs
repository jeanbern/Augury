using System;
using System.Collections.Generic;
using System.Linq;

//http://stevehanov.ca/blog/index.php?id=115
//https://gist.github.com/smhanov/94230b422c2100ae4218
//No license

namespace Augury
{
    internal class DawgBuilder
    {
        private string _previousWord = "";
        public DawgNode Root = new DawgNode();
        private readonly Dictionary<DawgNode, DawgNode> _minimizedNodes = new Dictionary<DawgNode, DawgNode>();
        private readonly Stack<Tuple<DawgNode, char, DawgNode>> _uncheckedNodes = new Stack<Tuple<DawgNode, char, DawgNode>>();

        public void Insert(string word)
        {
            if (string.Compare(word, _previousWord, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                throw new Exception($"We expect the words to be sorted. But we received {_previousWord} followed by {word}");
            }

            int commonPrefix;
            for (commonPrefix = 0; commonPrefix < Math.Min(word.Length, _previousWord.Length); commonPrefix++)
            {
                if (word[commonPrefix] != _previousWord[commonPrefix])
                {
                    break;
                }
            }

            Minimize(commonPrefix);

            var node = _uncheckedNodes.Count == 0 ? Root : _uncheckedNodes.Peek().Item3;

            foreach (var letter in word.Skip(commonPrefix))
            {
                var nextNode = new DawgNode();
                node.Children[letter] = nextNode;
                _uncheckedNodes.Push(new Tuple<DawgNode, char, DawgNode>(node, letter, nextNode));
                node = nextNode;
            }

            node.TerminalNode = true;
            _previousWord = word;
        }

        public DawgNode Finish()
        {
            Minimize(0);
            _minimizedNodes.Clear();
            _uncheckedNodes.Clear();

            return Root;
        }

        private void Minimize(int downTo)
        {
            for (var i = _uncheckedNodes.Count - 1; i > downTo - 1; i--)
            {
                var unNode = _uncheckedNodes.Pop();
                var parent = unNode.Item1;
                var letter = unNode.Item2;
                var child = unNode.Item3;

                DawgNode newChild;
                if (_minimizedNodes.TryGetValue(child, out newChild))
                {
                    parent.Children[letter] = newChild;
                }
                else
                {
                    _minimizedNodes.Add(child, child);
                }
            }
        }
    }
}
