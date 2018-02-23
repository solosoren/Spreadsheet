using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using Formulas;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// tests empty spreadsheet GetCellContents()
        /// </summary>
        [TestMethod]
        public void TestGetCellContentsEmptySpreadsheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.AreEqual("", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with number
        /// </summary>
        [TestMethod]
        public void TestGetCellContents1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "2.0");
            Assert.AreEqual(2.0, ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with string
        /// </summary>
        [TestMethod]
        public void TestGetCellContents2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "a");
            Assert.AreEqual("a", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with formula
        /// </summary>
        [TestMethod]
        public void TestGetCellContents3()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "a1 + b2");
            
            Assert.AreEqual("a1 + b2", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with formula circle
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestGetCellContents4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            
            ss.SetContentsOfCell("A1", "=(a1 + b2)");

            ss.SetContentsOfCell("a1", "=(A1 + b2)");

        }

        /// <summary>
        /// checks save
        /// </summary>
        [TestMethod]        
        public void TestSaveContents1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();

            ss.SetContentsOfCell("b2", "5");

            ss.SetContentsOfCell("A1", "=b2 + 2");

            ss.SetContentsOfCell("a2", "=A1 + b2");

            ss.SetContentsOfCell("b1", "=a2 - 10");

            StreamWriter writer = File.CreateText("C:\\Users\\Soren\\source\\repos\\u0967837\\Spreadsheet\\Spreadsheet\\SampleSavedSpreadsheet.xml");
            ss.Save(writer);
        }

        /// <summary>
        /// Tests set spreadsheet from xml
        /// </summary>
        [TestMethod]
        public void TestSpreadsheetFromXML()
        {
            StreamReader reader = File.OpenText("C:\\Users\\Soren\\source\\repos\\u0967837\\Spreadsheet\\Spreadsheet\\SampleSavedSpreadsheet.xml");
            Regex regex = new Regex(@"[a-zA-Z]+[0-9]+");
            AbstractSpreadsheet ss = new Spreadsheet(reader, regex);
        }

    }
}
