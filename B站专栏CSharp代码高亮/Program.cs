using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace B站专栏CSharp代码高亮 {
	//using MyStruct = B站专栏CSharp代码高亮.Program.Struct;
	/// <summary>
	/// <see cref="System.Console"/><seealso cref=""/>
	/// </summary>
	class Program {
		struct Struct : IEquatable<Struct>, IDisposable {
			public Struct[] test;
			bool IEquatable<Struct>.Equals(Struct other) => throw new NotImplementedException();
			public void Dispose() { }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args) {
			string code = @"using System;
using C = System.Console;
// ...
namespace Saar {
	using MyStruct = Saar.Program.Struct;
	/// <summary>
	/// <see cref=""System.ConsoleColor""/><seealso cref=""System.Console""/>
	/// </summary>
	unsafe class Program {
		struct Struct : IEquatable<Struct>, IDisposable {
			public Struct[] test;
			/// <param name=""other"">other</param>
			bool IEquatable<Struct>.Equals(Struct other) => throw new NotImplementedException();
			public void Dispose() {}
		}
		readonly static string s = $""hello{string.Join("", "", Enum.GetNames(typeof(E)))}"";
		[Flags]
		enum E {R, G, B = 2}
		readonly static string s2 = $@""hello{nameof(Saar.Program.Struct)}"";
		readonly static string s3 = @""hello"";
		readonly static string s4 = ""hello"";
		readonly static Struct[] ss = new Struct[1];
		private int i1 = 0, i2 = (int)123d;
		private static void* pointer = null;

		static void Main(string[] args) {
			C.WriteLine(*(int*)pointer * 123);
			if (s == ""hello"") {
				System.Console.WriteLine(""yes"");
			} else {
				C.WriteLine(""no"");
			}
			if (s == ""saar"") Console.WriteLine(""saar"");

			foreach (var c in new string(' ', 100)) Console.WriteLine(c);
			do {} while (true);

			var tt = typeof(Struct);
			var size = sizeof(int);
			C.WriteLine(size);
			C.WriteLine(args);
			Console.ForegroundColor = ConsoleColor.Black;
			using (var ss = new Program.Struct{}) {

			}
		}

		/// <summary>
		/// 输出所有枚举值
		/// </summary>
		static void Print<T>() where T : Enum {
			foreach (var name in Enum.GetNames(typeof(T))) {
				Console.WriteLine(name);
			}
		}
	}
}
";
			string code2 = @"
using System;

class Test {
	void F() {
		var s = "";
	}
}
";

			var path = @"D:\MyDocuments\Documents\Visual Studio 2019\Projects\B站专栏CSharp代码高亮\B站专栏CSharp代码高亮";
			var selfCode = File.ReadAllText($@"{path}\BilibiliCSharpHighlight.cs");
			var refs = Directory.GetFiles(Environment.CurrentDirectory).Where(f => Path.GetExtension(f) == ".dll" && !f.StartsWith("B站"));
			var highlight = new BilibiliCSharpHighlight(code, showError: true, withReferences: refs);
			var html = highlight.ToHtml();
			Console.WriteLine(html);
			File.WriteAllText(@"Z:\highlight\csharp.html", $@"
<html>
<head>
<link rel=""stylesheet"" type=""text/css"" href=""./prism.css"" />
</head>
<body>
<figure class=""code-box"">
<pre class="" language-csharp"" data-lang=""application/csharp@cs@CSharp""><code class="" language-csharp"">{html}</code></pre>
</figure>
</body>
</html>
");

		}
	}
}
