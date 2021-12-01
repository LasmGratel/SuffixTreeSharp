using System;
using System.Collections.Generic;

namespace SuffixTreeSharp
{
    /// <summary>
    /// A Generalized Suffix Tree, based on the Ukkonen's paper "On-line construction of suffix trees"
    /// http://www.cs.helsinki.fi/u/ukkonen/SuffixT1withFigs.pdf
    /// <br />
    /// Allows for fast storage and fast(er) retrieval by creating a tree-based index out of a set of strings.
    /// Unlike common suffix trees, which are generally used to build an index out of one (very) long string,
    /// a Generalized Suffix Tree can be used to build an index over many strings.
    /// <br />
    /// Its main operations are put and search:
    /// Put adds the given key to the index, allowing for later retrieval of the given value.
    /// Search can be used to retrieve the set of all the values that were put in the index with keys that contain a given input.
    /// <br />
    /// In particular, after put(K, V), search(H) will return a set containing V for any string H that is substring of K.
    /// <br />
    /// The overall complexity of the retrieval operation (search) is O(m) where m is the length of the string to search within the index.
    /// <br />
    /// Although the implementation is based on the original design by Ukkonen, there are a few aspects where it differs significantly.
    /// <br />
    /// The tree is composed of a set of nodes and labeled edges. The labels on the edges can have any length as long as it's greater than 0.
    /// The only constraint is that no two edges going out from the same node will start with the same character.
    /// <br />
    /// Because of this, a given (startNode, stringSuffix) pair can denote a unique path within the tree, and it is the path (if any) that can be
    /// composed by sequentially traversing all the edges (e1, e2, ...) starting from startNode such that (e1.label + e2.label + ...) is equal
    /// to the stringSuffix.
    /// See the search method for details.
    /// <br />
    /// The union of all the edge labels from the root to a given leaf node denotes the set of the strings explicitly contained within the GST.
    /// In addition to those Strings, there are a set of different strings that are implicitly contained within the GST, and it is composed of
    /// the strings built by concatenating e1.label + e2.label + ... + $end, where e1, e2, ... is a proper path and $end is prefix of any of
    /// the labels of the edges starting from the last node of the path.
    /// <br />
    /// This kind of "implicit path" is important in the testAndSplit method.
    /// <br />
    /// Edited by mezz:
    /// - improve performance of search by passing a set around instead of creating new ones and using addAll
    /// - only allow full searches
    /// - add nullable/nonnull annotations
    /// - formatting
    /// </summary>
    public class GeneralizedSuffixTree : ISearchTree
    {
        public int HighestIndex = -1;

        /// <summary>
        /// The root of the suffix tree
        /// </summary>
        private readonly Node _root = new Node();

        /// <summary>
        /// The last leaf that was added during the update operation
        /// </summary>
        private Node _activeLeaf;

        public GeneralizedSuffixTree()
        {
            _activeLeaf = _root;
        }

        public ISet<int> Search(string word)
        {
            var tmpNode = SearchNode(word);
            if (tmpNode == null)
            {
                return new HashSet<int>();
            }

            var ret = new HashSet<int>();
            tmpNode.GetData(ret);
            return ret;
        }

        /// <summary>
        /// Returns the tree node (if present) that corresponds to the given string.
        /// </summary>
        private Node SearchNode(string word)
        {
            /*
             * Verifies if exists a path from the root to a node such that the concatenation
             * of all the labels on the path is a superstring of the given word.
             * If such a path is found, the last node on it is returned.
             */
            var currentNode = _root;

            for (var i = 0; i < word.Length; ++i)
            {
                var ch = word[i];
                // follow the edge corresponding to this char
                if (!currentNode.Edges.TryGetValue(ch, out var currentEdge))
                {
                    // there is no edge starting with this char
                    return null;
                }

                var label = currentEdge.Label;
                var lenToMatch = Math.Min(word.Length - i, label.Length);
                if (word.Substring(i, lenToMatch) != label.Substring(0, lenToMatch))
                {
                    // the label on the edge does not correspond to the one in the string to search
                    return null;
                }

                if (label.Length >= word.Length - i)
                    return currentEdge.Dest;

                // advance to next node
                currentNode = currentEdge.Dest;
                i += lenToMatch - 1;
            }

            return null;
        }

