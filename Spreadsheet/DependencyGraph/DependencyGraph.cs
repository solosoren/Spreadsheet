// Skeleton implementation written by Joe Zachary for CS 3500, January 2018.
// Soren Nelson

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// This is a Dictionary containing a key and the dependees for that key inside of a hashset for the value
        /// </summary>
        public Dictionary<string, HashSet<string>> dependees { get; private set; }
        /// <summary>
        /// This is a Dictionary containing a key and the dependents for that key inside of a hashset for the value
        /// </summary>
        public Dictionary<string, HashSet<string>> dependents { get; private set; }
        private int size;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            dependees = new Dictionary<string, HashSet<string>>();
            dependents = new Dictionary<string, HashSet<string>>();
            size = 0;
        }

        public DependencyGraph(DependencyGraph dg) : this()
        {
            foreach (KeyValuePair<string, HashSet<string>> pair in dg.dependees) {
                dependees.Add(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<string, HashSet<string>> pair in dg.dependents)
            {
                dependents.Add(pair.Key, pair.Value);
            }
            size = dg.size;
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty. Thows ArgumentNullException if s is null.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            return dependents.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty. Throws ArgumentNullException if s is null.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            return dependees.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).  Throws ArgumentNullException if s is null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependents.ContainsKey(s))
            {
                HashSet<string> values = dependents[s];
                foreach (string dependent in values)
                {
                    yield return dependent;
                }
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Throws ArgumentNullException if s is null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (dependees.ContainsKey(s))
            {
                HashSet<string> values = dependees[s];
                foreach (string dependee in values)
                {
                    yield return dependee;
                }
            }
            
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Throws ArgumentNullException if s or t is null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }
            if (!GetDependees(t).Contains(s))
            {
                size++;
            }
            if (dependents.ContainsKey(s)) { dependents[s].Add(t);
            }
            else
            {
                HashSet<string> set = new HashSet<string> { t };
                dependents.Add(s, set);
            }

            if (dependees.ContainsKey(t)) { dependees[t].Add(s);
            }
            else
            {
                HashSet<string> set = new HashSet<string> { s };
                dependees.Add(t, set);
            }
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Throws ArgumentNullException if s or t is null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }
            if (dependents.ContainsKey(s) && dependents[s].Contains(t)) {
                
                if (dependents[s].Count == 1)
                {
                    dependents.Remove(s);
                }
                else
                {
                    dependents[s].Remove(t);
                }
                
                if (dependees[t].Count == 1)
                {
                    dependees.Remove(t);
                }
                else
                {
                    dependees[t].Remove(s);
                }

                size--;
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Throws ArgumentNullException if s or newDependents or one of the dependents in newDependents is null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (s == null || newDependents == null)
            {
                throw new ArgumentNullException();
            }
            IEnumerable<string> oldDependents = GetDependents(s).ToList();

            foreach (string dependent in oldDependents)
            {
                RemoveDependency(s, dependent);
            }
            foreach (string dependent in newDependents)
            {
                if (dependent == null)
                {
                    throw new ArgumentNullException();
                }
                AddDependency(s, dependent);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Throws ArgumentNullException if t or newDependees or a dependee in newDependees is null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            if (t == null || newDependees == null)
            {
                throw new ArgumentNullException();
            }
            IEnumerable<string> oldDependees = GetDependees(t).ToList();

            foreach (string dependee in oldDependees)
            {
                RemoveDependency(dependee, t);
            }
            foreach (string dependee in newDependees)
            {
                if (dependee == null)
                {
                    throw new ArgumentNullException();
                }
                AddDependency(dependee, t);
            }
        }
    }
}
