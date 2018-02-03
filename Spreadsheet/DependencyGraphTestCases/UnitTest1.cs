using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;
using System.Collections.Generic;

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
                graph.AddDependency((i+1).ToString(), (i+1).ToString());
            }
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
        /// Replaces dependees contained in DPG
        /// </summary>
        [TestMethod]
        public void TestReplaceWithValue()
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
