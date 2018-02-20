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
        /// checks set cell contents with formula circle
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

        /// <summary>
        /// checks set cell contents with deeper formula circle
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void TestGetCellContents5()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();

            Formula b2 = new Formula("b1 + b3");
            ss.SetCellContents("b2", b2);

            Formula a = new Formula("b2 + 2");
            ss.SetCellContents("A1", a);

            Formula b = new Formula("A1 + c2");
            ss.SetCellContents("a1", b);

            Formula c = new Formula("a1");
            ss.SetCellContents("b1", c);

        }

    }
}
