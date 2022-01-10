using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// This class evaluates a string formula into a solution or throws an
    /// exception if the formula doesnt follow infix standards. The evaluator is
    /// able to use variables within the formula after they've been translated to integers.
    /// 
    /// Author: Sam Christensen
    /// </summary>
    public static class Evaluator
    {
        // delegate method to convert variables to numbers
        public delegate int Lookup(String v);

        /// <summary>
        /// Finds the result of an infix formula given as a string. Can handle variables
        /// given in letter-number form through a delegate method which converts
        /// the variable to an integer.
        /// </summary>
        /// <param name="exp"></param> string formula given in infix form
        /// <param name="variableEvaluator"></param> delegate method for finding value of a variable
        /// <returns></returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();
            foreach (string s in substrings)
            {
                String t = s.Trim();
                // If the next string in the list is an int
                if (int.TryParse(t, out int num))
                {
                    IntegerHandler(num, values, operators);
                }
                // If the next string is a variable
                else if (IsVariable(t))
                {
                    try {
                        int varToNum = variableEvaluator(t);
                        IntegerHandler(varToNum, values, operators);
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException("Variable value not found" +
                            "");
                    }
                }
                // If the next string is + or -
                else if (t.Equals("+") || t.Equals("-"))
                {
                    AdditionHandler(t, values, operators);
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
                    ClosedParenthesisHandler(values, operators);
                }
            }
            // Return the final value
            if (operators.Count == 0 && values.Count == 1)
            {
                return values.Pop();
            }
            if (operators.Count == 1 && values.Count == 2)
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
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
            throw new ArgumentException("Invalid symbol or formula");
        }

        /// <summary>
        /// Private helper method used when an integer is found in the formula. Simplifies 
        /// the Evaluate function. Checks for correct operators in stack then either adds
        /// the product of two numbers or just pushes the number on the stack. Also called 
        /// after a variable has been converted to an integer.
        /// </summary>
        /// <param name="num"></param> the integer pulled from the formula
        /// <param name="values"></param> stack of values
        /// <param name="operators"></param> stack of operators
        private static void IntegerHandler(int num, Stack<int> values, Stack<string> operators)
        {
            // If top operator is *, multiply top value and integer
            if (operators.Count > 0 && operators.Peek().Equals("*"))
            {
                operators.Pop();
                int result = values.Pop() * num;
                values.Push(result);
            }
            // If top operator is /, divide top value and integer
            else if (operators.Count > 0 && operators.Peek().Equals("/"))
            {
                operators.Pop();
                try
                {
                    int result = values.Pop() / num;
                    values.Push(result);
                }
                catch (DivideByZeroException) 
                {
                    throw new ArgumentException("Cannot divide by zero");
                }
            }   
            else
            {
                values.Push(num);
            }
        }

        /// <summary>
        /// Private helper method to detect if a string is a variable.
        /// Starts at both ends of the string and works inward to 
        /// verify if the string has the right properties of a variable
        /// </summary>
        /// <param name="t"></param> next string in the formula
        /// <returns></returns> true if the string is a variable or false if not
        private static bool IsVariable(String t)
        {
            // Basic property checks so prevent use on integers or non variables
            if (t.Length > 1 && Char.IsLetter(t[0]) && Char.IsNumber(t[t.Length - 1]))
            {
                int letterSwitchIndex = -1;
                int numberSwitchIndex = -1;
                for (int letterIndex = 0, numberIndex = t.Length - 1; letterIndex < t.Length; letterIndex++, numberIndex--)
                {
                    // Checks when the letters switch to numbers
                    if (Char.IsDigit(t[letterIndex]))
                    {
                        if (letterSwitchIndex == -1)
                        {
                            letterSwitchIndex = letterIndex;
                        }
                    }
                    // Checks when numbers switch to letters
                    if (Char.IsLetter(t[numberIndex]))

                    {
                        if (numberSwitchIndex == -1)
                        {
                            numberSwitchIndex = numberIndex;
                        }
                    }
                }
                // The where the last letter is and the first number is are one index apart
                return letterSwitchIndex - numberSwitchIndex == 1;
            }
            return false;
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
        private static void AdditionHandler(string t, Stack<int> values, Stack<string> operators)
        {
            // If the top operator is +, add the top values
            if (values.Count > 1 && operators.Peek().Equals("+"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
                operators.Pop();
                values.Push(value1 + value2);
            }
            // If the top operator is -, subtract the top values
            else if (values.Count > 1 && operators.Peek().Equals("-"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
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
        private static void ClosedParenthesisHandler(Stack<int> values, Stack<string> operators)
        {
            // If top operator is +, add top values
            if (values.Count > 1 && operators.Peek().Equals("+"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
                operators.Pop();
                values.Push(value2 + value1);
            }
            // If top operator is -, subtract top values
            else if (values.Count > 1 && operators.Peek().Equals("-"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
                operators.Pop();
                values.Push(value2 - value1);
            }
            // Pop top operator if it is (
            if (operators.Count > 0 && operators.Peek().Equals("("))
            {
                operators.Pop();
            }
            else
            {
                throw new ArgumentException("Missing parenthesis");
            }
            // If top operator is *, multiply top values
            if (values.Count > 1 && operators.Peek().Equals("*"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
                operators.Pop();
                values.Push(value1 * value2);
            }
            // If top operator is /, divide top values
            else if (values.Count > 1 && operators.Peek().Equals("/"))
            {
                int value1 = values.Pop();
                int value2 = values.Pop();
                operators.Pop();
                try
                {
                    int result = value2 / value1;
                    values.Push(result);
                }
                catch (DivideByZeroException)
                {
                    throw new ArgumentException("Cannot divide by zero");
                }
            }
        }
    }
}