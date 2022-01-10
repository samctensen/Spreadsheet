CHANGE LOG
(10/11/21) The GUI must have an instance of spreadsheet to track cells and relationships.

(10/12/21) Once a cell value is submitted by the user, the spreadsheet needs to recalcualte all existing cells that depend on that value.

(10/15/21) Tried to add arrow key detection to move the selection, but the panel responds by shifting the scroll bars instead.

(10/16/21) Found a solution but must modify SpreadsheetPanel. By overriding ProccessCMDKey in the spreadsheet panel class, the arrow
	keys now respond. Also added the enter key to simplify things. When the user presses the enter key on the spreadsheet panel, it will 
	now focus to the text box where the user can enter data.

(10/17/21) The spreadsheet panel enter key detection needs to be able to access elements of the spreadsheet GUI to function. By using a delegate
	I was able to pass the object to the class to then be used.

(10/18/21) My extra feautre will be a visual indicator of the dependency between cells. This is a similar feature to what Microsoft Excel has. 
	When a user enters a formula that contains other cells, the spreadsheet panel will highlight those cells in red to show the user that is 
	where the values are coming from. This is also helpful if a user mistypes the wrong cell, because it will show them exactly which cells they are calling.

(10/19/21) It has been very annoying having to deal with both SpreadsheetGUI and SpreadsheetPanel and data that is shared between them. I want to limit
	project references and public methods but its hard to.

(10/21/21) To implement the special feature, I modified the OnPaint method in the spreadsheet panel class. The spreadsheet panel class is passed cell contents 
	whenever the selection is changed. It looks through the formulas variables and paints red rectanles on their grid locations.


INSTRUCTIONS
	Use the arrow keys or mouse to select a cell.
	Once a cell is selected, press the enter key which will move you to a textbox to enter the cells contents.
	Type a string, double. or Formula and press enter one more time to put it into the spreadsheet.

NAVIGATION
	Enter key -> switch to textbox to input cell data OR after data has been typed, enter it into the spreadsheet
	Mouse -> change the selected cell
	Arrow Keys -> move the selected cell

~SPECIAL FEATURE~
	If the cell contents entered are a formula depending on the values of other cells, 
	the spreadsheet will highlight those dependent cells in red after the data has been entered or when the cell is selected agian.