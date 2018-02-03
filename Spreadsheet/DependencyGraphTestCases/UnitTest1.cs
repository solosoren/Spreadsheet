using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Collections.Generic;

// Soren Nelson

namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// Adds 1000 values to dependency Graph
        /// </summary>        
        [TestMethod]
        public void TestAdd1000()
        {
            DependencyGraph graph = new DependencyGraph();
            for (int i = 0; i < 1000; i++)
            {
                graph.AddDependency(i.ToString(), i.ToString());
                Assert.AreEqual(graph.Size, i + 1);
            }
        }

        /// <summary>
        ///Checks has dependents
        /// </summary>        
        [TestMethod]
        public void HasDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency(1.ToString(), 2.ToString());
            Assert.AreEqual(graph.HasDependents(1.ToString()), true);
        }

        /// <summary>
        ///Checks does not have dependents
        /// </summary>        
        [TestMethod]
        public void DoesNotHaveDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency(1.ToString(), 2.ToString());
            Assert.AreEqual(graph.HasDependents(2.ToString()), false);
        }

        /// <summary>
        ///Checks has dependees
        /// </summary>        
        [TestMethod]
        public void HasDependees()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency(1.ToString(), 2.ToString());
            Assert.AreEqual(graph.HasDependees(2.ToString()), true);
        }

        /// <summary>
        ///Checks does not have dependees
        /// </summary>        
        [TestMethod]
        public void DoesNotHaveDependees()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency(1.ToString(), 2.ToString());
            Assert.AreEqual(graph.HasDependees(1.ToString()), false);
        }


        /// <summary>
        /// Removes values contained in DPG
        /// </summary>
        [TestMethod]
        public void TestRemoveWithValue()
        {
            DependencyGraph graph = new DependencyGraph();
            for (int i = 0; i < 1000; i++)
            {
                graph.AddDependency(i.ToString(), i.ToString());
            }
            for (int i = 0; i < 1000; i++)
            {
                graph.RemoveDependency((i).ToString(), (i).ToString());
            }
        }

        /// <summary>
        /// Removes value not contained in DPG
        /// </summary>
        [TestMethod]
        public void TestRemoveWithoutValue()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency(1.ToString(), 2.ToString());

            graph.RemoveDependency(2.ToString(), 1.ToString());
        }


        /// <summary>
        /// Replaces dependees contained in DPG with values from Strings()
        /// </summary>
        [TestMethod]
        public void TestReplaceDependeesWithValue()
        {
            DependencyGraph graph = new DependencyGraph();
            for (int i = 0; i < 1000; i++)
            {
                graph.AddDependency(i.ToString(), i.ToString());
            }
            for (int i = 0; i < 1000; i++)
            {
                graph.ReplaceDependees(i.ToString(), Strings());
            }
        }

        /// <summary>
        /// Replaces dependees contained in DPG with values from Strings()
        /// </summary>
        [TestMethod]
        public void TestReplaceDependentsWithValue()
        {
            DependencyGraph graph = new DependencyGraph();
            for (int i = 0; i < 1000; i++)
            {
                graph.AddDependency(i.ToString(), i.ToString());
            }
            for (int i = 0; i < 1000; i++)
            {
                graph.ReplaceDependents(i.ToString(), Strings());
            }
        }







        /// <summary>
        /// Creates an IEnumerable of strings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Strings()
        {
            for (int i = 1000; i < 2000; i++)
            {
                yield return (i * 2).ToString();
            }
        }



    }
}
