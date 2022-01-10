// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

//  (Version 1.2) Changed the definition of equality with regards
//                to numeric token
//
// (Sam Christensen)
//  (Version 1.3) Added logic to evaluate methods and constructor. Equal methods compare the string of the formula

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private Stack<double> values;
        private Stack<string> operators;
        private IEnumerable<string> tokens;
        private Func<string, string> normalize;
        private Func<string, bool> isValid;
        private StringBuilder formulaBuilder;
        private HashSet<string> variableSet;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) : this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            tokens = GetTokens(formula);
            values = new Stack<double>();
            operators = new Stack<string>();
            this.normalize = normalize;
            this.isValid = isValid;
            formulaBuilder = new StringBuilder();
            variableSet = new HashSet<string>();
            FormulaScanner();
        }

        /// <summary>
        /// Private helper method to simplify the toString and getVariable methods. Builds a string
        /// formula and adds to set of variables
        /// </summary>
        private void FormulaScanner()
        {
            int rightParenthesis = 0;
            int leftParenthesis = 0;
            int index = 0;
            string previousToken = "";
            if (tokens.Count() < 1)
            {
                throw new FormulaFormatException("Formula must have at least one token. Add more tokens to the formula");
            }
            foreach (string s in tokens)
            {
                String t = s.Trim();
                // If the next string is a double
                if (double.TryParse(t, out double num))
                {
                    String doubletoString = num.ToString();
                    formulaBuilder.Append(doubletoString);
                    if (index > 0 && (double.TryParse(previousToken, out double prev) || IsVariable(previousToken) || previousToken.Equals(")")))
                    {
                        throw new FormulaFormatException("Extra number following a number or variable. Add an operator or parenthesis inbetween");
                    }
                }
                // If the next string is a variable
                else if (IsVariable(t))
                {
                    string normalized = normalize(t);
                    if (!variableSet.Contains(normalized) && isValid(normalized))
                    {
                        variableSet.Add(normalized);
                    }
                    if (!isValid(normalized))
                    {
                        throw new FormulaFormatException("Variable not valid");
                    }
                    formulaBuilder.Append(normalized);
                    if (index > 0 && (double.TryParse(previousToken, out double prev) || IsVariable(previousToken) || previousToken.Equals(")")))
                    {
                        throw new FormulaFormatException("Extra variable following a number or variable. Add an operator or parenthesis inbetween");
                    }
                }
                // If the next token is a (
                else if (t == "(")
                {
                    formulaBuilder.Append(t);
                    leftParenthesis++;
                    if (index == tokens.Count() - 1)
                    {
                        throw new FormulaFormatException("Incorrect ending token. Add a closing parenthesis after.");
                    }
                    if (index > 0 && (double.TryParse(previousToken, out double prev) || IsVariable(previousToken) || previousToken.Equals(")")))
                    {
                        throw new FormulaFormatException("Extra opening parenthesis following a number or variable. Add an operator or parenthesis inbetween");
                    }
                }
                // If the next token is a )
                else if (t == ")")
                {
                    formulaBuilder.Append(t);
                    rightParenthesis++;
                    if (index == 0)
                    {
                        throw new FormulaFormatException("Incorrect starting token. The first token of an expression must be a number, a variable, or an opening par");
                    }
                    if (IsOperator(previousToken) || previousToken.Equals("("))
                    {
                        throw new FormulaFormatException("Cannot have a closing parenthesis following an operator or (. Add a variable or number to finish the expression");
                    }
                }
                else if (IsOperator(t))
                {
                    formulaBuilder.Append(t);
                    if (index == 0)
                    {
                        throw new FormulaFormatException("Incorrect starting token. The first token of an expression must be a number, a variable, or an opening par");
                    }
                    if (index == tokens.Count() - 1)
                    {
                        throw new FormulaFormatException("Incorrect ending token. The last token of an expression must be a number, a variable, or a closing parenthesis.");
                    }
                    if (IsOperator(previousToken) || previousToken.Equals("("))
                    {
                        throw new FormulaFormatException("Multiple operators error. Add a number or variable between operators.");
                    }
                }
                else
                {
                    throw new FormulaFormatException("Invalid token. Tokens can only be numbers, variables, (, ), +, -, *, /");
                }
                if (index == tokens.Count() - 1)
                {
                    if (rightParenthesis != leftParenthesis)
                    {
                        throw new FormulaFormatException("Missing parenthesis. Make sure each opening parenthesis has a closing");
                    }
                }
                previousToken = s;
                index++;
            }
        }

        /// <summary>
        /// Helper method to check if a string is a valid operator
        /// </summary>
        /// <param name="s"></param>
        /// <returns>a bool on whether the string is a valid operator or not </returns>
        private bool IsOperator(string s)
        {
            if (s == "+" || s == "-" || s == "*" || s == "/")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            operators = new Stack<string>();
            values = new Stack<double>();
            foreach (string s in tokens)
            {
                String t = s.Trim();
                // If the next string in the list is an int
                if (double.TryParse(t, out double num))
                {
                    try
                    {
                        DoubleHandler(num);
                    }
                    catch(ArgumentException)
                    {
                        return new FormulaError("Cannot divide by zero");
                    }
                }
                // If the next string is a variable
                else if (IsVariable(t))
                {
                    string normalized = normalize(t);
                    if (isValid(normalized))
                    {
                        try
                        {
                            DoubleHandler(lookup(normalized));
                        }
                        catch (ArgumentException)
                        {
                            return new FormulaError("Variable value not found.");
                        }
                    }
                    else
                    {
                        return new FormulaError("Variable is not valid.");
                    }
                }
                // If the next string is + or -
                else if (t.Equals("+") || t.Equals("-"))
                {
                    AdditionHandler(t);
                }
                // If the next string is * or /
                else if (t.Equals("*") || t.Equals("/"))
                {
                    operators.Push(t);
                }
                // If the next string is (
                else if (t.Equals("("))
                {
                    operators.Push(t);
                }
                // If the next string is )
                else if (t.Equals(")"))
                {
                    object divideByZero = ClosedParenthesisHandler();
                    if (divideByZero != null)
                    {
                        return divideByZero;
                    }
                }
            }
            // Return the final value
            if (operators.Count == 0 && values.Count == 1)
            {
                return values.Pop();
            }
            if (operators.Count == 1 && values.Count == 2)
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                // Returns the sum of the final two values
                if (operators.Peek().Equals("+"))
                {
                    return value2 + value1;
                }
                // Returns the difference of the final two values
                else if (operators.Peek().Equals("-"))
                {
                    return value2 - value1;
                }
            }
            return new FormulaError();
        }

        /// <summary>
        /// Private helper method used when an integer is found in the formula. Simplifies 
        /// the Evaluate function. Checks for correct operators in stack then either adds
        /// the product of two numbers or just pushes the number on the stack. Also called 
        /// after a variable has been converted to an integer.
        /// </summary>
        /// <param name="num"></param> the integer pulled from the formula
        private object DoubleHandler(double num)
        {
            // If top operator is *, multiply top value and integer
            if (operators.Count > 0 && operators.Peek().Equals("*"))
            {
                operators.Pop();
                double result = values.Pop() * num;
                values.Push(result);
            }
            // If top operator is /, divide top value and integer
            else if (operators.Count > 0 && operators.Peek().Equals("/"))
            {
                operators.Pop();
                if (num != 0)
                {
                    double result = values.Pop() / num;
                    values.Push(result);
                }
                else
                {
                    throw new ArgumentException("Cannot divide by zero");
                }
            }
            else
            {
                values.Push(num);
            }
            return null;
        }

        /// <summary>
        /// Private helper method to detect if a string is a variable.
        /// </summary>
        /// <param name="t"></param> next string in the formula
        /// <returns></returns> true if the string is a variable or false if not
        private bool IsVariable(String t)
        {
            return Regex.IsMatch(t, "^[a-zA-Z_][a-zA-z0-9_]*$");
        }

        /// <summary>
        /// Private helper method used when the next string from the equation is a + 
        /// or -. Checks for at least 2 values to pop, then performs addition
        /// or subtraction between the two numbers and adds the result back to
        /// the value stack.
        /// </summary>
        /// <param name="t"></param> next string in the formula
        /// <param name="values"></param>
        /// <param name="operators"></param>
        private void AdditionHandler(string t)
        {
            // If the top operator is +, add the top values
            if (values.Count > 1 && operators.Peek().Equals("+"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                values.Push(value1 + value2);
            }
            // If the top operator is -, subtract the top values
            else if (values.Count > 1 && operators.Peek().Equals("-"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                values.Push(value2 - value1);
            }
            operators.Push(t);
        }

        /// <summary>
        /// Private helper method used when the next string in the formula
        /// is a closing parenthesis. Checks for enough values and a + or - top operator
        /// and performs a summation. Checks for an opening parenthesis operator 
        /// to pop. If enough values and * or / are left, it multiplies or divdes
        /// the top two values.
        /// </summary>
        /// <param name="values"></param> stack of values
        /// <param name="operators"></param> stack of operators
        private object ClosedParenthesisHandler()
        {
            // If top operator is +, add top values
            if (values.Count > 1 && operators.Peek().Equals("+"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                values.Push(value2 + value1);
            }
            // If top operator is -, subtract top values
            else if (values.Count > 1 && operators.Peek().Equals("-"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                values.Push(value2 - value1);
            }
            // Pop top operator if it is (
            if (operators.Count > 0 && operators.Peek().Equals("("))
            {
                operators.Pop();
            }
            // If top operator is *, multiply top values
            if (values.Count > 1 && operators.Peek().Equals("*"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                values.Push(value1 * value2);
            }
            // If top operator is /, divide top values
            else if (values.Count > 1 && operators.Peek().Equals("/"))
            {
                double value1 = values.Pop();
                double value2 = values.Pop();
                operators.Pop();
                if (value1 != 0)
                {
                    double result = value2 / value1;
                    values.Push(result);
                }
                else
                {
                    return new FormulaError("Cannot divide by zero");
                }
            }
            return null;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return variableSet;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return formulaBuilder.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is null || !ReferenceEquals(obj.GetType(), typeof(Formula)))
            {
                return false;
            }
            Formula objectFormula = (Formula)obj;
            return this.formulaBuilder.ToString() == objectFormula.formulaBuilder.ToString();
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (f1 is null)
            {
                return false;
            }
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return formulaBuilder.ToString().GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";
            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})", lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);
            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}