using Formulas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

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
        private Regex isValid;

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }


        /// <summary>
        /// Creates an empty Spreadsheet whose IsValid regular expression accepts every string
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, object>();
            dependencyGraph = new Dependencies.DependencyGraph();
            this.isValid = new Regex(".");
            Changed = false;
        }

        /// <summary>
        /// Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        /// </summary>
        public Spreadsheet(Regex isValid) :this()
        {
            this.isValid = isValid;
        }

        /// <summary>
        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.
        ///
        /// See the AbstractSpreadsheet.Save method and Spreadsheet.xsd for the file format 
        /// specification.  
        ///
        /// If there's a problem reading source, throws an IOException.
        ///
        /// Else if the contents of source are not consistent with the schema in Spreadsheet.xsd, 
        /// throws a SpreadsheetReadException.  
        ///
        /// Else if the IsValid string contained in source is not a valid C# regular expression, throws
        /// a SpreadsheetReadException.  (If the exception is not thrown, this regex is referred to
        /// below as oldIsValid.)
        ///
        /// Else if there is a duplicate cell name in the source, throws a SpreadsheetReadException.
        /// (Two cell names are duplicates if they are identical after being converted to upper case.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a 
        /// SpreadsheetReadException.  (Use oldIsValid in place of IsValid in the definition of 
        /// cell name validity.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetVersionException.  (Use newIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there's a formula that causes a circular dependency, throws a SpreadsheetReadException. 
        ///
        /// Else, create a Spreadsheet that is a duplicate of the one encoded in source except that
        /// the new Spreadsheet's IsValid regular expression should be newIsValid.
        /// </summary>
        public Spreadsheet(TextReader source, Regex newIsValid) :this()
        {
            isValid = newIsValid;
            string oldIsValid = @"."; 
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add(null, "Spreadsheet.xsd");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;

            try
            {
                using (XmlReader reader = XmlReader.Create(source, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    break;

                                case "isValid":
                                    oldIsValid = reader["regex"];
                                    if (!Regex.IsMatch(oldIsValid, @"."))
                                    {
                                        throw new SpreadsheetReadException("Invalid isValid");
                                    }
                                    break;

                                case "cell":
                                    string name = reader["name"];
                                    string contents = reader["contents"];
                                    if (cells.ContainsKey(name.ToUpper())) { throw new SpreadsheetReadException("Multiple of the same cell"); }

                                    if (Regex.IsMatch(name, oldIsValid))
                                    {   
                                        try
                                        {
                                            Formula f = new Formula(contents, s => s, s => Regex.IsMatch(s, oldIsValid));
                                        }

                                        catch (Exception) { throw new SpreadsheetReadException("Invalid Formula"); }
                                        
                                    } else { throw new SpreadsheetReadException("Invalid Cell Name"); }
                                    
                                    if (Regex.IsMatch(name, isValid.ToString()))
                                    {
                                        try
                                        {
                                            Formula f = new Formula(contents, s => s, s => Regex.IsMatch(s, isValid.ToString()));
                                            
                                        }

                                        catch (Exception) { throw new SpreadsheetVersionException("Invalid Formula"); }

                                    }
                                    else { throw new SpreadsheetVersionException("Invalid Cell Name"); }

                                    SetContentsOfCell(name, contents);

                                    break;
                            }
                        }
                    }
                }  
            }
            catch (InvalidNameException i)
            {
                throw new SpreadsheetVersionException(i.Message);
            }
            catch (FormulaFormatException f)
            {
                throw new SpreadsheetVersionException(f.Message);
            }

        }


        /// <summary>
        /// Returns names of all the used cells.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (string key in cells.Keys)
            {
                if (!(cells[key] is string s) || !(s == ""))
                {
                    yield return key;
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
        protected override ISet<string> SetCellContents(string name, double number)
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
        protected override ISet<string> SetCellContents(string name, string text)
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
        protected override ISet<string> SetCellContents(string name, Formula formula)
        { 
            if (name == null || !IsValid(name)) { throw new InvalidNameException(); }

            HashSet<string> set = new HashSet<string>{ name };

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

            foreach (string s in GetCellsToRecalculate(set))
            {
                SetContentsOfCell(s, GetCellValue(s).ToString());
            }

            return set;
        }

        /// <summary>
        /// Returns the Indirect dependees of the given dependee
        /// </summary>
        /// <param name="dependee"></param>
        /// <returns></returns>
        private HashSet<string> GetIndirectDependees(string dependee)
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
       /// Recursively checks whether the given target has a circular dependency
       /// </summary>
       /// <param name="dependee"></param>
       /// <param name="target"></param>
       /// <returns></returns>
        private Boolean IsACircle(string dependee, string target)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Boolean IsValid(String name) {
            return Regex.IsMatch(name, isValid.ToString());
        }


        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the IsValid attribute should be IsValid.ToString()
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(dest))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("", "spreadsheet", "urn:spreadsheet-schema");

                    writer.WriteAttributeString("IsValid", isValid.ToString());

                    foreach (string s in GetNamesOfAllNonemptyCells())
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteAttributeString("name", s);
                        object o = cells[s];
                        if (o is Formula)
                        {
                            string f = "=" + o.ToString();
                            writer.WriteAttributeString("contents", f);
                        }
                        else
                        {
                            writer.WriteAttributeString("contents", cells[s].ToString());
                        }
                        
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    Changed = false;
                }
            }
            catch (Exception _)
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            if (name.Equals(null) || !IsValid(name) || !isValid.IsMatch(name.ToUpper())) { throw new InvalidNameException(); }

            Object contents = GetCellContents(name);
            if (contents is Formula formula)
            {

                ISet<string> set = formula.GetVariables();
                foreach (string var in set)
                {
                    if (!cells.ContainsKey(var))
                    {
                        return new FormulaError("Variable " + var + " does not have a value yet");
                    }
                }
                return formula.Evaluate(s => (double)cells[s]);

            }
            else if (contents is double)
            {
                return (double)contents;
            }
            else
            {
                return (string)contents;
            }
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            if (content == null) { throw new ArgumentNullException(); }
            if (name == null || !IsValid(name) || !isValid.IsMatch(name.ToUpper())) { throw new InvalidNameException(); }
            Changed = true;

            name = name.ToUpper();
            // double
            if (double.TryParse(content, out double doubleContents))
            {
                return SetCellContents(name, doubleContents);
            }
            
            // formula
            if (content.StartsWith("="))
            {
                Formula formula = new Formula(content.Substring(1), s => s.ToUpper(), s => IsValid(name));
                return SetCellContents(name, formula);
            }

            // string
            return SetCellContents(name, content);
        }
    }
}
