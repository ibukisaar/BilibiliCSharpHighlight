using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Calculator {
	using Expr = Expression;
	using ParamMap = Dictionary<string, ParameterExpression>;

	class Program {
		/// <summary>
		/// 表达式解析器
		/// </summary>
		/// <remarks>
		/// define
		/// 	: param* ':' addSub
		/// 	;
		/// param
		/// 	: Id
		/// 	;
		/// addSub
		/// 	: mulDiv (('+'|'-') mulDiv)*
		/// 	;
		/// mulDiv
		/// 	: pow (('*'|'/') pow)*
		/// 	;
		/// pow
		/// 	: unary ('^' pow)?
		/// 	;
		/// unary
		/// 	: ('+'|'-') unary
		/// 	| atom
		/// 	;
		/// atom
		/// 	: Id
		/// 	| Number
		/// 	;
		/// </remarks>
		class Parser {
			public class ParseException : Exception {
				public ParseException(int index, string message)
					: base($"位置({index})解析错误，{message}") { }
			}

			static readonly MethodInfo powMethod = typeof(Math).GetMethod("Pow");
			/// <summary>
			/// 匹配Id或Number的正则表达式
			/// </summary>
			static readonly Regex idOrNumRegex
				= new Regex(@"^((?<id>[a-zA-Z_][a-zA-Z_\d]*)|(\d*\.\d+|\d+\.?)(e[+\-]?\d+)?)",
					RegexOptions.Compiled | RegexOptions.ExplicitCapture);


			readonly ParamMap @params = new ParamMap();
			readonly string expr;
			int i = 0;

			Parser(string expr) {
				this.expr = expr;
				ParseMulDiv = BinaryParserGenerate(ParsePow,
					("*", (x, y) => Expr.Multiply(x, y)),
					("/", (x, y) => Expr.Divide(x, y)));
				ParseAddSub = BinaryParserGenerate(ParseMulDiv,
					("+", (x, y) => Expr.Add(x, y)),
					("-", (x, y) => Expr.Subtract(x, y)));
			}

			/// <summary>
			/// 跳过空白字符
			/// </summary>
			void SkipWhiteSpace() {
				while (i < expr.Length && char.IsWhiteSpace(expr[i])) i++;
			}

			/// <summary>
			/// 当前匹配位置之后如果是<paramref name="s"/>则吃掉
			/// </summary>
			bool Eat(string s) {
				SkipWhiteSpace();
				if (i + s.Length <= expr.Length && expr.AsSpan(i, s.Length).SequenceEqual(s)) {
					i += s.Length;
					return true;
				}
				return false;
			}

			/// <summary>
			/// 生成左结合的二元表达式解析器
			/// </summary>
			/// <param name="nextParser">下一级解析器</param>
			/// <param name="ops"></param>
			Func<Expr> BinaryParserGenerate(Func<Expr> nextParser,
				params (string Op, Func<Expr, Expr, Expr> Creator)[] ops) {
				return () => {
					var left = nextParser();
					int index;
					while ((index = Array.FindIndex(ops, p => Eat(p.Op))) >= 0) {
						left = ops[index].Creator(left, nextParser());
					}
					return left;
				};
			}

			/// <summary>
			/// param
			/// </summary>
			ParameterExpression ParseParam() {
				SkipWhiteSpace();
				var group = idOrNumRegex.Match(expr, i, expr.Length - i).Groups["id"];
				if (group.Success) {
					i += group.Length;
					return Expr.Parameter(typeof(double), group.Value);
				}
				return null;
			}

			/// <summary>
			/// define
			/// </summary>
			LambdaExpression ParseDefine() {
				while (ParseParam() is ParameterExpression param) {
					@params.Add(param.Name, param);
				}
				if (!Eat(":")) throw new ParseException(i, "期望: ':'");
				return Expr.Lambda(ParseAddSub(), @params.Values);
			}

			/// <summary>
			/// addSub
			/// </summary>
			readonly Func<Expr> ParseAddSub;
			/// <summary>
			/// mulDiv
			/// </summary>
			readonly Func<Expr> ParseMulDiv;

			/// <summary>
			/// pow
			/// </summary>
			Expr ParsePow() {
				var left = ParseUnary();
				return Eat("^") ? Expr.Call(powMethod, left, ParsePow()) : left;
			}

			/// <summary>
			/// unary
			/// </summary>
			Expr ParseUnary() {
				if (Eat("-")) return Expr.Negate(ParseUnary());
				if (Eat("+")) return ParseUnary();
				return ParseAtom();
			}

			/// <summary>
			/// atom
			/// </summary>
			Expr ParseAtom() {
				SkipWhiteSpace();
				var m = idOrNumRegex.Match(expr, i, expr.Length - i);
				if (m.Success) {
					i += m.Length;
					if (m.Groups["id"].Success) {
						if (@params.TryGetValue(m.Groups["id"].Value, out var param)) {
							return param;
						}
						throw new ParseException(i, $"未定义的参数:{m.Groups["id"].Value}");
					} else {
						return Expr.Constant(double.Parse(m.Value), typeof(double));
					}
				} else if (Eat("(")) {
					var result = ParseAddSub();
					if (!Eat(")")) throw new ParseException(i, "期望: ')'");
					return result;
				}
				throw new ParseException(i, "期望: '(' 或 <参数> 或 <数字>");
			}

			public static LambdaExpression Parse(string expr) {
				var parser = new Parser(expr);
				var result = parser.ParseDefine();
				parser.SkipWhiteSpace();
				if (parser.i != expr.Length)
					throw new ParseException(parser.i, "未能完全解析表达式");
				return result;
			}
		}

		delegate double CalcHandler(params double[] args);

		static CalcHandler Lambda(string expr) {
			var lambda = Parser.Parse(expr);
			var paramCount = lambda.Parameters.Count;
			var paramsParam = Expr.Parameter(typeof(double[]));
			var invoke = Expr.Invoke(lambda, Enumerable.Range(0, paramCount)
				.Select(i => Expr.ArrayIndex(paramsParam, Expr.Constant(i))));
			return Expr.Lambda<CalcHandler>(invoke, paramsParam).Compile();
		}

		static TDelegate Lambda<TDelegate>(string expr) where TDelegate : Delegate
			=> Parser.Parse(expr).Compile() as TDelegate;

		static void Main(string[] args) {
			void Print(double value) => Console.WriteLine(value);

			Print(Lambda(": 233")()); // 233
			Print(Lambda("x y: x + y")(22, 33)); // 55
			Print(Lambda("a b c: -a - b - c")(1, 2, 3)); // -6
			Print(Lambda("x1 y1 x2 y2: ((x1 - x2) ^ 2 + (y1 - y2) ^ 2) ^ 0.5")(0, 0, 3, 4)); // 5
			Print(Lambda("x1 y1 x2 y2: (x1-x2)*(x1-x2) + (y1-y2)*(y1-y2)")(0, 0, 3, 4)); // 25
			Print(Lambda<Func<double>>(": 2 ^ 3 ^ 2")()); // 512
		}
	}
}
