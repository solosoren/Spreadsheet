using Formulas;
using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;

namespace Spreadsheet
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
                yield return pair.Key;
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
                    foreach (string dependent in dependencyGraph.GetDependents(name))
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

            if (dependencyGraph.HasDependees(name))
            {
                foreach (string dependee in dependencyGraph.GetDependees(name))
                {
                    set.Add(dependee);
                }
            }
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
                    foreach (string dependent in dependencyGraph.GetDependents(name))
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

            if (dependencyGraph.HasDependees(name))
            {
                foreach (string dependee in dependencyGraph.GetDependees(name))
                {
                    set.Add(dependee);
                }
            }
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
            cells.Add(name, formula);

            if (cells.ContainsKey(name))
            {
                cells[name] = formula;

                ISet<string> vars = formula.GetVariables();
                dependencyGraph.ReplaceDependees(name, vars);

                foreach (string var in vars)
                {   
                    if (IsACircle(var, name))
                    {
                        throw new CircularException();
                    }
                }
            }
            else
            {
                cells.Add(name, formula);
            }

            HashSet<string> tempSet = new HashSet<string>();
            foreach (string dependent in GetDirectDependents(name))
            {
                tempSet = GetIndirectDependents(dependent);
            }
            set.UnionWith(tempSet);
            
            return set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependent"></param>
        /// <returns></returns>
        public HashSet<string> GetIndirectDependents(string dependent)
        {
            
            HashSet<string> set = new HashSet<string>();
            foreach (string d in dependencyGraph.GetDependents(dependent))
            {
                set.Add(dependent);
                if (dependencyGraph.HasDependents(dependent))
                {
                    GetIndirectDependents(dependent);
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
            string d = String.Copy(dependee);
            
            foreach (string s in dependencyGraph.GetDependees(d))
            {
                if (s == target)
                {
                    return true;
                }
                if (dependencyGraph.HasDependees(s))
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

        private Boolean IsValid(String name) { return Regex.IsMatch(name, @"[a-zA-Z][0-9a-zA-Z]*"); }
    }
}
