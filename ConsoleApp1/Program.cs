using B站专栏CSharp代码高亮;
using System;
using System.IO;
using System.Linq;

namespace ConsoleApp1 {
	class Program {
		static void Main(string[] args) {
			const string netCorePath =
				@"C:\Users\ibuki\.nuget\packages\microsoft.netcore.app\3.0.0-preview-27324-5\ref\netcoreapp3.0";
			var testCode = File.ReadAllText(@"D:\MyDocuments\Documents\Visual Studio 2019\Projects\B站专栏CSharp代码高亮\B站专栏CSharp代码高亮\Program.cs");
			var refs = new string[] {
				"B站专栏CSharp代码高亮.dll",
			};
			var highlight = new BilibiliCSharpHighlight(testCode,
				showError: true, // 如果代码有错误则提示
				withReferences: refs // 添加引用
				);
			var html = highlight.ToHtml();
			Console.WriteLine(html);
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
