using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using Spreadsheet;
using Formulas;

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
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            Assert.AreEqual("", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with number
        /// </summary>
        [TestMethod]
        public void TestGetCellContents1()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            ss.SetCellContents("A1", 2.0);
            Assert.AreEqual(2.0, ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with string
        /// </summary>
        [TestMethod]
        public void TestGetCellContents2()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            ss.SetCellContents("A1", "a");
            Assert.AreEqual("a", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with formula
        /// </summary>
        [TestMethod]
        public void TestGetCellContents3()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            ss.SetCellContents("A1", "a1 + b2");
            
            Assert.AreEqual("a1 + b2", ss.GetCellContents("A1"));
        }

        /// <summary>
        /// checks set cell contents with formula
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestGetCellContents4()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            Formula a = new Formula("a1 + b2");
            ss.SetCellContents("A1", a);

            Formula b = new Formula("A1 + b2");
            ss.SetCellContents("a1", b);

        }

    }
}
