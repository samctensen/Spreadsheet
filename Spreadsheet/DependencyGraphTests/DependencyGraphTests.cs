using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;


namespace DevelopmentTests
{
    /// <summary>
    ///This is a test class for DependencyGraphTest and is intended
    ///to contain all DependencyGraphTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DependencyGraphTest
    {
        // PROVIDED TESTS

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyTest()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyRemoveTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyEnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            Assert.AreEqual("x", e1.Current);
            IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
            Assert.IsTrue(e2.MoveNext());
            Assert.AreEqual("y", e2.Current);
            t.RemoveDependency("x", "y");
            Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
            Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void SimpleReplaceTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(t.Size, 1);
            t.RemoveDependency("x", "y");
            t.ReplaceDependents("x", new HashSet<string>());
            t.ReplaceDependees("y", new HashSet<string>());
        }

        ///<summary>
        ///It should be possibe to have more than one DG at a time.
        ///</summary>
        [TestMethod()]
        public void StaticTest()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("x", "y");
            Assert.AreEqual(1, t1.Size);
            Assert.AreEqual(0, t2.Size);
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void SizeTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            Assert.AreEqual(4, t.Size);
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void EnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void ReplaceThenEnumerate()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            t.AddDependency("a", "z");
            t.ReplaceDependents("b", new HashSet<string>());
            t.AddDependency("y", "b");
            t.ReplaceDependents("a", new HashSet<string>() { "c" });
            t.AddDependency("w", "d");
            t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
            t.ReplaceDependees("d", new HashSet<string>() { "b" });

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 200;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 4; j < SIZE; j += 4)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Add some back
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j += 2)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove some more
            for (int i = 0; i < SIZE; i += 2)
            {
                for (int j = i + 3; j < SIZE; j += 3)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        // MY TESTS
        [TestMethod()]
        public void OneDependencySize()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
            t.AddDependency("1", "2");
            Assert.AreEqual(1, t.Size);
        }

        [TestMethod()]
        public void OneDependencyRemovedSize()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
            t.AddDependency("1", "2");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("1", "2");
            Assert.AreEqual(0, t.Size);
        }

        [TestMethod()]
        public void Add100DependencysSize()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                string numAddOne = (index + 1).ToString();
                t.AddDependency(num, numAddOne);
            }
            Assert.AreEqual(100, t.Size);
        }

        [TestMethod()]
        public void Remove100DependencysSize()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                string numAddOne = (index + 1).ToString();
                t.AddDependency(num, numAddOne);
            }
            Assert.AreEqual(100, t.Size);
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                string numAddOne = (index + 1).ToString();
                t.RemoveDependency(num, numAddOne);
            }
            Assert.AreEqual(0, t.Size);
        }

        [TestMethod()]
        public void AddOneIndexer()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a1", "b1");
            Assert.AreEqual(t["b1"], 1);
        }

        [TestMethod()]
        public void Add100Indexer()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                t.AddDependency(num, "nuts");
            }
            Assert.AreEqual(t["nuts"], 100);
        }


        [TestMethod()]
        public void HasNoDependents()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsFalse(t.HasDependents("b"));
        }

        [TestMethod()]
        public void HasOneDependent()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependents("a"));
        }

        [TestMethod()]
        public void HasNoDependentsAfterRemove()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependents("a"));
            t.RemoveDependency("a", "b");
            Assert.IsFalse(t.HasDependents("a"));
        }

        [TestMethod()]
        public void HasDependentsAfterReplace()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependents("a"));
            string[] array = { "z" };
            t.ReplaceDependents("a", array);
            Assert.IsTrue(t.HasDependents("a"));
        }

        [TestMethod()]
        public void HasOneDependee()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependees("b"));
        }

        [TestMethod()]
        public void HasNoDependeesAfterRemove()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependees("b"));
            t.RemoveDependency("a", "b");
            Assert.IsFalse(t.HasDependees("b"));
        }

        [TestMethod()]
        public void HasDependeesAfterReplace()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.IsTrue(t.HasDependees("b"));
            string[] array = { "z" };
            t.ReplaceDependees("b", array);
            Assert.IsTrue(t.HasDependees("b"));
        }

        [TestMethod()]
        public void GetNoDependents()
        {
            DependencyGraph t = new DependencyGraph();
            IEnumerator<string> dependents = t.GetDependents("a").GetEnumerator();
            Assert.IsFalse(dependents.MoveNext());
        }

        [TestMethod()]
        public void Get100Dependents()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                t.AddDependency("nuts", num);
            }
            IEnumerator<string> dependents = t.GetDependents("nuts").GetEnumerator();
            for (int index = 1; index < 101; index++)
            {
                Assert.IsTrue(dependents.MoveNext());
            }
        }

        [TestMethod()]
        public void GetNoDependees()
        {
            DependencyGraph t = new DependencyGraph();
            IEnumerator<string> dependents = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(dependents.MoveNext());
        }

        [TestMethod()]
        public void Get100Dependees()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                t.AddDependency(num, "nuts");
            }
            IEnumerator<string> dependees = t.GetDependees("nuts").GetEnumerator();
            for (int index = 1; index < 101; index++)
            {
                Assert.IsTrue(dependees.MoveNext());
            }
        }

        [TestMethod()]
        public void AddNewDependentAndNewDependee()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            IEnumerator<string> dependees = t.GetDependees("b").GetEnumerator();
            IEnumerator<string> dependents = t.GetDependents("x").GetEnumerator();
            dependees.MoveNext();
            dependents.MoveNext();
            Assert.AreEqual(dependees.Current, "x");
            Assert.AreEqual(dependents.Current, "b");
        }

        [TestMethod()]
        public void AddToExistingDependentandDependees()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "a");
            t.AddDependency("y", "a");
            t.AddDependency("x", "b");
            IEnumerator<string> dependees = t.GetDependees("a").GetEnumerator();
            IEnumerator<string> dependents = t.GetDependents("x").GetEnumerator();
            dependees.MoveNext();
            dependents.MoveNext();
            Assert.AreEqual(dependees.Current, "x");
            Assert.AreEqual(dependents.Current, "a");
            dependees.MoveNext();
            dependents.MoveNext();
            Assert.AreEqual(dependees.Current, "y");
            Assert.AreEqual(dependents.Current, "b");
        }

        [TestMethod()]
        public void Add100Remove99Depenencys()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                string numAddOne = (index + 1).ToString();
                t.AddDependency(num, numAddOne);
            }
            Assert.AreEqual(100, t.Size);
            for (int index = 1; index < 100; index++)
            {
                string num = index.ToString();
                string numAddOne = (index + 1).ToString();
                t.RemoveDependency(num, numAddOne);
            }
            IEnumerator<string> dependees = t.GetDependees("101").GetEnumerator();
            IEnumerator<string> dependents = t.GetDependents("100").GetEnumerator();
            dependees.MoveNext();
            dependents.MoveNext();
            Assert.AreEqual(dependees.Current, "100");
            Assert.AreEqual(dependents.Current, "101");
        }

        [TestMethod()]
        public void RemoveNonExistantDependency()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("a", "c");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("z", "x");
            Assert.AreEqual(1, t.Size);
        }

        [TestMethod()]
        public void Replace100Dependents()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                t.AddDependency("nuts", num);
            }
            Assert.AreEqual(t.Size, 100);
            string[] array = new string[100];
            for (int value = 100, index = 0; value > 0; value--, index++)
            {
                string num = value.ToString();
                array[index] = num;
            }
            t.ReplaceDependents("nuts", array);
            IEnumerator<string> dependents = t.GetDependents("nuts").GetEnumerator();
            for (int index = 100; index > 0; index--)
            {
                dependents.MoveNext();
                Assert.AreEqual(dependents.Current, index.ToString());
            }
        }

      

        [TestMethod()]
        public void Replace100Dependees()
        {
            DependencyGraph t = new DependencyGraph();
            for (int index = 1; index < 101; index++)
            {
                string num = index.ToString();
                t.AddDependency(num, "nuts");
            }
            Assert.AreEqual(t.Size, 100);
            string[] array = new string[100];
            for (int value = 100, index = 0; value > 0; value--, index++)
            {
                string num = value.ToString();
                array[index] = num;
            }
            t.ReplaceDependees("nuts", array);
            IEnumerator<string> dependees = t.GetDependees("nuts").GetEnumerator();
            for (int index = 100; index > 0; index--)
            {
                dependees.MoveNext();
                Assert.AreEqual(dependees.Current, index.ToString());
            }
        }
    }
}