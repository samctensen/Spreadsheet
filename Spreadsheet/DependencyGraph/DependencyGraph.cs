// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)
// Version 1.3 - Sam Christensen
//              (Implemented the methods and added private helper methods for abstraction)
//              (Added spectacular comments for clarification)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        // To keep access and modifcation times constant, dictionarys mapping strings to hashsets of strings are used
        // to repreesent the dependents and the dependees of the graph
        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;
        private int _size;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
            Size = 0;
        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        /// <returns>
        /// the size of the graph
        /// </returns>
        public int Size
        {
            get { return _size; }
            private set { _size = value; }
        }

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// </summary>
        /// <returns> 
        /// the size of dependees("a") if it exists
        /// or returns -1 if it does not exist
        /// </returns>
        public int this[string s]
        {
            get
            { 
                if (dependees.ContainsKey(s))
                {
                    return dependees[s].Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        /// <returns>
        /// true of the node has dependents
        /// </returns>
        public bool HasDependents(string s)
        {
            if (dependents.ContainsKey(s))
            {
                return dependents[s].Count > 0;
            }
            return false;
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        /// <returns>
        /// true of the node has dependees
        /// </returns>
        public bool HasDependees(string s)
        {
            if (dependees.ContainsKey(s))
            {
                return dependees[s].Count > 0;
            }
            return false;
        }

        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        /// <returns>
        /// An IEnumerable set of the dependents for s, 
        /// or an empty set if s does not exist or has none
        /// </returns>
        public IEnumerable<string> GetDependents(string s)
        {
            if (dependents.ContainsKey(s))
            {
                return dependents[s];
            }
            return new HashSet<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        /// <returns>
        /// An IEnumerable set of the dependees for s, 
        /// or an empty set if s does not exist or has none
        /// </returns>
        public IEnumerable<string> GetDependees(string s)
        {
            if (dependees.ContainsKey(s))
            {
                return dependees[s];
            }
            return new HashSet<string>();
        }

        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        ///   t depends on s
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>       
        public void AddDependency(string s, string t)
        {
            // Either pulls the existing list of dependents or dependees, or creates a new one
            HashSet<string> getDependents;
            HashSet<string> getDependees;
            bool dependentsExists = false;
            bool dependeesExists = false;
            if (dependents.ContainsKey(s))
            {
                getDependents = dependents[s];
                dependentsExists = true;
            }
            else
            {
                getDependents = new HashSet<string>();
            }
            if (dependees.ContainsKey(t))
            {
                getDependees = dependees[t];
                dependeesExists = true;
            }
            else
            {
                getDependees = new HashSet<string>();
            }
            AddDependents(s, t, dependentsExists, getDependents);
            AddDependees(s, t, dependeesExists, getDependees);
        }

        /// <summary>
        /// Private helper method to add t to the hashset of dependents to then be added back into the dictionary
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is </param>
        /// <param name="exists"> boolean on whether the set of dependents already existed or was created</param>
        /// <param name="getDependents"> set of the dependents s of s</param>
        private void AddDependents(string s, string t, bool exists, HashSet<string> getDependents)
        {
            if (!getDependents.Contains(t))
            {
                getDependents.Add(t);
                if (exists)
                {
                    dependents[s] = getDependents;
                }
                else
                {
                    dependents.Add(s, getDependents);
                }
                Size++;
            }
        }

        /// <summary>
        /// Private helper method to add s to the hashset of dependees to then be added back into the dictionary
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is </param>
        /// <param name="exists"> boolean on whether the set of dependees already existed or was created</param>
        /// <param name="getDependents"> set of the dependees s of s</param>
        private void AddDependees(string s, string t, bool exists, HashSet<string> getDependees)
        {
            if (!getDependees.Contains(s))
            {
                getDependees.Add(s);
                if (exists)
                {
                    dependees[t] = getDependees;
                }
                else
                {
                    dependees.Add(t, getDependees);
                }
            }
        }

        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if(dependents.ContainsKey(s) && dependees.ContainsKey(t))
            {
                HashSet<string> getDependents = dependents[s];
                HashSet<string> getDependees = dependees[t];
                if (getDependents.Contains(t) && getDependees.Contains(s))
                {
                    getDependents.Remove(t);
                    dependents[s] = getDependents;
                    getDependees.Remove(s);
                    dependees[t] = getDependees;
                    // If the dependent or dependee has no more pairs, remove it from the dictionary
                    if (getDependents.Count == 0)
                    {
                        dependents.Remove(s);
                    }
                    if (getDependees.Count == 0)
                    {
                        dependees.Remove(t);
                    }
                    Size--;
                }
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (dependents.ContainsKey(s))
            {
                // Creates a copy to avoid current modification error
                HashSet<string> getDependents = new HashSet<string>(GetDependents(s));
                // Removes each old pair
                foreach (string r in getDependents)
                {
                    RemoveDependency(s, r);
                }
                // Adds a new pair for each item in the set
                foreach (string t in newDependents)
                {
                    AddDependency(s, t);
                }
            }
            else
            {
                foreach (string t in newDependents)
                {
                    AddDependency(s, t);
                }
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            if (dependees.ContainsKey(s))
            {
                // Creates a copy to avoid current modification error
                HashSet<string> getDependees = new HashSet<string>(GetDependees(s));
                // Removes each old pair
                foreach (string r in getDependees)
                {
                    RemoveDependency(r, s);
                }
                // Adds a new pair for each item in the set
                foreach (string t in newDependees)
                {
                    AddDependency(t, s);
                }
            }
            else
            {
                foreach (string t in newDependees)
                {
                    AddDependency(t, s);
                }
            }
        }
    }
}
