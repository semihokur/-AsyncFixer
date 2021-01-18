using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using AsyncFixer.AsyncVoid;
using Xunit;

namespace AsyncFixer.Test
{
    public class AsyncVoidTests : CodeFixVerifier
    {
        [Fact]
        public void NoWarn_AsyncVoidMethodTest1()
        {
            //No diagnostics expected to show up

            var test = @"
using System;
using System.Threading.Tasks;

class Program
{
    static async Task<bool> foo()
    {
        await Task.Delay(1);
        return true;
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void AsyncVoidMethodTest2()
        {
            var test = @"
using System;
using System.Threading.Tasks;

class Program
{
    private static async void foo()
    {
        await Task.Delay(1);
        await Task.Delay(1);
    }
}";
            var expected = new DiagnosticResult { Id = DiagnosticIds.AsyncVoid };
            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Threading.Tasks;

class Program
{
    private static async Task foo()
    {
        await Task.Delay(1);
        await Task.Delay(1);
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void AsyncVoidAnonymousFunction()
        {
            var test = @"
using System;
using System.Threading.Tasks;

class Program
{
    private static void foo()
    {
        bar(async i => await Task.FromResult(i));
    }

    private static void bar(Action<int> action)
    {
        action(3);
    }
}";
            var expected = new DiagnosticResult { Id = DiagnosticIds.AsyncVoid };
            VerifyCSharpDiagnostic(test, expected);

            // No fix should be suggested.
            VerifyCSharpFix(test, test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AsyncVoidFixer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AsyncVoidAnalyzer();
        }
    }
}
