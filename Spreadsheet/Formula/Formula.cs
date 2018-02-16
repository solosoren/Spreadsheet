// Skeleton written by Joe Zachary for CS 3500, January 2017
// Soren Nelson

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public struct Formula
    {

        private Stack<String> operatorStack;
        private Stack<Double> valueStack;
        private IEnumerable<string> strings;

        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula) : this(formula, s => s, s => true) { }

        public Formula(String formula, Normalizer normalizer, Validator validator)
        {
            if (formula == null && normalizer == null && validator == null)
            {
                formula = "0";
            }
            operatorStack = new Stack<string>();
            valueStack = new Stack<Double>();
            strings = GetTokens(formula);

            if (formula == null || normalizer == null || validator == null)
            {
                throw new ArgumentNullException();
            }

            int parenthesis = 0;
            String prev = "";

            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"^[\+\-*/]$";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            String doublePattern = @"(?:\d+\.\d*|\d*\.\d+|\d+)(?:e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            foreach (String s in strings)
            {

                if (prev == "" && Regex.IsMatch(s, opPattern) || prev == "" && Regex.IsMatch(s, rpPattern))
                {
                    throw new FormulaFormatException("Not starting with a number, variable, or open parenthesis");
                }
                else if ((Regex.IsMatch(prev, lpPattern) || Regex.IsMatch(prev, opPattern)) && (Regex.IsMatch(s, opPattern) || Regex.IsMatch(s, rpPattern)))
                {
                    throw new FormulaFormatException("Following opening parenthesis or operation with an operation or closing parenthesis");
                }
                else if ((Regex.IsMatch(prev, doublePattern) || Regex.IsMatch(prev, varPattern) || Regex.IsMatch(prev, rpPattern)) && !(Regex.IsMatch(s, opPattern) || Regex.IsMatch(s, rpPattern)))
                {
                    throw new FormulaFormatException("Following number, variable or closing parenthesis with an number, variable or open parenthesis");
                }
                else if (Regex.IsMatch(s, spacePattern))
                {
                    continue;
                }
                else if (Regex.IsMatch(s, lpPattern))
                {
                    parenthesis++;
                }
                else if (Regex.IsMatch(s, rpPattern))
                {
                    parenthesis--;
                    if (parenthesis == -1)
                    {
                        throw new FormulaFormatException("Too many closing parenthesis");
                    }
                }
                else if (Regex.IsMatch(s, opPattern))
                {
                    prev = s;
                    continue;
                }
                else if (Regex.IsMatch(s, varPattern) || Regex.IsMatch(s, doublePattern))
                {
                    prev = s;
                    continue;
                }
                else
                {
                    throw new FormulaFormatException("Some Error");
                }
                prev = s;
            }

            if (prev == "")
            {
                throw new FormulaFormatException("No Tokens");
            }
            else if (parenthesis != 0)
            {
                throw new FormulaFormatException("Too many opening parenthesis");
            }
            // lp vs rp
            else if (Regex.IsMatch(prev, lpPattern) || Regex.IsMatch(prev, opPattern))
            {
                throw new FormulaFormatException("Not closing with number, variable, or closing parenthesis");
            }
            
            string normal = "";
            foreach (string s in strings)
            {
                string temp = s;
                if (Regex.IsMatch(s, varPattern))
                {
                    if (!Regex.IsMatch(temp = normalizer(String.Copy(s)), varPattern))
                    {
                        throw new FormulaFormatException("Normalizer changed variable to invalid term");
                    }

                }
                if (!validator(temp))
                {
                    throw new FormulaFormatException("Not valid according to validator");
                }
                normal = normal + temp;
            }
            strings = GetTokens(normal);  
        }

        /// <summary>
        /// Loops through the formula and finds the variables.
        /// </summary>
        /// <returns> A sorted set of all the variables in the formula</returns>
        public ISet<string> GetVariables()
        {
            ISet<string> set = new SortedSet<string>();
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            foreach (string s in strings)
            {
                if (Double.TryParse(s, out Double current)) { continue; }
                else if (Regex.IsMatch(s, varPattern))
                {
                    set.Add(s);
                }
                
            }
            return set;
        }

        /// <summary>
        /// Overrides the toString() to return the formula in string format
        /// </summary>
        /// <returns>the formula as a string</returns>
        public override string ToString()
        {
            string temp = "";
            foreach (string s in strings)
            {
                temp = temp + s;
            }
            return temp;
        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            if (lookup == null)
            {
                throw new ArgumentNullException();
            }
            String op = "";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            String doublePattern = @"(?:\d+\.\d*|\d*\.\d+|\d+)(?:e[\+-]?\d+)?";

            foreach (String t in strings)
            {
                if (Regex.IsMatch(t, doublePattern) || Regex.IsMatch(t, varPattern))
                {
                    Double current;
                    if (Regex.IsMatch(t, varPattern))
                    {

                        if (Double.TryParse(t, out current)) { }
                        else
                        {
                            try
                            {
                                current = lookup(t);
                            }
                            catch (UndefinedVariableException)
                            {
                                throw new FormulaEvaluationException("Undefined value");
                            }
                        }                        
                    }
                    else
                    {
                        current = Convert.ToDouble(t);
                    }

                    if (operatorStack.Count > 0)
                    {
                        op = operatorStack.Pop();
                        if (op == "/" || op == "*")
                        {
                            Double previous = valueStack.Pop();
                            if (op == "/")
                            {
                                if (current != 0)
                                {
                                    valueStack.Push(previous / current);
                                }
                                else
                                {
                                    throw new FormulaEvaluationException("Undefined value");
                                }
                            }
                            else
                            {
                                valueStack.Push(previous * current);
                            }
                            // Why does this not work?
                            // current = previous (op == "/" ? / : *) current;
                        }
                        else
                        {
                            operatorStack.Push(op);
                            valueStack.Push(current);
                        }
                    }
                    else
                    {
                        valueStack.Push(current);
                    }
                }
                else if (t == "+" || t == "-")
                {
                    if (operatorStack.Count > 0)
                    {
                        if ((op = operatorStack.Pop()) == "+")
                        {
                            Double r = valueStack.Pop();
                            Double l = valueStack.Pop();
                            valueStack.Push(l + r);
                        }
                        else if (op == "-")
                        {
                            Double r = valueStack.Pop();
                            Double l = valueStack.Pop();
                            valueStack.Push(l - r);
                        }
                        else
                        {
                            operatorStack.Push(op);
                        } 
                    }
                    operatorStack.Push(t);
                }
                else if (t == "*" || t == "/" || t == "(")
                {
                    operatorStack.Push(t);
                }
                else if (t == ")")
                {
                    if (operatorStack.Count > 0 && (op = operatorStack.Pop()) == "+" || op == "-")
                    {
                        Double r = valueStack.Pop();
                        Double l = valueStack.Pop();
                        if (op == "+")
                        {                     
                            valueStack.Push(l + r);
                        }
                        else if (op == "-")
                        {
                            valueStack.Push(l - r);
                        }
                        operatorStack.Pop();
                    }
                    // in case of the operatorStack count being 1 and it being popped off
                    Boolean popped = operatorStack.Count > 0 ? true : false;
                    
                    if (operatorStack.Count > 0 && (op = operatorStack.Pop()) == "*" || op == "/")
                    {
                        Double r = valueStack.Pop();
                        Double l = valueStack.Pop();
                        if (op == "*")
                        {
                            valueStack.Push(l * r);
                        }
                        else if (op == "/")
                        {
                            if (r != 0)
                            {
                                valueStack.Push(l / r);
                            }
                            else
                            {
                                throw new FormulaEvaluationException("Dividing by 0");
                            }
                            
                        }

                    }
                    else if (operatorStack.Count > 0 || popped)
                    {
                        operatorStack.Push(op);
                    }
                }
            }
            if (operatorStack.Count == 0)
            {
                return valueStack.Pop();
            }
            if ((op = operatorStack.Pop()) == "+")
            {
                return valueStack.Pop() + valueStack.Pop();
            }

            Double right = valueStack.Pop();
            Double left = valueStack.Pop();
            return left - right;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            // NOTE:  These patterns are designed to be used to create a pattern to split a string into tokens.
            // For example, the opPattern will match any string that contains an operator symbol, such as
            // "abc+def".  If you want to use one of these patterns to match an entire string (e.g., make it so
            // the opPattern will match "+" but not "abc+def", you need to add ^ to the beginning of the pattern
            // and $ to the end (e.g., opPattern would need to be @"^[\+\-*/]$".)
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.  This pattern is useful for 
            // splitting a string into tokens.
            String splittingPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, splittingPattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);

    /// <summary>
    /// normalizes the passed in formula into the wanted form.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public delegate string Normalizer(string s);

    /// <summary>
    /// Imposes extra restrictions on the validity of a variable 
    /// </summary>
    /// <param name="s"></param>
    /// <returns>Whether the formula is able to be validated</returns>
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