        /// <summary>
        /// Adds the specified <tt>index</tt> to the GST under the given <tt>key</tt>.
        ///
        /// Entries must be inserted so that their indexes are in non-decreasing order,
        /// otherwise an IllegalStateException will be raised.
        /// </summary>
        /// <param name="key">the string key that will be added to the index</param>
        /// <param name="index">the value that will be added to the index</param>
        public void Put(string key, int index)
        {
            if (index < HighestIndex)
            {
                throw new IndexOutOfRangeException(
                    "The input index must not be less than any of the previously inserted ones. Got " + index +
                    ", expected at least " + HighestIndex);
            }

            HighestIndex = index;

            // reset activeLeaf
            _activeLeaf = _root;

            var s = _root;

            // proceed with tree construction (closely related to procedure in Ukkonen's paper)
            var text = "";
            // iterate over the string, one char at a time
            for (var i = 0; i < key.Length; i++)
            {
                // line 6, line 7: update the tree with the new transitions due to this new char
                (s, text) = Update(s, text, key[i], key.Substring(i), index);
            }

            // add leaf suffix link, is necessary
            if (null == _activeLeaf.Suffix && _activeLeaf != _root && _activeLeaf != s)
            {
                _activeLeaf.Suffix = s;
            }
        }

        /// <summary>
        /// Tests whether the string stringPart + t is contained in the subtree that has inputs as root.
        /// If that's not the case, and there exists a path of edges e1, e2, ... such that
        /// e1.label + e2.label + ... + $end = stringPart
        /// and there is an edge g such that
        /// g.label = stringPart + rest
        /// 
        /// Then g will be split in two different edges, one having $end as label, and the other one
        /// having rest as label.
        /// </summary>
        /// <param name="inputs">the starting node</param>
        /// <param name="stringPart">the string to search</param>
        /// <param name="t">the following character</param>
        /// <param name="remainder">the remainder of the string to add to the index</param>
        /// <param name="value">the value to add to the index</param>
        /// <returns>a pair containing
        /// true/false depending on whether (stringPart + t) is contained in the subtree starting in inputs
        /// the last node that can be reached by following the path denoted by stringPart starting from inputs</returns>
        private (bool, Node) TestAndSplit(Node inputs, string stringPart, char t, string remainder, int value)
        {
            // descend the tree as far as possible
            var ret = canonize(inputs, stringPart);
            var s = ret.Item1;
            var str = ret.Item2;

            if (str != "")
            {
                var g = s.Edges[str[0]];
                var label = g.Label;
                // must see whether "str" is Substring of the label of an edge
                if (label.Length > str.Length && label[str.Length] == t)
                {
                    return (true, s);
                }

                // need to split the edge
                var newLabel = label.Substring(str.Length);
                if (!label.StartsWith(str)) throw new Exception();

                // build a new node
                var r = new Node();
                // build a new edge
                var newEdge = new Edge(str, r);

                g.Label = newLabel;

                // link s -> r
                r.Edges[newLabel[0]] = g;
                s.Edges[str[0]] = newEdge;

                return (false, r);
            }

            if (!s.Edges.TryGetValue(t, out var e))
            {
                // if there is no t-transtion from s
                return (false, s);
            }

            if (remainder == e.Label)
            {
                // update payload of destination node
                e.Dest.AddRef(value);
                return (true, s);
            }

            if (remainder.StartsWith(e.Label))
            {
                return (true, s);
            }

            if (e.Label.StartsWith(remainder))
            {
                // need to split as above
                var newNode = new Node();
                newNode.AddRef(value);

                var newEdge = new Edge(remainder, newNode);

                e.Label = e.Label.Substring(remainder.Length);

                newNode.Edges[e.Label[0]] = e;

                s.Edges[t] = newEdge;

                return (false, s);
            }

            // they are different words. No prefix. but they may still share some common substr
            return (true, s);
        }


