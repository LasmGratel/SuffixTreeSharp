using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuffixTreeSharp
{
    public class CombinedSearchTrees : ISearchTree
    {
        public readonly List<ISearchTree> SearchTrees = new List<ISearchTree>();
        
        public ISet<int> Search(string word)
        {
            ISet<int> searchResults = new HashSet<int>();
            return SearchTrees.Select(searchTree => searchTree.Search(word)).Aggregate(searchResults, Union);
        }
        
        /// <summary>
        /// Efficiently get all the elements from both sets.
        /// Note that this implementation will alter the original sets.
        /// </summary>
        private static ISet<int> Union(ISet<int> set1, ISet<int> set2)
        {
            if (set1.Count > set2.Count)
            {
                set1.UnionWith(set2);
                return set1;
            }

            set2.UnionWith(set1);
            return set2;
        }
	}
}
