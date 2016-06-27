using Augury.Base;

namespace Augury.PriorityQueue
{
    public sealed class WordSimilarityNode : IWordSimilarityNode
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        public double Similarity { get; set; }

        /// <summary>
        /// The node's value.
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }
    }
}
