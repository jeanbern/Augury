namespace Augury.Base
{
    public interface IWordSimilarityNode
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        double Similarity { get;}

        /// <summary>
        /// The node's value.
        /// </summary>
        string Word { get; }
    }
}
