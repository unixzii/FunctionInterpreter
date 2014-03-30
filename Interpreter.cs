using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunctionInterpreter
{
    class Interpreter
    {
        private Context _context;

        public Interpreter()
        {
            _context = new Context();
        }

        public List<Token> Tokenize(string expr)
        {
            List<Token> tokens = new List<Token>();

            string temp = "";

            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];

                if (c == '(')
                {
                    if (temp.Trim() != "")
                    {
                        tokens.Add(new Token() { type = Token.TokenType.Value, value = temp });
                    }
                    tokens.Add(new Token() { type = Token.TokenType.LeftBracket });
                    temp = "";
                }
                else if (c == ')')
                {
                    if (temp.Trim() != "")
                    {
                        tokens.Add(new Token() { type = Token.TokenType.Value, value = temp });
                    }
                    tokens.Add(new Token() { type = Token.TokenType.RightBracket });
                    temp = "";
                }
                else if (c == ',')
                {
                    if (temp.Trim() != "")
                    {
                        tokens.Add(new Token() { type = Token.TokenType.Value, value = temp });
                    }
                    tokens.Add(new Token() { type = Token.TokenType.Comma });
                    temp = "";
                }
                else
                {
                    temp += c;
                }
            }

            if (temp.Trim() != "")
            {
                tokens.Add(new Token() { type = Token.TokenType.Value, value = temp });
            }

            return tokens;
        }

        public AbstractExpression GenerateTree(string expr)
        {
            List<Token> tokens = Tokenize(expr);
            Stack<FunctionExpression> func_stack = new Stack<FunctionExpression>();
            Stack<AbstractExpression> arg_stack = new Stack<AbstractExpression>();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];

                if (token.type == Token.TokenType.LeftBracket)
                {
                    FunctionExpression fe = new FunctionExpression(tokens[i - 1].value, _context);
                    func_stack.Push(fe);
                }

                if (token.type == Token.TokenType.RightBracket)
                {
                    FunctionExpression fe = func_stack.Pop();
                    AbstractExpression ae = arg_stack.Pop();
                    fe.PushArgument(ae);
                    arg_stack.Push(fe);
                }

                if (token.type == Token.TokenType.Comma)
                {
                    FunctionExpression fe = func_stack.Peek();
                    AbstractExpression ae = arg_stack.Pop();
                    fe.PushArgument(ae);
                }

                if (token.type == Token.TokenType.Value)
                {
                    string value = token.value;
                    if (value[0] >= '0' && value[0] <= '9')
                    {
                        IntegerExpression ie = new IntegerExpression(value, _context);
                        arg_stack.Push(ie);
                    }
                }
            }

            return arg_stack.Pop();
        }

        public int Interpret(string expr)
        {
            AbstractExpression ae = GenerateTree(expr);
            IntegerExpression result = ae.Evaluate() as IntegerExpression;

            if (result != null)
            {
                return result.ToInt();
            }
            else
            {
                return 0;
            }
        }

        public Context GetContext()
        {
            return _context;
        }

        public struct Token
        {
            public enum TokenType
            {
                LeftBracket,
                RightBracket,
                Comma,
                Value
            }

            public TokenType type;
            public string value;
        }
    }

    class Context
    {
        public delegate AbstractExpression Function(List<AbstractExpression> args);

        private Dictionary<string, Function> _functions;

        public Context()
        {
            _functions = new Dictionary<string, Function>();

            AddFunction(InterpreterGlobal.Add, "ADD");
            AddFunction(InterpreterGlobal.Minus, "MINUS");
            AddFunction(InterpreterGlobal.Multiply, "MULTIPLY");
            AddFunction(InterpreterGlobal.Divide, "DIVIDE");
        }

        public void AddFunction(Function func, string key)
        {
            _functions.Add(key, func);
        }

        public Function GetFunction(string key)
        {
            return _functions[key];
        }
    }

    abstract class AbstractExpression
    {
        private string _expr;
        private Context _context;

        public AbstractExpression(string expr, Context context)
        {
            _expr = expr;
            _context = context;
        }

        public string GetExpressionString()
        {
            return _expr;
        }

        public Context GetContext()
        {
            return _context;
        }

        public abstract AbstractExpression Evaluate();
        public abstract override string ToString();
    }

    class IntegerExpression : AbstractExpression
    {
        public IntegerExpression(string expr, Context context)
            : base(expr, context)
        {
            
        }

        public override AbstractExpression Evaluate()
        {
            return this;
        }

        public int ToInt()
        {
            return Int32.Parse(GetExpressionString());
        }

        public override string ToString()
        {
            return GetExpressionString();
        }
    }

    class FunctionExpression : AbstractExpression
    {
        private List<AbstractExpression> _args;

        public FunctionExpression(string expr, Context context)
            : base(expr, context)
        {
            _args = new List<AbstractExpression>();
        }

        public void PushArgument(AbstractExpression p)
        {
            _args.Add(p);
        }

        public override AbstractExpression Evaluate()
        {
            Context.Function func = GetContext().GetFunction(GetExpressionString());

            return func(_args);
        }

        public override string ToString()
        {
            return GetExpressionString();
        }
    }

    static class InterpreterGlobal
    {
        public static AbstractExpression Add(List<AbstractExpression> args)
        {
            AbstractExpression left = args[0].Evaluate();
            AbstractExpression right = args[1].Evaluate();

            int value = ((IntegerExpression)left).ToInt() + ((IntegerExpression)right).ToInt();

            return new IntegerExpression(value.ToString(), null);
        }

        public static AbstractExpression Minus(List<AbstractExpression> args)
        {
            AbstractExpression left = args[0].Evaluate();
            AbstractExpression right = args[1].Evaluate();

            int value = ((IntegerExpression)left).ToInt() - ((IntegerExpression)right).ToInt();

            return new IntegerExpression(value.ToString(), null);
        }

        public static AbstractExpression Multiply(List<AbstractExpression> args)
        {
            AbstractExpression left = args[0].Evaluate();
            AbstractExpression right = args[1].Evaluate();

            int value = ((IntegerExpression)left).ToInt() * ((IntegerExpression)right).ToInt();

            return new IntegerExpression(value.ToString(), null);
        }

        public static AbstractExpression Divide(List<AbstractExpression> args)
        {
            AbstractExpression left = args[0].Evaluate();
            AbstractExpression right = args[1].Evaluate();

            int value = ((IntegerExpression)left).ToInt() / ((IntegerExpression)right).ToInt();

            return new IntegerExpression(value.ToString(), null);
        }
    }
}
