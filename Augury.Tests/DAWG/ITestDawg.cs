using System.Collections.Generic;

namespace Augury.Test.DAWG
{
    public interface ITestDawg
    {
        ICollection<string> AllWords();

        int EdgeCount { get; }
        int NodeCount { get; }
    }
}
