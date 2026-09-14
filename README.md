# Spreadsheet

A C# desktop spreadsheet with formula evaluation, dependency tracking, and a Windows Forms interface.

Originally developed during the first half of fall semester 2021. Uploaded to GitHub in January 2022. The [original development notes](README.txt) include entries from October 11–21, 2021.

## Run

Open `Spreadsheet.sln` in Visual Studio on Windows, restore dependencies, and select `SpreadsheetGUI` as the startup project. The original solution combines .NET Framework 4.7.2, .NET Standard 2.0, and .NET Core 3.1 projects; use the corresponding targeting tools.

Run the test projects through Visual Studio's Test Explorer. Use the mouse or arrow keys to select a cell, then Enter to edit its contents. Formulas can reference other cells, and the UI highlights those dependencies.

## Project

- `Spreadsheet/`, `Formula/`, `FormulaEvaluator/`, `DependencyGraph/` — spreadsheet logic.
- `SpreadsheetGUI/`, `SpreadsheetPanel/` — desktop interface.
- `*Tests/` — unit and course grading tests.
- `exercises/tip-calculator/` — a separate earlier exercise.

The duplicate upload was consolidated without changing the C# source. Original notes and course exercises remain available.
