using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace B站专栏CSharp代码高亮 {
	class Program {
		static void Main(string[] args) {
			var code = @"using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using C = System.Console;
using static System.Console;

namespace MyNamespace {
	class Program {
		[Flags]
		enum RGB { R = 1, G = 2, B = 4 }

		[StructLayout(LayoutKind.Sequential)]
		struct Point : IEquatable<Point> {
			public static readonly Point Empty = default;

			public int X;
			public int Y;

			public Point(int x, int y) => (X, Y) = (x, y);

			public void Deconstruct(out int x, out int y)
				=> (x, y) = (X, Y);

			bool IEquatable<Point>.Equals(Point other) {
				return X == other.X && Y == other.Y;
			}

			public override string ToString()
				=> this switch {
					(0, 0) => ""原点"",
					{ X: 1, Y: 0 } => ""(1, 0)"",
					Point{ X: 0, Y: 1 } => ""(0, 1)"",
					_ => $""({X}, {Y})""
				};

			public static implicit operator Point((int X, int Y) p)
				=> new Point(p.X, p.Y);
		}

		delegate void PrintHandler(object obj);
		/// <summary>
		/// 参考<see cref=""Console.WriteLine(object)""/>
		/// </summary>
		static readonly PrintHandler Print = Console.WriteLine;

		unsafe static void Main(string[] args) {
			PrintEnumNames<RGB>();

			Span<Point> points = stackalloc[] {
				new Point(1, 1),
				new Point{ X = 2, Y = 3 },
				Point.Empty,
			};
			PrintSpan(points);
			WriteLine(sizeof(Point));
			WriteLine(typeof(Point));
			WriteLine(nameof(Point));

			using var mem = new System.IO.MemoryStream();
			int i = 0;
			while (true) {
				if (i >= 256) break;
				else {
					mem.WriteByte((byte)i);
					i++;
				}
			}

			using (var mem2 = new System.IO.MemoryStream()) {
				for (i = 0; i <= 0xff; i++) {
					mem2.WriteByte((Byte)i);
				}
			}

			try {
				throw new Exception();
			} catch (Exception e) when (true) {
				switch (e) {
					case ArgumentException{ ParamName: nameof(args) }:
						Console.WriteLine(""hello"");
						break;
					default: throw e;
				}
			} finally {

			}
		}

		static void PrintSpan<T>(Span<T> structs) where T : unmanaged {
			foreach (ref readonly var s in structs) {
				Print(s);
			}
			unsafe {
				fixed (T* p = structs) {
					for (int i = 0; i < structs.Length; i++) {
						Print(p[i]);
					}
				}
			}
		}

		/// <summary>	
		/// 输出所有枚举成员名称	
		/// </summary>
		/// <typeparam name=""TEnum"">枚举类型</typeparam>
		static void PrintEnumNames<TEnum>() where TEnum : Enum {
			foreach (var name in Enum.GetNames(typeof(TEnum))) {
				C.WriteLine(name);
			}
		}

	}

	unsafe public static class FastMath {
		const int LogTableLevel = 13; // 64KB
		static readonly double* LogTable = (double*)
			Marshal.AllocHGlobal((1 << LogTableLevel) * sizeof(double));

		static FastMath() {
			const int N = 1 << LogTableLevel;
			for (int i = 0; i < N; i++) {
				LogTable[i] = Math.Log(1 + (double)i / N, 2);
			}
		}

		/// <summary>
		/// 快速log2
		/// </summary>
		/// <param name=""x""><paramref name=""x""/>必须大于0</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Log2(double x) {
			const int N = 1 << LogTableLevel;
			ulong t = *(ulong*)&x;
			int exp = (int)(t >> 52) - 0x3ff;
			return LogTable[(t >> (52 - LogTableLevel)) & (N - 1)] + exp;
		}
	}
}
";
			
			var highlight = new BilibiliCSharpHighlight(
				code, // 代码
				showError: true, // 如果代码有错误则提示
				withReferences: null // 添加引用
				);
			var html = highlight.ToHtml();
			// html保存到 Z:\highlight\csharp.html
			File.WriteAllText(@"Z:\highlight\csharp.html",
$@"<html>
<head>
<link rel=""stylesheet"" type=""text/css"" href=""./prism.css"" />
</head>
<body>
<figure class=""code-box"">
<pre class="" language-csharp"" data-lang=""application/csharp@cs@CSharp""
><code class="" language-csharp"">{html}</code></pre>
</figure>
</body>
</html>
");
		}
	}
}
