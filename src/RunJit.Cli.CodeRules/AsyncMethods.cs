using System.Collections;
using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace RunJit.Cli.CodeRules
{
    [TestCategory("Coding-Rules")]
    [TestCategory("AsyncMethods")]
    [TestClass]
    public class AsyncMethods : MsTestBase
    {
        [TestMethod]
        public void Async_Methods_Which_Returns_Only_Await_Task_Should_Just_Return_The_Async_Call()
        {
            var returnAwaitOnly = (from syntaxTree in AllSyntaxTrees
                                   from @class in syntaxTree.Classes
                                   from method in @class.Methods
                                   where method.Statements.Count < 2 && method.MethodBody.Contains("await") && method.LineStatements.Count < 2
                                   select new
                                          {
                                              Error = $@"
Do not use await if you only return a task
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.MethodValue}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.MethodValue.Replace("return await", "return").Replace(".ConfigureAwait(false)", string.Empty).Replace(".ConfigureAwait(true)", string.Empty)}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                          }).ToImmutableList();

            Assert.IsTrue(returnAwaitOnly.IsEmpty(),
                          @$"Invalid response type. Findings: '{returnAwaitOnly.Count}': {Environment.NewLine}{returnAwaitOnly.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void Async_Methods_Which_Returns_Only_Await_Task_Should_Just_Return_The_Async_Call_In_Tests_As_Well()
        {
            var returnAwaitOnly = from syntaxTree in AllSyntaxTrees
                                  from @class in syntaxTree.Classes
                                  from method in @class.Methods
                                  where method.Statements.Count == 1 &&
                                        (method.Statements[0].StartWith("await") || method.Statements[0].StartWith("return await"))
                                  select new
                                         {
                                             Error = $@"
Do not use await if you only return a task
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.MethodValue}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.MethodValue.Replace("return await", "return").Replace("await", "return").Replace(" async", string.Empty).Replace(".ConfigureAwait(false)", string.Empty).Replace(".ConfigureAwait(true)", string.Empty)}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                         };

            Assert.IsTrue(returnAwaitOnly.IsEmpty(),
                          @$"Invalid response type. Findings: '{returnAwaitOnly.Count()}': {Environment.NewLine}{returnAwaitOnly.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void All_Methods_Which_Return_A_Task_Should_Have_Post_Fix_Async()
        {
            var returnAwaitOnly = from syntaxTree in AllSyntaxTrees
                                  from @class in syntaxTree.Classes
                                  where @class.Name.NotEqualsTo(nameof(Program))
                                  from method in @class.Methods
                                  where method.Attributes.IsNull() || method.Attributes.All(a => a.Name.DoesNotContain("TestMethod"))
                                  where method.ReturnParameter.Contains("Task") && method.Name.EndWith("Async").IsFalse()
                                  where method.Name.NotEqualsTo("Handle") // Exception MediatR class dont have async :(
                                  select new
                                         {
                                             Error = $@"
Please name your async methods correctly with postfix 'Async'.
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.Name}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.Name}Async
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                         };

            Assert.IsTrue(returnAwaitOnly.IsEmpty(),
                          @$"Please name your async methods correctly with postfix 'Async'. Findings {returnAwaitOnly.Count()}': {Environment.NewLine}{returnAwaitOnly.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void All_Methods_Which_Return_A_Task_Should_Have_Post_Fix_Async_In_Tests_As_Well()
        {
            var returnAwaitOnly = from syntaxTree in AllSyntaxTrees
                                  from @class in syntaxTree.Classes
                                  where @class.Name.NotEqualsTo(nameof(Program))
                                  from method in @class.Methods
                                  where method.Attributes.Any(attribute => attribute.Name.EqualsTo("TestMethod") || attribute.Name.EqualsTo("DataTestMethod")).IsFalse()
                                  where method.ReturnParameter.Contains("Task") && method.Name.EndWith("Async").IsFalse()
                                  where method.Name.NotEqualsTo("Handle") // Exception MediatR class dont have async :(
                                  select new
                                         {
                                             Error = $@"
Please name your async methods correctly with postfix 'Async'
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.Name}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.Name}Async
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                         };

            Assert.IsTrue(returnAwaitOnly.IsEmpty(),
                          @$"Please name your async methods correctly with postfix 'Async'. Findings: '{returnAwaitOnly.Count()}': {Environment.NewLine}{returnAwaitOnly.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void Async_Methods_Should_Never_Use_IEnumerable_As_Return_Type()
        {
            var ienumerableReturnTypes = from syntaxTree in AllSyntaxTrees
                                         from @class in syntaxTree.Classes
                                         from method in @class.Methods
                                         where method.ReturnParameter.Contains("Task<IEnumerable")
                                         select new
                                                {
                                                    Error = $@"
Never use IEnumerable as return type for async methods.
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.MethodValue.Split(Environment.NewLine)[0]}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.MethodValue.Split(Environment.NewLine)[0].Replace(method.ReturnParameter, method.ReturnParameter.Replace(nameof(IEnumerable), "IImmutableList"))}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Best:   {method.MethodValue.Split(Environment.NewLine)[0].Replace(method.ReturnParameter, method.ReturnParameter.Replace("Task<IEnumerable", "IAsyncEnumerable"))}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                                };

            Assert.IsTrue(ienumerableReturnTypes.IsEmpty(),
                          @$"Never use IEnumerable as return type for async methods. Findings: '{ienumerableReturnTypes.Count()}': {Environment.NewLine}{ienumerableReturnTypes.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void Use_IAsyncEnumerable_Instead_Of_IEnumerable_Of_Task()
        {
            var ienumerableReturnTypes = from syntaxTree in AllSyntaxTrees
                                         from @class in syntaxTree.Classes
                                         from method in @class.Methods
                                         where method.ReturnParameter.Contains("IEnumerable<Task")
                                         select new
                                                {
                                                    Error = $@"
Never use IEnumerable as return type for async methods.
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:  {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method: {method.MethodValue.Split(Environment.NewLine)[0]}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample: {method.MethodValue.Split(Environment.NewLine)[0].Replace(method.ReturnParameter, $"{method.ReturnParameter.Replace("IEnumerable<Task", "IAsyncEnumerable").TrimEnd('>')}>")}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                                };

            Assert.IsTrue(ienumerableReturnTypes.IsEmpty(),
                          @$"Never use IEnumerable as return type for async methods. Findings: '{ienumerableReturnTypes.Count()}': {Environment.NewLine}{ienumerableReturnTypes.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        public void Never_Use_TaskWaitAll_Use_TaskWhenAll_Instead()
        {
            var blockingOperation = from syntaxTree in AllSyntaxTrees
                                    from @class in syntaxTree.Classes
                                    where @class.Name.NotEqualsTo(nameof(AsyncMethods))
                                    from method in @class.Methods
                                    from statement in method.Statements
                                    where statement.Contains($"{nameof(Task)}.{nameof(Task.WaitAll)}")
                                    select new
                                           {
                                               Error = $@"
Never use blocking Task.WaitAll operations, use Task.WhenAll instead.
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:     {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method:    {method.MethodValue.Split(Environment.NewLine)[0]}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Statement: {statement}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample:    {statement.Replace($"{nameof(Task)}.{nameof(Task.WaitAll)}", $"await {nameof(Task)}.{nameof(Task.WhenAll)}")}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                           };

            Assert.IsTrue(blockingOperation.IsEmpty(),
                          @$"Never use blocking Task.WaitAll operations, use Task.WhenAll instead.. Findings: '{blockingOperation.Count()}': {Environment.NewLine}{blockingOperation.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        [TestMethod]
        [DataRow(".Result;")]
        [DataRow(".Result.")]
        [DataRow(".Wait()")]
        [DataRow(".GetAwaiter().GetResult()")]
        public void Never_Use_Blocking_Async_Operations(string blockingOperation)
        {
            var blockingOperations = from syntaxTree in AllSyntaxTrees
                                     from @class in syntaxTree.Classes
                                     from method in @class.Methods
                                     from statement in method.Statements
                                     where statement.Contains(blockingOperation)
                                     select new
                                            {
                                                Error = $@"
Never use blocking {blockingOperation} operation, use async/await for async operations
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Class:     {@class.FullQualifiedName}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Method:    {method.MethodValue.Split(Environment.NewLine)[0]}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Statement: {statement}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample:    {statement.Replace("= ", "= await ").Replace(blockingOperation, ".ConfigureAwait(false)")}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                            };

            Assert.IsTrue(blockingOperations.IsEmpty(),
                          @$"Never use blocking {blockingOperation} operation, use async/await for async operations. Findings: '{blockingOperations.Count()}': {Environment.NewLine}{blockingOperations.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        }

        //        [TestMethod]
        //        public void Use_For_Each_Await_Statement_A_Separate_Line()
        //        {
        //            var returnAwaitOnly = from syntaxTree in AllSyntaxTrees
        //                                  from @class in syntaxTree.Classes
        //                                  from @method in @class.Methods
        //                                  from @method
        //                                  where method.ReturnParameter.Contains("Task") && method.Name.EndWith("Async").IsFalse()
        //                                  where method.Name.NotEqualsTo("Handle") // Exception MediatR class dont have async :(
        //                                  select new
        //                                  {
        //                                      Error = $@"
        //Please name your async methods correctly with postfix 'Async'
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Class:  {@class.FullQualifiedName}
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Method: {method.Name}
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //Sample: {method.Name}Async
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //"
        //                                  };

        //            Assert.IsTrue(returnAwaitOnly.IsEmpty(),
        //                @$"Please name your async methods correctly with postfix 'Async': {Environment.NewLine}{returnAwaitOnly.Select(error => error.Error).Flatten(Environment.NewLine)}{Environment.NewLine}");
        //        }
    }
}
