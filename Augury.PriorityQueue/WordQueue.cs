using Augury.Base;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Augury.PriorityQueue
{
    /// <summary>
    /// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    /// </summary>
    public sealed class WordQueue : IEnumerable<IWordSimilarityNode>
    {
        private int _numNodes;
        private readonly WordSimilarityNode[] _nodes;
        private readonly List<WordSimilarityNode> _extras = new List<WordSimilarityNode>();

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
        public WordQueue(int maxNodes)
        {
            _nodes = new WordSimilarityNode[maxNodes + 1];
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// If the queue is full, the result is undefined.
        /// If the node is already enqueued, the result is undefined.
        /// O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(string word, double priority)
        {
            if (_numNodes >= _nodes.Count() - 1)
            {
                if (priority > First.Similarity)
                {
                    Dequeue();
                    _extras.Clear();
                }
                else if (priority == First.Similarity)
                {
                    _extras.Add(new WordSimilarityNode { Similarity = priority, Word = word });
                    return;
                }
                else
                {
                    return;
                }
            }

            var node = new WordSimilarityNode { Similarity = priority, Word = word };
            _numNodes++;
            _nodes[_numNodes] = node;
            node.QueueIndex = _numNodes;
            CascadeUp(_nodes[_numNodes]);
        }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// </summary>
        public IWordSimilarityNode Dequeue()
        {
            var node = _nodes[1];
            //If the node is already the last node, we can remove it immediately
            if (node.QueueIndex == _numNodes)
            {
                _nodes[_numNodes] = null;
                _numNodes--;
                return node;
            }

            //Swap the node with the last node
            var formerLastNode = _nodes[_numNodes];
            Swap(node, formerLastNode);
            _nodes[_numNodes] = null;
            _numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            var parentIndex = formerLastNode.QueueIndex / 2;
            var parentNode = _nodes[parentIndex];

            if (parentIndex > 0 && formerLastNode.Similarity < parentNode.Similarity)
            {
                CascadeUp(formerLastNode);
            }
            else
            {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                CascadeDown(formerLastNode);
            }

            return node;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// If the queue is empty, behavior is undefined.
        /// O(1)
        /// </summary>
        public IWordSimilarityNode First
        {
            get
            {
                return _nodes[1];
            }
        }

        public IEnumerator<IWordSimilarityNode> GetEnumerator()
        {
            return _nodes.Skip(1).Take(_numNodes).Union(_extras).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(WordSimilarityNode node1, WordSimilarityNode node2)
        {
            //Swap the nodes
            _nodes[node1.QueueIndex] = node2;
            _nodes[node2.QueueIndex] = node1;

            //Swap their indicies
            var temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        private void CascadeUp(WordSimilarityNode node)
        {
            //aka Heapify-up
            var parent = node.QueueIndex / 2;
            while (parent >= 1)
            {
                var parentNode = _nodes[parent];
                if (parentNode.Similarity < node.Similarity)
                {
                    break;
                }

                //Node has lower priority value, so move it up the heap
                Swap(node, parentNode); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

                parent = node.QueueIndex / 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeDown(WordSimilarityNode node)
        {
            //aka Heapify-down
            var finalQueueIndex = node.QueueIndex;
            while (true)
            {
                var newParent = node;
                var childLeftIndex = 2 * finalQueueIndex;

                //Check if the left-child is higher-priority than the current node
                if (childLeftIndex > _numNodes)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != node twice
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }

                var childLeft = _nodes[childLeftIndex];
                if (childLeft.Similarity < newParent.Similarity)
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current node or the left child
                var childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= _numNodes)
                {
                    var childRight = _nodes[childRightIndex];
                    if (childRight.Similarity < newParent.Similarity)
                    {
                        newParent = childRight;
                    }
                }

                //If either of the children has higher (smaller) priority, swap and continue cascading
                if (newParent != node)
                {
                    //Move new parent to its new index.  node will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    _nodes[finalQueueIndex] = newParent;

                    var temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    //See note above
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }
        #endregion private methods
    }
}