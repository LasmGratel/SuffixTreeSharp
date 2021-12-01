using System;
using System.Collections.Generic;
using System.Text;

namespace SuffixTreeSharp
{
    public class Edge
    {
        public string Label { get; set; }
        public Node Dest { get; internal set; }

        public Edge(string label, Node dest)
        {
            Label = label;
            Dest = dest;
        }
    }
}
