// Written by Joe Zachary for CS 3500, January 2017.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formulas;

namespace FormulaTestCases
{
    /// <summary>
    /// These test cases are in no sense comprehensive!  They are intended to show you how
    /// client code can make use of the Formula class, and to show you how to create your
    /// own (which we strongly recommend).  To run them, pull down the Test menu and do
    /// Run > All Tests.
    /// </summary>
    [TestClass]
    public class UnitTests
    {
        /// <summary>
        /// This tests that a syntactically incorrect parameter to Formula results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct1()
        {
            Formula f = new Formula("_");
        }

        /// <summary>
        /// This is another syntax error
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct2()
        {
            Formula f = new Formula("2++3");
        }

        /// <summary>
        /// Another syntax error.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct3()
        {
            Formula f = new Formula("2 3");
        }

        /// <summary>
        /// Testing for ToString()
        /// </summary>
        [TestMethod]
        public void TestToString()
        {
            Formula f1 = new Formula("2 + 3");
            Formula f2 = new Formula(f1.ToString(), s => s, s => true);
            Assert.AreEqual(f1.Evaluate(s => 0), f2.Evaluate(s => 0));
        }

        /// <summary>
        /// Makes sure that "2+3" evaluates to 5.  Since the Formula
        /// contains no variables, the delegate passed in as the
        /// parameter doesn't matter.  We are passing in one that
        /// maps all variables to zero.
        /// </summary>
        [TestMethod]
        public void Evaluate1()
        {
            Formula f = new Formula("2+3");
            Assert.AreEqual(f.Evaluate(v => 0), 5.0, 1e-6);
        }

        /// <summary>
        /// The Formula consists of a single variable (x5).  The value of
        /// the Formula depends on the value of x5, which is determined by
        /// the delegate passed to Evaluate.  Since this delegate maps all
        /// variables to 22.5, the return value should be 22.5.
        /// </summary>
        [TestMethod]
        public void Evaluate2()
        {
            Formula f = new Formula("x5");
            Assert.AreEqual(f.Evaluate(v => 22.5), 22.5, 1e-6);
        }

        /// <summary>
        /// Here, the delegate passed to Evaluate always throws a
        /// UndefinedVariableException (meaning that no variables have
        /// values).  The test case checks that the result of
        /// evaluating the Formula is a FormulaEvaluationException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaEvaluationException))]
        public void Evaluate3()
        {
            Formula f = new Formula("x + y");
            f.Evaluate(v => { throw new UndefinedVariableException(v); });
        }

        /// <summary>
        /// The delegate passed to Evaluate is defined below.  We check
        /// that evaluating the formula returns in 10.
        /// </summary>
        [TestMethod]
        public void Evaluate4()
        {
            Formula f = new Formula("x + y");
            Assert.AreEqual(f.Evaluate(Lookup4), 10.0, 1e-6);
        }

        /// <summary>
        /// This uses one of each kind of token.
        /// </summary>
        [TestMethod]
        public void Evaluate5()
        {
            Formula f = new Formula("(x + y) * (z / x) * 1.0");
            Assert.AreEqual(f.Evaluate(Lookup4), 20.0, 1e-6);
        }

        /// <summary>
        /// Testing validation and normalizer
        /// </summary>
        [TestMethod]
        public void Evaluate6()
        {
            Formula f = new Formula("3 + x", s => s.ToUpper(), s => s.Equals("X"));
            Assert.AreEqual(f.Evaluate(s => 3), 6);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Evaluate7()
        {
            Formula f = new Formula("3 + x", s => s.ToUpper(), s => s.Equals("O"));
        }

        /// <summary>
        /// Test to see if ToString works as expected with a normalizer
        /// </summary>
        [TestMethod]
        public void Evaluate8()
        {
            Formula f = new Formula("(x+4) / (x+y)", s => s.ToUpper(), s => true);
            Assert.AreEqual("(X+4)/(X+Y)", f.ToString());
        }

        [TestMethod]
        public void Evaluate9()
        {
            Formula f = new Formula("(x+4) / (x+y)", s => s.ToUpper(), s => true);
            foreach (var VARIABLE in f.GetVariables())
            {
                Assert.IsFalse(VARIABLE.Equals("x") || VARIABLE.Equals("y"));
                Assert.IsTrue(VARIABLE.Equals("X") || VARIABLE.Equals("Y"));
            }
        }

        // The following tests are to to test that ArgumentNullExceptions are thrown
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull1()
        {
            Formula f = new Formula(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull2()
        {
            Formula f = new Formula("2 + 3", null, s => false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull3()
        {
            Formula f = new Formula(null, null, s => false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull4()
        {
            Formula f = new Formula(null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNull5()
        {
            Formula f = new Formula("2 + 3", s => s, s => false);
            f.Evaluate(null);
        }

        /// <summary>
        /// A Lookup method that maps x to 4.0, y to 6.0, and z to 8.0.
        /// All other variables result in an UndefinedVariableException.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double Lookup4(String v)
        {
            switch (v)
            {
                case "x": return 4.0;
                case "y": return 6.0;
                case "z": return 8.0;
                default: throw new UndefinedVariableException(v);
            }
        }
    }
}