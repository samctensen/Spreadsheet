using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        public static double lookup(string yuh)
        {
            return 0;
        }

        public static string toUpperCase(string yuh)
        {
            return yuh.ToUpper();
        }

        public static bool Validator1(string s)
        {
            return Char.IsLower(s[0]);
        }
        public static string Normalizer1(string s)
        {
            return s.ToLower();
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void EmptyFormulaThrows()
        {
            Formula f1 = new Formula("");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void NumberBeforeVariableThrows()
        {
            Formula f1 = new Formula("1 a");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void VariableBeforeNumberThrows()
        {
            Formula f1 = new Formula("a 1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void EndingOpeningParenthesisThrows()
        {
            Formula f1 = new Formula("1+2(");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ParenthesisMultiplicationThrows()
        {
            Formula f1 = new Formula("2(2+1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void VariableAfterClosingParenthesis()
        {
            Formula f1 = new Formula("2(2+1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ParenthesisAfterVariableThrows()
        {
            Formula f1 = new Formula("a(2+1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void UnmatchedParenthesisThrows()
        {
            Formula f1 = new Formula("(2+1) * 3)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void StartingWithClosingParenthesisThrows()
        {
            Formula f1 = new Formula(") 2+2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ClosingParenthesiAfterOperatorThrows()
        {
            Formula f1 = new Formula("(2+)2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void StartingWithOperatorThrows()
        {
            Formula f1 = new Formula("+2*7");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void EndinggWithOperatorThrows()
        {
            Formula f1 = new Formula("2*7+");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TwoOperatorsThrows()
        {
            Formula f1 = new Formula("2++6");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void InvalidTokenThrows()
        {
            Formula f1 = new Formula("2 + !");
        }

        [TestMethod]
        public void DivideByZero()
        {
            Formula f1 = new Formula("1/0");
            FormulaError awNuts = (FormulaError) f1.Evaluate(lookup);
            Assert.AreEqual(awNuts.Reason, "Cannot divide by zero");
        }

        [TestMethod]
        public void SubtractionTest()
        {
            Formula f1 = new Formula("1-1");
            Assert.AreEqual((double)f1.Evaluate(lookup), 0, 1e-9);
        }
        [TestMethod]
        public void ParenthesisSubtractionTest()
        {
            Formula f1 = new Formula("(56-1)");
            Assert.AreEqual((double)f1.Evaluate(lookup), 55, 1e-9);
        }

        [TestMethod]
        public void AdditionnTest()
        {
            Formula f1 = new Formula("1+1");
            Assert.AreEqual((double)f1.Evaluate(lookup), 2, 1e-9);
        }
        [TestMethod]
        public void ParenthesisAdditionTest()
        {
            Formula f1 = new Formula("(56+1)");
            Assert.AreEqual((double)f1.Evaluate(lookup), 57, 1e-9);
        }

        [TestMethod]
        public void MultiplicationTest()
        {
            Formula f1 = new Formula("2*123");
            Assert.AreEqual((double)f1.Evaluate(lookup), 246, 1e-9);
        }
        [TestMethod]
        public void MultiplicationParenthesisTest()
        {
            Formula f1 = new Formula("(2*123)");
            Assert.AreEqual((double)f1.Evaluate(lookup), 246, 1e-9);
        }

        [TestMethod]
        public void DivisionTest()
        {
            Formula f1 = new Formula("8/2");
            Assert.AreEqual((double)f1.Evaluate(lookup), 4, 1e-9);
        }
        [TestMethod]
        public void DivisionParenthesisTest()
        {
            Formula f1 = new Formula("(1/2)");
            Assert.AreEqual((double)f1.Evaluate(lookup), 0.5, 1e-9);
        }

        [TestMethod]
        public void VariableEvaluator()
        {
            Formula f1 = new Formula("(a + 1)");
            Assert.AreEqual((double)f1.Evaluate(s => 1), 2, 1e-9);
        }

        [TestMethod]
        public void TestEquals()
        {
            Formula f1 = new Formula("(a + 1)");
            Formula f2 = new Formula("(a + 1.000000)");
            Assert.IsTrue(f1.Equals(f2));
        }

        [TestMethod]
        public void TestEqualsOperators()
        {
            Formula f1 = new Formula("(a + 1)");
            Formula f2 = new Formula("(a + 1.000000)");
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod]
        public void TestDoesNotEqualsOperators()
        {
            Formula f1 = new Formula("(a + 1)");
            Formula f2 = new Formula("(a + 1.2)");
            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void TestEqualsNull()
        {
            Formula f1 = new Formula("(a + 1)");
            Assert.IsFalse(f1.Equals(null));
        }

        [TestMethod]
        public void TestEqualsString()
        {
            Formula f1 = new Formula("(a + 1)");
            Assert.IsFalse(f1.Equals("a"));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Formula f1 = new Formula("(a + 1)");
            Formula f2 = new Formula("(a + 1.00000000)");
            Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
        }

        [TestMethod]
        public void TestGetVariablese()
        {
            Formula f1 = new Formula("(a + 1)");
            IEnumerator<string> e2 = f1.GetVariables().GetEnumerator();
            e2.MoveNext();
            Assert.AreEqual(e2.Current, "a");
        }

        [TestMethod]
        public void TestToString()
        {
            Formula f1 = new Formula("(a + 1)");
            Formula f2 = new Formula("(a + 1.00000000)");
            Assert.AreEqual(f1.ToString(), f2.ToString());
        }
    }
}