        /// <summary>
        /// Return a (Node, String) (n, remainder) pair such that n is a farthest descendant of
        /// s (the input node) that can be reached by following a path of edges denoting
        /// a prefix of inputstr and remainder will be string that must be
        /// appended to the concatenation of labels from s to n to get inputstr.
        /// </summary>
        private (Node, string) canonize(Node s, string inputstr)
        {
            if (inputstr == "")
            {
                return (s, inputstr);
            }
            else
            {
                var currentNode = s;
                var str = inputstr;
                var g = s.Edges[str[0]];
                // descend the tree as long as a proper label is found
                while (g != null && str.StartsWith(g.Label))
                {
                    str = str.Substring(g.Label.Length);
                    currentNode = g.Dest;
                    if (str.Length > 0)
                    {
                        g = currentNode.Edges[str[0]];
                    }
                }

                return (currentNode, str);
            }
        }

        /// <summary>
        /// Updates the tree starting from inputNode and by adding stringPart.
        /// Returns a reference (Node, String) pair for the string that has been added so far.
        /// This means:
        /// - the Node will be the Node that can be reached by the longest path string (S1)
        /// that can be obtained by concatenating consecutive edges in the tree and
        /// that is a Substring of the string added so far to the tree.
        /// - the String will be the remainder that must be added to S1 to get the string
        /// added so far.
        /// </summary>
        /// <param name="inputNode">the node to start from</param>
        /// <param name="stringPart">the string to add to the tree</param>
        /// <param name="newChar"></param>
        /// <param name="rest">the rest of the string</param>
        /// <param name="value">the value to add to the index</param>
        private (Node, string) Update(Node inputNode, string stringPart, char newChar, string rest, int value)
        {
            var s = inputNode;
            var tempstr = stringPart + newChar;

            // line 1
            var oldroot = _root;

            // line 1b
            var (endpoint, r) = TestAndSplit(s, stringPart, newChar, rest, value);


            // line 2
            while (!endpoint)
            {
                // line 3
                Node leaf;
                if (r.Edges.TryGetValue(newChar, out var tempEdge))
                {
                    // such a node is already present. This is one of the main differences from Ukkonen's case:
                    // the tree can contain deeper nodes at this stage because different strings were added by previous iterations.
                    leaf = tempEdge.Dest;
                }
                else
                {
                    // must build a new leaf
                    leaf = new Node();
                    leaf.AddRef(value);
                    var newedge = new Edge(rest, leaf);
                    r.Edges[newChar] = newedge;
                }

                // update suffix link for newly created leaf
                if (_activeLeaf != _root)
                {
                    _activeLeaf.Suffix = leaf;
                }

                _activeLeaf = leaf;

                // line 4
                if (oldroot != _root)
                {
                    oldroot.Suffix = r;
                }

                // line 5
                oldroot = r;

                // line 6
                if (null == s.Suffix)
                {
                    // root node
                    if (_root != s) throw new Exception("Root node is not s");
                    // this is a special case to handle what is referred to as node _|_ on the paper
                    tempstr = tempstr.Substring(1);
                }
                else
                {
                    string tempstr1;
                    (s, tempstr1) = canonize(s.Suffix, SafeCutLastChar(tempstr));
                    tempstr = (tempstr1 + tempstr[tempstr.Length - 1]);
                }

                // line 7
                (endpoint, r) = TestAndSplit(s, SafeCutLastChar(tempstr), newChar, rest, value);
            }

            // line 8
            if (oldroot != _root)
            {
                oldroot.Suffix = r;
            }

            // make sure the active pair is canonical
            return canonize(s, tempstr);
        }

        private static string SafeCutLastChar(string seq)
        {
            return seq.Length == 0 ? "" : seq.Substring(0, seq.Length - 1);
        }
    }
}