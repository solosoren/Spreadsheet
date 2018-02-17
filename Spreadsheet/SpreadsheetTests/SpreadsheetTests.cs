using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using Spreadsheet;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            AbstractSpreadsheet ss = new Spreadsheet.Spreadsheet();
            Assert.AreEqual("", ss.GetCellContents("A1"));
        }
    }
}
