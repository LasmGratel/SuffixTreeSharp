using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SuffixTreeSharp.Test
{
    [TestClass]
    public class SuffixTreeTest
    {
        public static void AssertEmpty<T>(ICollection<T> collection)
        {
            Assert.IsTrue(collection.Count == 0, "Expected empty collection.");
        }

        [TestMethod]
        public void TestBasicTreeGeneration()
        {
            var input = new GeneralizedSuffixTree();

            var word = "cacao";
            input.Put(word, 0);

            /* Test that every substring is contained within the tree */
            foreach (var s in word.GetSubstrings())
            {
                Assert.IsTrue(input.Search(s).Contains(0));
            }

            AssertEmpty(input.Search("caco"));
            AssertEmpty(input.Search("cacaoo"));
            AssertEmpty(input.Search("ccacao"));

            input = new GeneralizedSuffixTree();
            word = "bookkeeper";
            input.Put(word, 0);
            foreach (var s in word.GetSubstrings())
            {
                Assert.IsTrue(input.Search(s).Contains(0));
            }

            AssertEmpty(input.Search("books"));
            AssertEmpty(input.Search("boke"));
            AssertEmpty(input.Search("ookepr"));
        }

        [TestMethod]
        public void TestWeirdword()
        {
            var input = new GeneralizedSuffixTree();

            var word = "cacacato";
            input.Put(word, 0);

            /* Test that every substring is contained within the tree */
            foreach (var s in word.GetSubstrings())
            {
                Assert.IsTrue(input.Search(s).Contains(0));
            }
        }

        [TestMethod]
        public void TestDouble()
        {
            // Test whether the tree can handle repetitions
            var input = new GeneralizedSuffixTree();
            var word = "cacao";
            input.Put(word, 0);
            input.Put(word, 1);

            foreach (var s in word.GetSubstrings())
            {
                Assert.IsTrue(input.Search(s).Contains(0));
                Assert.IsTrue(input.Search(s).Contains(1));
            }
        }

        [TestMethod]
        public void TestBananaAddition()
        {
            var input = new GeneralizedSuffixTree();
            var words = new[] { "banana", "bano", "ba" };
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i);

                foreach (var s in words[i].GetSubstrings())
                {
                    var result = input.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }

            // verify post-addition
            for (var i = 0; i < words.Length; ++i)
            {
                foreach (var s in words[i].GetSubstrings())
                {
                    Assert.IsTrue(input.Search(s).Contains(i));
                }
            }

            // add again, to see if it's stable
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i + words.Length);

                foreach (var s in words[i].GetSubstrings())
                {
                    Assert.IsTrue(input.Search(s).Contains(i + words.Length));
                }
            }
        }

        [TestMethod]
        public void TestAddition()
        {
            var input = new GeneralizedSuffixTree();
            var words = new[] { "cacaor", "caricato", "cacato", "cacata", "caricata", "cacao", "banana" };
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i);

                foreach (var s in words[i].GetSubstrings())
                {
                    var result = input.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }

            // verify post-addition
            for (var i = 0; i < words.Length; ++i)
            {
                foreach (var s in words[i].GetSubstrings())
                {
                    var result = input.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }

            // add again, to see if it's stable
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i + words.Length);

                foreach (var s in words[i].GetSubstrings())
                {
                    Assert.IsTrue(input.Search(s).Contains(i + words.Length));
                }
            }

            //        input.computeCount();
            //        TestResultsCount(input.getRoot());

            AssertEmpty(input.Search("aoca"));
        }

        [TestMethod]
        public void TestSampleAddition()
        {
            var input = new GeneralizedSuffixTree();
            var words = new[]
            {
                "libertypike",
                "franklintn",
                "carothersjohnhenryhouse",
                "carothersezealhouse",
                "acrossthetauntonriverfromdightonindightonrockstatepark",
                "dightonma",
                "dightonrock",
                "6mineoflowgaponlowgapfork",
                "lowgapky",
                "lemasterjohnjandellenhouse",
                "lemasterhouse",
                "70wilburblvd",
                "poughkeepsieny",
                "freerhouse",
                "701laurelst",
                "conwaysc",
                "hollidayjwjrhouse",
                "mainandappletonsts",
                "menomoneefallswi",
                "mainstreethistoricdistrict",
                "addressrestricted",
                "brownsmillsnj",
                "hanoverfurnace",
                "hanoverbogironfurnace",
                "sofsavannahatfergusonaveandbethesdard",
                "savannahga",
                "bethesdahomeforboys",
                "bethesda"
            };
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i);

                foreach (var s in words[i].GetSubstrings())
                {
                    var result = input.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }

            // verify post-addition
            for (var i = 0; i < words.Length; ++i)
            {
                foreach (var s in words[i].GetSubstrings())
                {
                    Assert.IsTrue(input.Search(s).Contains(i));
                }
            }

            // add again, to see if it's stable
            for (var i = 0; i < words.Length; ++i)
            {
                input.Put(words[i], i + words.Length);

                foreach (var s in words[i].GetSubstrings())
                {
                    Assert.IsTrue(input.Search(s).Contains(i + words.Length));
                }
            }

            //        input.computeCount();
            //        TestResultsCount(input.getRoot());

            AssertEmpty(input.Search("aoca"));
        }

        //    private void TestResultsCount(Node n) {
        //        for (Edge e : n.getEdges().values()) {
        //            assertEquals(n.getData(-1).size(), n.getResultCount());
        //            TestResultsCount(e.getDest());
        //        }
        //    }

        /* Testing a Test method :) */
        [TestMethod]
        public void TestGetSubstrings()
        {
            var exp = new[] { "w", "r", "d", "wr", "rd", "wrd" }.ToHashSet();
            var ret = "wrd".GetSubstrings();
            Assert.IsTrue(ret.SetEquals(exp));
        }
    }
}