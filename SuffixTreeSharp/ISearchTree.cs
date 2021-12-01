using System;
using System.Collections.Generic;
using System.Text;

namespace SuffixTreeSharp
{
    public interface ISearchTree
    {
        ISet<int> Search(string word);
    }
}
