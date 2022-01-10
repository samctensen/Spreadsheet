using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {

        [TestMethod]
        public void TestSimpleChanged()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            Assert.IsFalse(spreadsheet.Changed);
            spreadsheet.SetContentsOfCell("x1", "1");
            Assert.IsTrue(spreadsheet.Changed);
        }

        [TestMethod]
        public void TestReadFileChanged()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.SetContentsOfCell("x1", "4");
            Assert.IsTrue(spreadsheet.Changed);
            spreadsheet.Save("xmlfile.txt");
            Assert.IsFalse(spreadsheet.Changed);

            Spreadsheet spreadsheet2 = new Spreadsheet("xmlfile.txt", s => true, s => s, "default");
            Assert.IsFalse(spreadsheet2.Changed);
            spreadsheet2.SetContentsOfCell("y2", "3");
            Assert.IsTrue(spreadsheet2.Changed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidConstructor()
        {
            Spreadsheet spreadsheet = new Spreadsheet(s => false, s => s, "1");
            spreadsheet.SetContentsOfCell("x1", "3");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadTagOutOfOrder()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteStartElement("cell");
                writer.WriteEndElement();
                writer.WriteStartElement("name");
                writer.WriteAttributeString("version", "1.0");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, null);
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadInvalidTag()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("badtag");
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadInvalidSpreadsheetTagOrder()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("name");
                writer.WriteStartElement("spreadsheet");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadInvalidNameTagOrderWithinCell()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");
                writer.WriteStartElement("cell");
                writer.WriteElementString("spreadsheet", "name");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadInvalidContentsTagOrderWithinCell()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "x1");
                writer.WriteElementString("spreadsheet", "2");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadInvalidFormula()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "x1");
                writer.WriteElementString("contents", "=nuts++");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadCircularFormulas()
        {
            using (XmlWriter writer = XmlWriter.Create("test.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");
                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "x1");
                writer.WriteElementString("contents", "=2 + x1");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            Spreadsheet spreadsheet2 = new Spreadsheet("test.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadMismatchingVersions()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.SetContentsOfCell("x1", "4");
            Assert.IsTrue(spreadsheet.Changed);
            spreadsheet.Save("xmlfile.txt");

            Spreadsheet spreadsheet2 = new Spreadsheet("xmlfile.txt", s => true, s => s, "1.0");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadNonExistentFile()
        {
            Spreadsheet spreadsheet = new Spreadsheet("xmlfileas.txt", s => true, s => s, "default");
        }

        [TestMethod]
        public void TestSaveFormula()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "=2+4");
            spreadsheet.Save("xmlfile.txt");

            Spreadsheet spreadsheet2 = new Spreadsheet("xmlfile.txt", s => true, s => s, "default");
            Assert.AreEqual((double)spreadsheet2.GetCellValue("y2"), 6, 1e-9);
        }

        [TestMethod]
        public void TestSaveString()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "nuts");
            spreadsheet.Save("xmlfile.txt");

            Spreadsheet spreadsheet2 = new Spreadsheet("xmlfile.txt", s => true, s => s, "default");
            Assert.AreEqual(spreadsheet2.GetCellValue("y2"), "nuts");
        }

        [TestMethod]
        public void TestSaveDouble()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "1");
            spreadsheet.Save("xmlfile.txt");

            Spreadsheet spreadsheet2 = new Spreadsheet("xmlfile.txt", s => true, s => s, "default");
            Assert.AreEqual((double)spreadsheet2.GetCellValue("y2"), 1, 1e-9);
        }

        [TestMethod]
        public void TestSaveXML()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.SetContentsOfCell("x1", "4");
            spreadsheet.Save("xmlfile.txt");
        }

       [TestMethod]
       [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSaveBadFilePath()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.Save("/some/nonsense/path.xml");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestNullGetValue()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellValue(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidVariableGetValue()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellValue("1a");
        }

        [TestMethod]
        public void GetNamesOfNoEmptyCells()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            Assert.IsFalse(spreadsheet.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetNullCellContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestInvalidCellNameContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.GetCellContents("0McChicken");
        }

        [TestMethod]
        public void TestValidExistingCellContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("a1", "nuts");
            Assert.AreEqual(spreadsheet.GetCellContents("a1"), "nuts");
        }

        [TestMethod]
        public void TestExistingCellChangedContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("a1", "nuts");
            Assert.AreEqual(spreadsheet.GetCellContents("a1"), "nuts");
            double num = 6.0;
            string numToString = num.ToString();
            spreadsheet.SetContentsOfCell("a1", numToString);
            Assert.AreEqual(spreadsheet.GetCellContents("a1"), 6.0);
        }

        [TestMethod]
        public void TestValidNonExistingCellContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            Assert.AreEqual(spreadsheet.GetCellContents("a1"), "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetNullContents()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y1", null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullCellDouble()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            double num = 6.0;
            string numToString = num.ToString();
            spreadsheet.SetContentsOfCell(null, numToString);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidCellDouble()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            double num = 6.0;
            string numToString = num.ToString();
            spreadsheet.SetContentsOfCell("0McChicken", numToString);
        }

        [TestMethod]
        public void TestSetNewCellDouble()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            double num = 6.0;
            string numToString = num.ToString();
            IList<string> list = spreadsheet.SetContentsOfCell("x1", numToString);
            Assert.AreEqual(list[0], "x1");
            Assert.AreEqual(list.Count, 1);
        }

        [TestMethod]
        public void TestSetFormulaToDouble()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            IList<string> list = spreadsheet.SetContentsOfCell("x1", krabbyPatty);
            Assert.AreEqual(list[0], "x1");
            double num = 6.0;
            string numToString = num.ToString();
            IList<string> list2 = spreadsheet.SetContentsOfCell("x1", numToString);
            Assert.AreEqual(list2[0], "x1");
            Assert.AreEqual(list2.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullCellString()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell(null, "nuts");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidCellString()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("0McChicken", "nuts");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestSetInvalidFormula()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "=bad ++ formula");
        }

        [TestMethod]
        public void TestSetNewCellString()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            IList<string> list = spreadsheet.SetContentsOfCell("x1", "nuts");
            Assert.AreEqual(list[0], "x1");
            Assert.AreEqual(list.Count, 1);
        }

        [TestMethod]
        public void TestSetEmptyString()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "");
            Assert.AreEqual(spreadsheet.GetCellContents("x1"), "");
        }

        [TestMethod]
        public void TestSetFormulaToString()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            IList<string> list = spreadsheet.SetContentsOfCell("x1", krabbyPatty);
            Assert.AreEqual(list[0], "x1");
            IList<string> list2 = spreadsheet.SetContentsOfCell("x1", "nuts");
            Assert.AreEqual(list2[0], "x1");
            Assert.AreEqual(list2.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullCellFormula()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell(null, krabbyPatty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidCellFormula()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("0McChicken", krabbyPatty);
        }

        [TestMethod]
        public void TestSetFormulaToFormula()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            IList<string> list = spreadsheet.SetContentsOfCell("x1", krabbyPatty);
            Assert.AreEqual(list[0], "x1");
            string chumBucket = "=2+2";
            IList<string> list2 = spreadsheet.SetContentsOfCell("x1", chumBucket);
            Assert.AreEqual(list2[0], "x1");
            Assert.AreEqual(list2.Count, 1);
        }

        [TestMethod]
        public void EvaluateFormulaWithMissingVariable()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "=1/0");
            spreadsheet.SetContentsOfCell("y2", "=2+x1");
            Assert.AreEqual(spreadsheet.GetCellValue("y2").GetType(), typeof(FormulaError));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularException()
        {
            string krabbyPatty = "=2+y2";
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", krabbyPatty);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestReplaceFormulaRelationships()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "=2+4");
            spreadsheet.SetContentsOfCell("y2", "=1+x1");
            spreadsheet.SetContentsOfCell("y2", "=y2");
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestReplaceContentAfterCircularException()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.SetContentsOfCell("y2", "=y2");
        }

        [TestMethod]
        public void TestRecalculateCellValue()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "2");
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            spreadsheet.SetContentsOfCell("y2", "3");
            Assert.AreEqual((double) spreadsheet.GetCellValue("x1"), 4, 1e-9);
        }

        [TestMethod]
        public void TestGetValueWithStringVariable ()
        {
            
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "nuts");
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            Assert.AreEqual(spreadsheet.GetCellValue("x1").GetType(), typeof(FormulaError));
        }

        [TestMethod]
        public void TestGetValueWithNoVariable()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            Assert.AreEqual(spreadsheet.GetCellValue("x1").GetType(), typeof(FormulaError));
        }

        [TestMethod]
        public void TestStringToDoubleRecalculateFormula()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "nuts");
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            Assert.AreEqual(spreadsheet.GetCellValue("x1").GetType(), typeof(FormulaError));
            spreadsheet.SetContentsOfCell("y2", "1");
            Assert.AreEqual((double)spreadsheet.GetCellValue("x1"), 2, 1e-9);
        }

        [TestMethod]
        public void TestDoubleToFormulaValue()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("y2", "6.0");
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            Assert.AreEqual((double)spreadsheet.GetCellValue("x1"), 7, 1e-9);
            spreadsheet.SetContentsOfCell("y2", "=2 + 10");
            Assert.AreEqual((double)spreadsheet.GetCellValue("x1"), 13, 1e-9);
        }

        [TestMethod]
        public void RecalculateStringFormulaValue()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("x1", "=y2 + 1");
            Assert.AreEqual(spreadsheet.GetCellValue("x1").GetType(), typeof(FormulaError));
            spreadsheet.SetContentsOfCell("y2", "nuts");
            Assert.AreEqual(spreadsheet.GetCellValue("x1").GetType(), typeof(FormulaError));
        }

        [TestMethod]
        public void SaveAndReadStressTest()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            for (int index = 0; index < 100; index++)
            {
                spreadsheet.SetContentsOfCell("X" + index, index.ToString());
            }
            spreadsheet.Save("stresstest.txt");
            Spreadsheet spreadsheet2 = new Spreadsheet("stresstest.txt", s => true, s => s, "default");
            for (int index = 0; index < 100; index++)
            {
                Assert.AreEqual((double)spreadsheet2.GetCellValue("X" + index), index, 1e-9);
            }
        }

        [TestMethod]
        public void EvaluateFormulaStressTest()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.SetContentsOfCell("X0", "1");
            for (int index = 1; index < 100; index++)
            {
                spreadsheet.SetContentsOfCell("X" + index, "=X" + (index - 1) + " + 1");
            }
            for (int index = 0; index < 100; index++)
            {
                Assert.AreEqual((double)spreadsheet.GetCellValue("X" + index), index + 1, 1e-9);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void EvaluateFormulaCircularException()
        {
            Spreadsheet spreadsheet = new Spreadsheet();
            int max = 100;
            spreadsheet.SetContentsOfCell("X0", "1");
            for (int index = 1; index < max; index++)
            {
                spreadsheet.SetContentsOfCell("X" + index, "=X" + (index - 1) + " + 1");
            }
            for (int index = 0; index < max; index++)
            {
                Assert.AreEqual((double)spreadsheet.GetCellValue("X" + index), index + 1, 1e-9);
            }
            spreadsheet.SetContentsOfCell("X0", "=X" + (max - 1));
        }
    }
}