// Written by Sam Christensen
// October 2021
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SS
{
    /// <summary>
    /// Form1 class for the spreadsheetGUI. Builds a spreadsheet user interface and computes
    /// multicell formulas. Written by Sam Christensen, October 2021
    /// </summary>
    public partial class Form1 : Form
    {
        // Spreadsheet instance variables used to store cells and their relationships
        private Spreadsheet spreadsheet;
        //the filename where the spreadsheet is saved
        private string filename;

        /// <summary>
        /// Form1 Constructor which does inistial setup and 
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            // Sets spreadsheet, allowing for lowercase and capital letter cell names and saves the file with a version ps6
            spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
            // Passes the textbox to the SpreadsheetPanel
            spreadsheetPanel.SetTextBox(GetTextBox);
            // Sets the main focus to the panel (not the textbox)
            ActiveControl = spreadsheetPanel;
        }

        /// <summary>
        /// Helper method used as a basic delegate to pass the cellEntryTextBox to the Spreadsheet Panel.
        /// </summary>
        /// <returns>the cellEntryTextBox text box</returns>
        private System.Windows.Forms.TextBox GetTextBox()
        {
            return cellEntryTextBox;
        }

        // Every time the selection changes, this method is called with the
        // Spreadsheet as its parameter.  We display the current time in the cell.
        /// <summary>
        /// SelectionChanged event handler for the Spreadsheet panel. Refreshes the main GUI hud labels when the selection is changed. 
        /// </summary>
        /// <param name="ss"></param>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            ss.GetSelection(out col, out row);
            string cellName = getCellName(col, row);
            // Refreshes the current cells value for the dependee highlights
            spreadsheetPanel.UpdateCellContents(cellName, spreadsheet);
            // Refreshes the cell name label
            currentCellLabel.Text = "Current Cell: " + cellName;
            if (spreadsheetPanel.GetValue(col, row, out string value))
            {
                // Refreshes cell value label
                object cellValue = spreadsheet.GetCellValue(cellName);
                if (cellValue is FormulaError)
                {
                    currentCellValueLabel.Text = "Cell Value: " + ((FormulaError)cellValue).Reason;
                }
                else
                {
                    currentCellValueLabel.Text = "Cell Value: " + cellValue.ToString();
                }
                object cellContents = spreadsheet.GetCellContents(cellName);
                if (cellContents is Formula)
                {
                    cellEntryTextBox.Text = "=" + cellContents.ToString(); ;
                }
                else
                {
                    cellEntryTextBox.Text = cellContents.ToString(); ;
                }
            }
            else
            {
                cellEntryTextBox.Text = "";
            }
            spreadsheetPanel.Focus();
        }

        /// <summary>
        /// Returns a string of the cell name given the cells row and column
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns> a combined string of the cell name </returns>
        private string getCellName(int col, int row)
        {
            return Convert.ToChar(col + 65).ToString() + (row + 1);
        }

        /// <summary>
        /// Converts a string name to coordinates
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="col">column coordinate from the cell name</param>
        /// <param name="row">row coordinate from the cell name</param>
        private void getCellCoordinates(string cellName, out int col, out int row)
        {
            col = cellName[0] - 65;
            if (int.TryParse(cellName.Substring(1), out row))
            {
                row = row - 1;
            }
        }

        /// <summary>
        /// Click event handler for the new menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tell the application context to run the form on the same
            // thread as the other forms.
            DemoApplicationContext.getAppContext().RunForm(new Form1());
        }

        /// <summary>
        /// Click event handler for the save menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Spreadsheet (*.sprd)|*.sprd|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".sprd";
            var saveResult = saveFileDialog.ShowDialog();
            if (saveResult == DialogResult.OK)
            {
                spreadsheet.Save(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Click event handler for the close menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// FormClosing event handler for the close menu button or in the top corner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormIsClosing(object sender, FormClosingEventArgs e)
        {
            if (spreadsheet.Changed)
            {
                DialogResult result = MessageBox.Show("Save changes to the document before closing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    DialogResult saveResult = SaveFileMenu();
                    // Stops the form closing if the user cancels
                    if (saveResult == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Click event handler for the open menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Spreadsheet Files(*.sprd)|*.sprd| All files(*.*) | *.*";
            openFileDialog.DefaultExt = ".sprd";
            var openResult = openFileDialog.ShowDialog();
            if (openResult == DialogResult.OK)
            {
                DialogResult result = DialogResult.No;
                string newFileName = openFileDialog.FileName;
                if (newFileName != filename)
                {
                    if (spreadsheet.Changed)
                    {
                        result = MessageBox.Show("Save changes to the current document before overwriting?", "Overwrite Document?", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            DialogResult saveResult = SaveFileMenu();
                        }
                    }
                }
                if (newFileName == filename || result != DialogResult.Cancel)
                {
                    spreadsheet.GetSavedVersion(newFileName);
                    if (openFileDialog.FilterIndex == 1)
                    {
                        if (!Regex.IsMatch(newFileName, ".sprd$"))
                        {
                            newFileName = newFileName + ".sprd";
                        }
                    }
                    filename = newFileName;
                    recalculateCells(spreadsheet.GetNamesOfAllNonemptyCells().ToList());
                }

            }
        }

        /// <summary>
        /// Private helper method that generates the save file dialog when the user has unsaved changes or is saving their file
        /// </summary>
        /// <param name="e"></param>
        private DialogResult SaveFileMenu()
        {
            saveFileDialog.Filter = "Spreadsheet Files(*.sprd)|*.sprd| All files(*.*) | *.*";
            saveFileDialog.DefaultExt = ".sprd";
            var saveResult = saveFileDialog.ShowDialog();
            if (saveResult == DialogResult.OK)
            {
                string newFileName = saveFileDialog.FileName;
                if (saveFileDialog.FilterIndex == 1)
                {
                    if (!Regex.IsMatch(newFileName, ".sprd$"))
                    {
                        newFileName = newFileName + ".sprd";
                    }
                }
                filename = newFileName;
                spreadsheet.Save(newFileName);
            }
            return saveResult;
        }

        /// <summary>
        /// Click event handler for the help menu button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Opens a quick message box to show basic instructions
            var box = MessageBox.Show("Use the ARROW KEYS and MOUSE to move the cell selection. " +
                "\r\n \r\n To enter a cell contents press the ENTER key or CLICK the textbox above and start typing. " +
                "\r\n \r\n After a cells contents have been entered press the ENTER key again to set the cell contents." +
                "\r\n \r\n If the cell data entered is a Formula that uses other cells, the spreadsheet will highlight in red the dependents of that cell.", "Help Menu", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Handler for when a key is pressed from the text box for entering cell data. 
        /// Takes the text from the textbox and tries to convert it into a cell value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellSelectEnterKey_pressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                spreadsheetPanel.GetSelection(out int col, out int row);
                string cellName = getCellName(col, row);
                if (cellEntryTextBox.Text != null)
                {
                    // Puts the data into the spreadsheet
                    try
                    {
                        recalculateCells(spreadsheet.SetContentsOfCell(cellName, cellEntryTextBox.Text));
                        currentCellValueLabel.Text = "Cell Value: " + spreadsheet.GetCellValue(cellName).ToString();
                    }
                    // Catches if the formula is invalid or creates a circular exception
                    catch (CircularException)
                    {
                        MessageBox.Show("Circular Formula: the formula is dependent on itself", "Circular Formula", MessageBoxButtons.OK);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid Formula: please enter a correct formula", "Invalid Formula", MessageBoxButtons.OK);
                    }
                }
                // Updates cell contents and display panel
                spreadsheetPanel.UpdateCellContents(cellName, spreadsheet);
                displaySelection(spreadsheetPanel);
                // Sets the focus away from the textbox
                ActiveControl = spreadsheetPanel;
            }
        }

        /// <summary>
        /// Private helper method which iterates through the list of dependent cells
        /// and recalculates their values as shown in the spreadsheet.
        /// </summary>
        /// <param name="cellList"> list of cells needing to be recalculated </param>
        private void recalculateCells(IList<string> cellList)
        {
            foreach (string cell in cellList)
            {
                getCellCoordinates(cell, out int col, out int row);
                object cellValue = spreadsheet.GetCellValue(cell);
                if (cellValue.GetType() == typeof(FormulaError))
                {
                    spreadsheetPanel.SetValue(col, row, ((FormulaError)cellValue).Reason);
                }
                else
                {
                    spreadsheetPanel.SetValue(col, row, cellValue.ToString());
                }
            }
        }
    }
}
