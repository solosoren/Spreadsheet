using Formulas;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SS
{
    /// <summary>
    /// Creates a spreadsheet
    /// 
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {

        private Dictionary<String, Object> cells;
        private Dependencies.DependencyGraph dependencyGraph;

        /// <summary>
        /// Creates an empty spreadsheet.
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, object>();
            dependencyGraph = new Dependencies.DependencyGraph();
        }

        /// <summary>
        /// Returns names of all the used cells.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (KeyValuePair<String, Object> pair in cells)
            {
                if (pair.Value != "")
                {
                    yield return pair.Key;
                }
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
           if (name == null || !IsValid(name)) { throw new InvalidNameException(); }
           
           if (cells.ContainsKey(name)) { return cells[name]; }

           return "";

        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, double number)
        {
            if (name == null || !IsValid(name)) { throw new InvalidNameException(); }

            HashSet<string> set = new HashSet<string>();

            if (cells.ContainsKey(name))
            {
                cells[name] = number;
                if (dependencyGraph.HasDependents(name))
                {
                    HashSet<string> temp = new HashSet<string>();
                    foreach (string dependent in dependencyGraph.GetDependents(name))
                    {
                        temp.Add(dependent);
                    }
                    foreach (string dependent in temp)
                    {
                        dependencyGraph.RemoveDependency(name, dependent);
                    }
                }
            }
            else
            {
                cells.Add(name, number);
            }

            set.Add(name);

            HashSet<string> tempSet = new HashSet<string>();
            foreach (string dependee in dependencyGraph.GetDependees(name))
            {
                tempSet.Add(dependee);
                tempSet.UnionWith(GetIndirectDependees(dependee));
            }
            set.UnionWith(tempSet);

            GetCellsToRecalculate(set);
            return set;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null) { throw new ArgumentNullException(); }
            if (name == null || !IsValid(name)) { throw new InvalidNameException(); }

            HashSet<string> set = new HashSet<string>();

            if (cells.ContainsKey(name))
            {
                cells[name] = text;
                if (dependencyGraph.HasDependents(name))
                {
                    HashSet<string> temp = new HashSet<string>();
                    foreach (string dependent in dependencyGraph.GetDependents(name))
                    {
                        temp.Add(dependent);
                    }
                    foreach (string dependent in temp)
                    {
                        dependencyGraph.RemoveDependency(name, dependent);
                    }
                }
            }
            else
            {
                cells.Add(name, text);
            }
            
            set.Add(name);

            HashSet<string> tempSet = new HashSet<string>();
            foreach (string dependee in dependencyGraph.GetDependees(name))
            {
                tempSet.Add(dependee);
                tempSet.UnionWith(GetIndirectDependees(dependee));
            }
            set.UnionWith(tempSet);

            GetCellsToRecalculate(set);

            return set;
        }

        /// <summary>
        /// Requires that all of the variables in formula are valid cell names.
        /// 
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetCellContents(string name, Formula formula)
        { 
            if (name == null || !IsValid(name)) { throw new InvalidNameException(); }

            HashSet<string> set = new HashSet<string>();
            set.Add(name);

            if (cells.ContainsKey(name))
            {
                
                ISet<string> vars = formula.GetVariables();
                dependencyGraph.ReplaceDependents(name, vars);

                foreach (string var in vars)
                {   
                    if (IsACircle(var, name))
                    {
                        throw new CircularException();
                    }
                }
                cells[name] = formula;
            }
            else
            {
                
                ISet<string> vars = formula.GetVariables();
                foreach (string s in vars)
                {
                    dependencyGraph.AddDependency(name, s);

                    if (IsACircle(s, name))
                    {
                        throw new CircularException();
                    }
                }
                cells.Add(name, formula);
            }

            HashSet<string> tempSet = new HashSet<string>();
            foreach (string dependee in dependencyGraph.GetDependees(name))
            {
                tempSet.Add(dependee);
                tempSet.UnionWith(GetIndirectDependees(dependee));
            }
            set.UnionWith(tempSet);

            GetCellsToRecalculate(set);

            return set;
        }

        /// <summary>
        /// Returns the Indirect dependees of the given dependee
        /// </summary>
        /// <param name="dependee"></param>
        /// <returns></returns>
        public HashSet<string> GetIndirectDependees(string dependee)
        {
            HashSet<string> set = new HashSet<string>();
            
            foreach (string d in dependencyGraph.GetDependees(dependee))
            {
                set.Add(d);
                if (dependencyGraph.HasDependees(d))
                {
                   set.UnionWith(GetIndirectDependees(d));
                }
            }

            return set;
            
        }
     
       /// <summary>
       /// recursively checks whether the given target has a circular dependency
       /// </summary>
       /// <param name="dependee"></param>
       /// <param name="target"></param>
       /// <returns></returns>
        public Boolean IsACircle(string dependee, string target)
        {
            foreach (string s in dependencyGraph.GetDependents(dependee))
            {
                if (s == target)
                {
                    return true;
                }
                if (dependencyGraph.HasDependents(s))
                {
                    if (IsACircle(s, target) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null) { throw new ArgumentNullException(); }
            if (!IsValid(name)) { throw new ArgumentNullException(); }

            return dependencyGraph.GetDependents(name);
        }

        private Boolean IsValid(String name) {
            return Regex.IsMatch(name, @"[a-zA-Z]+[0-9]+");
        }
    }
}
