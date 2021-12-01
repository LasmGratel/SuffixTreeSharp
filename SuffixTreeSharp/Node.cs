using System;
using System.Collections.Generic;
using System.Text;

namespace SuffixTreeSharp
{
    public class Node
    {
        public readonly List<int> Data = new List<int>();
        public readonly Dictionary<char, Edge> Edges = new Dictionary<char, Edge>();
        public Node Suffix { get; set; } = null;

        /**
	     * Gets data from the payload of both this node and its children, the string representation
	     * of the path to this node is a substring of the one of the children nodes.
	     */
        public void GetData(ISet<int> ret)
        {
            Data.ForEach(x => ret.Add(x));

            foreach (var e in Edges.Values)
            {
                e.Dest.GetData(ret);
            }
        }

        /**
         * Adds the given <tt>index</tt> to the set of indexes associated with <tt>this</tt>
         * returns false if this node already contains the ref
         */
        public bool AddRef(int index)
        {
            if (Data.Contains(index))
            {
                return false;
            }
            
            Data.Add(index);

            // add this reference to all the suffixes as well
            var node = Suffix;
            while (node != null)
            {
                if (!node.Data.Contains(index))
                {
                    node.Data.Add(index);
                    node = node.Suffix;
                }
                else
                {
                    break;
                }
            }

            return true;
        }
        
        public override string ToString()
        {
            return "Node: size:" + Data.Count + " Edges: " + Edges;
        }
    }
}
