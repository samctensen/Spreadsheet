using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a cell name if and only if it consists of one or more letters,
    /// followed by one or more digits AND it satisfies the predicate IsValid.
    /// For example, "A15", "a15", "XY032", and "BC7" are cell names so long as they
    /// satisfy IsValid.  On the other hand, "Z", "X_", and "hello" are not cell names,
    /// regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized with the Normalize method before it is used by or saved in 
    /// this spreadsheet.  For example, if Normalize is s => s.ToUpper(), then
    /// the Formula "x3+a5" should be converted to "X3+A5" before use.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// Private class representing a singular cell in a spreadsheet. Privately stores
        /// cell contents on a string
        /// </summary>
        private class Cell
        {
            private Object contents;
            private Object value;

            /// <summary>
            /// Contrsuctor for the cell class. Takes an object parameter to store as its contents
            /// </summary>
            /// <param name="obj">Contents of the cell</param>
            public Cell(object obj)
            {
                contents = obj;
            }

            /// <summary>
            /// Setter method to change the value of the contents of a cell
            /// </summary>
            /// <param name="obj"></param>
            public void SetContents(object obj)
            {
                contents = obj;
            }

            /// <summary>
            /// Getter method that returns the contents of the cell
            /// </summary>
            /// <returns></returns>
            public Object GetContents()
            {
                return contents;
            }

            /// <summary>
            /// Setter method to change the value of a cell
            /// </summary>
            /// <param name="obj"></param>
            public void SetValue(object obj)
            {
                value = obj;
            }

            /// <summary>
            /// Getter method that returns the contents of the cell
            /// </summary>
            /// <returns></returns>
            public Object GetValue()
            {
                return value;
            }
        }

        private DependencyGraph dependencyGraph;
        private HashSet<string> nonEmptyCellNames;
        private Dictionary<string, Cell> nonEmptyCells;
        private bool isChanged;
        private bool isDefault = false;

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed 
        {
            get { return isChanged; }
            protected set { isChanged = value; }
        }

        /// <summary>
        /// Contructor for the spreadsheet class. Initializes a new dependency graph to track the relationship
        /// between cells, a hashset to track the names of existing cells, and a dictionary to map names to cells.
        /// Uses a basic isValid that returns true for all values, and a normalize method that doesnt change the original
        /// string. Uses default version.
        /// </summary>
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            dependencyGraph = new DependencyGraph();
            nonEmptyCellNames = new HashSet<string>();
            nonEmptyCells = new Dictionary<string, Cell>();
            isDefault = true;
            Changed = false;
        }

        /// <summary>
        /// Contructor for the spreadsheet class. Initializes a new dependency graph to track the relationship
        /// between cells, a hashset to track the names of existing cells, and a dictionary to map names to cells.
        /// Takes in an IsValid and Normalize method parameters, and a string for the version.
        /// </summary>
        /// <param name="isValid"> returns a boolean on whether the string is valid </param>
        /// <param name="normalize"> may modify string to normalize it </param>
        /// <param name="version"> the spreadsheet xml version </param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            dependencyGraph = new DependencyGraph();
            nonEmptyCellNames = new HashSet<string>();
            nonEmptyCells = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Contructor for the spreadsheet class. Initializes a new dependency graph to track the relationship
        /// between cells, a hashset to track the names of existing cells, and a dictionary to map names to cells.
        /// Reads in an xml file spreadsheet from the string filepath. Takes in an IsValid and Normalize method parameters,
        /// and a string for the version. 
        /// </summary>
        /// <param name="filepath"> filepath of the file to be read </param>
        /// <param name="isValid"> returns a boolean on whether the string is valid </param>
        /// <param name="normalize"> may modify string to normalize it </param>
        /// <param name="version"> the spreadsheet xml version </param>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            dependencyGraph = new DependencyGraph();
            nonEmptyCellNames = new HashSet<string>();
            nonEmptyCells = new Dictionary<string, Cell>();
            GetSavedVersion(filepath);
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(String filename)
        {
            try
            {
                string version = "";
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    int index = 0;
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            // Checks if the tag found is either contents, cell, spreadsheet, or name
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    version = reader.GetAttribute("version");
                                    // Checks if version is the first tag
                                    if (index != 0)
                                    {
                                        throw new SpreadsheetReadWriteException("Invalid tag order");
                                    }
                                    // Checks if the versions match
                                    if (isDefault)
                                    {
                                        Version = version;
                                    }
                                    if (version != Version)
                                    {
                                        throw new SpreadsheetReadWriteException("File versions don't match");
                                    }
                                    break;
                                case "cell":
                                    reader.Read();
                                    // Checks if the following tag is name
                                    if (reader.Name == "")
                                    {
                                        reader.Read();
                                    }
                                    if (reader.Name != "name")
                                    {
                                        throw new SpreadsheetReadWriteException("Invalid tag");
                                    }
                                    reader.Read();
                                    string cellName = reader.Value;
                                    // Checks if the detected cellName is valid
                                    if (!IsVariable(cellName))
                                    {
                                        throw new SpreadsheetReadWriteException("Invalid cell name");
                                    }
                                    reader.Read();
                                    reader.Read();
                                    if (reader.Name == "")
                                    {
                                        reader.Read();
                                    }
                                    if (reader.Name != "contents")
                                    {
                                        throw new SpreadsheetReadWriteException("Invalid tag");
                                    }
                                    reader.Read();
                                    string contents = reader.Value;
                                    try
                                    {
                                        SetContentsOfCell(cellName, contents);
                                    }
                                    catch (FormulaFormatException)
                                    {
                                        throw new SpreadsheetReadWriteException("Invalid formula");
                                    }
                                    catch (CircularException)
                                    {
                                        throw new SpreadsheetReadWriteException("Circular exception found");
                                    }
                                    break;
                            }
                            index++;
                        }
                    }
                }
                Changed = false;
                return version;
            }
            // If the file is unable to be found
            catch (System.IO.FileNotFoundException)
            {
                throw new SpreadsheetReadWriteException("File not found");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                throw new SpreadsheetReadWriteException("File not found");
            }
            catch (System.Xml.XmlException)
            {
                throw new SpreadsheetReadWriteException("Cannot read file");
            }
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>cell name goes here</name>
        /// <contents>cell contents goes here</contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(String filename)
        {
            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);
                    // Writes data for each cell
                    foreach (string cellName in nonEmptyCellNames)
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", cellName);
                        object contents = nonEmptyCells[cellName].GetContents();
                        string cellContents;
                        if (contents.GetType() == typeof(Formula))
                        {
                            cellContents = "=" + contents.ToString();
                        }
                        else if (contents.GetType() == typeof(double))
                        {
                            cellContents = contents.ToString();
                        }
                        else
                        {
                            cellContents = contents.ToString();
                        }
                        writer.WriteElementString("contents", cellContents);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    Changed = false;
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                throw new SpreadsheetReadWriteException("File path is invalid or does not exist");
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            if(name is null || !IsVariable(name))
            {
                throw new InvalidNameException();
            }
            if (nonEmptyCells.ContainsKey(name))
            {
                return nonEmptyCells[name].GetValue();
            }
            return "";
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return nonEmptyCellNames;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (name is null || !IsVariable(name))
            {
                
                throw new InvalidNameException();
            }
            else
            {
                if (nonEmptyCells.TryGetValue(Normalize(name), out Cell newCell))
                {
                    return newCell.GetContents();
                }
                // If no cell exists but the name is valid, return an empty string
                return "";
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
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<String> SetContentsOfCell(String name, String content)
        {
            if (content is null)
            {
                throw new ArgumentNullException("Cell content is null");
            }
            if (name is null || !IsVariable(name) || !IsValid(name))
            {
                throw new InvalidNameException();
            }
            IList<string> cellList;
            // If the contents are a double
            if (double.TryParse(content, out double doubleContent))
            {
                cellList = SetCellContents(name, doubleContent);
            }
            // If the contents are a formula
            else if (content.Length > 1 && content[0] == '=')
            {
                try
                {
                    cellList =  SetCellContents(name, new Formula(content.Substring(1), this.Normalize, this.IsValid));
                }
                catch(FormulaFormatException)
                {
                    throw new FormulaFormatException("String cannot be converted to Formula");
                }
                catch(CircularException)
                {
                    throw new CircularException();
                }
            }
            // If the contents are a string
            else
            {
                cellList =  SetCellContents(name, content);
            }
            if (!Changed)
            {
                Changed = true;
            }
            return cellList;
        }

        /// <summary>
        /// Private helper method passed when evaluating the formula contents of a cell. If the 
        /// formula can be evaluated, returns the solution, otherwise returns an argument exception.
        /// </summary>
        /// <param name="name"></param>
        /// <returns> the solution to the formula </returns>
        private double Lookup(string name)
        {
            Object contents = GetCellContents(name);
            // If the cell contents are a formula, returns the evaluated formula
            if (contents.GetType() == typeof(Formula))
            {
                Formula formulaContents = (Formula)contents;
                Object value = formulaContents.Evaluate(Lookup);
                if (value.GetType() != typeof(double))
                {
                    throw new ArgumentException();
                }
                return (double) value;
            }
            else if (contents.GetType() == typeof(double))
            {
                return (double)contents;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// The contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {
            if (!RemoveOldDependencies(name, number, out Cell newCell))
            {
                newCell = new Cell(number);
                nonEmptyCellNames.Add(name);
                nonEmptyCells.Add(name, newCell);
            }
            newCell.SetValue(number);
            return RecalculateValues(name);
        }

        /// <summary>
        /// The contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, string text)
        {
            if (!RemoveOldDependencies(name, text, out Cell newCell))
            {
                newCell = new Cell(text);
                nonEmptyCellNames.Add(name);
                nonEmptyCells.Add(name, newCell);
            }
            newCell.SetValue(text);
            // If the text is an empty string, removes the empty cell
            if (text == "")
            {
                nonEmptyCells.Remove(name);
                nonEmptyCellNames.Remove(name);
            }
            return RecalculateValues(name);
        }

        /// <summary>
        /// If changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula. The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            // Removes old dependencies if they exist
            bool exists = RemoveOldDependencies(name, formula, out Cell newCell, out bool hasFormula, out object originalContents);
            if (!exists)
            {
                newCell = new Cell(formula);
                nonEmptyCellNames.Add(name);
                nonEmptyCells.Add(name, newCell);
            }
            // Adds graph dependencies for new cell
            foreach (string varName in formula.GetVariables())
            {
                dependencyGraph.AddDependency(name, varName);
            }
            // Checks if the new formula creates a circular exception
            try
            {
                return RecalculateValues(name);
            }
            // If the new formula creates a circular dependency
            catch (CircularException)
            {
                // Removes new graph relationships
                foreach (string varName in formula.GetVariables())
                {
                    dependencyGraph.RemoveDependency(name, varName);
                }
                // If the original cell contents were a formula, adds the old graph relationships
                if (exists && hasFormula)
                {
                    Formula originalFormula = (Formula)originalContents;
                    foreach (string varName in originalFormula.GetVariables())
                    {
                        dependencyGraph.AddDependency(name, varName);
                    }
                    newCell.SetContents(originalFormula);
                }
                else if (exists && !hasFormula)
                {
                    newCell.SetContents(originalContents);
                }
                else
                {
                    nonEmptyCellNames.Remove(name);
                    nonEmptyCells.Remove(name);
                }
                throw new CircularException();
            }
        }

        /// <summary>
        /// Private helper method used when setting the contents of a cell to a string or a double. Overloaded version
        /// of the same method that removes two boolean parameters only used when setting the contents to a formula
        /// </summary>
        /// <param name="name"> the name of the cell </param>
        /// <param name="content"> the content of the cell </param>
        /// <param name="outCell"> the existing cell or a new cell if it does not exist </param>
        /// <returns> true if the cell already exists in the spreadsheet </returns>
        private bool RemoveOldDependencies(string name, Object content, out Cell outCell)
        {
            return RemoveOldDependencies(name, content, out outCell, out bool hasFormula, out Object oldContents);
        }

        /// <summary>
        /// Private helper method that returns a bool on whether the cell called name
        /// already exists within the spreadsheet or not. If it does exist, it checks if the cells 
        /// old contents contained a formula and removes the old dependency relationship
        /// </summary>
        /// <param name="name"> the name of the cell </param>
        /// <param name="content"> the content of the cell </param>
        /// <param name="outCell"> the existing cell or a new cell if it does not exist </param>
        /// <param name="hasFormula"> boolean on if the original contents were a formula </param>
        /// <param name="content"> the old content of the cell </param>
        /// <returns> a boolean on whether the cell already exists in the spreadsheet </returns>
        private bool RemoveOldDependencies(string name, Object content, out Cell outCell, out bool hasFormula, out Object oldContents)
        {
            bool exists = nonEmptyCells.TryGetValue(name, out Cell newCell);
            hasFormula = false;
            oldContents = content;
            if (exists)
            {
                // Checks if the old contents were a formula and removes old graph relationships
                if (newCell.GetContents().GetType() == typeof(Formula))
                {
                    hasFormula = true;
                    Formula cellFormula = (Formula)newCell.GetContents();
                    foreach (string varName in cellFormula.GetVariables())
                    {
                        dependencyGraph.RemoveDependency(name, varName);
                    }
                }
                else
                {
                    oldContents = newCell.GetContents();
                }
                newCell.SetContents(content);
            }
            outCell = newCell;
            return exists;
        }

        /// <summary>
        /// Private helper method used to acquire the list of cells to be recalculated, then 
        /// iterate through each cell and change its value
        /// </summary>
        /// <param name="name"> name of the cell </param>
        /// <returns></returns>
        private List<string> RecalculateValues(string name)
        {
            List<string> recalculateCells = GetCellsToRecalculate(name).ToList();
            // Recalculates the value for every dependent cell
            foreach (string cellName in recalculateCells)
            {
                if (nonEmptyCells.TryGetValue(cellName, out Cell recalculateCell))
                {
                    if (recalculateCell.GetContents().GetType() == typeof(Formula))
                    {
                        recalculateCell.SetValue(((Formula)recalculateCell.GetContents()).Evaluate(Lookup));
                    }
                }
            }
            return recalculateCells;
        }

        /// <summary>
        /// Private helper method to detect if a string is a variable.
        /// </summary>
        /// <param name="t"></param> next string in the formula
        /// <returns></returns> true if the string is a variable or false if not
        private bool IsVariable(String t)
        {
            return Regex.IsMatch(t, "^[a-zA-Z]+[0-9]+$");
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
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
            if (!dependencyGraph.HasDependees(name))
            {
                return new HashSet<string>();
            }
            return dependencyGraph.GetDependees(name);
        }
    }
}