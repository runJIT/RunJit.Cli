using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using $ProjectName$.Api.$DomainNamePlural$.V1;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace $ProjectName$.Test.Api.$DomainNamePlural$.V1
{
    /// <summary>
    /// For request bodies that are syntactically correct yet semantically invalid (e.g., in POST, PUT, PATCH), 
    /// a 422 Unprocessable Content status is recommended 
    /// (<see href="https://www.rfc-editor.org/rfc/rfc9110#status.422">RFC 9110</see>).
    /// </summary>
    [TestClass]
    [TestCategory("$DomainNamePlural$")]
    [TestCategory("$DomainNamePlural$ V1 Create Status 422 UnprocessableContent")]
    public class Create_Status_422_UnprocessableContent_Test : ApiTestBase
    {
        private static readonly string Unique$DomainName$Name = GetUniqueRunnerName();

        [TestInitialize]
        public Task InitAsync()
        {
            return CleanupAsync();
        }

        [DataTestMethod]
        [DataRow("EndDate_Before_StartDate.json")]
        [DataRow("EndDate_Is_Empty.json")]
        [DataRow("EndDate_Is_Invalid_Format.json")]
        [DataRow("EndDate_Is_Whitespace.json")]
        [DataRow("Forms$IdPropertyName$_Is_Empty.json")]
        [DataRow("Forms$IdPropertyName$_Is_Invalid.json")]
        [DataRow("Forms$IdPropertyName$_Is_Null.json")]
        [DataRow("Forms$IdPropertyName$_Is_Whitespace.json")]
        [DataRow("Forms$IdPropertyName$_Is_Negative.json")]
        [DataRow("FormsType_Is_Empty.json")]
        [DataRow("FormsType_Is_Invalid.json")]
        [DataRow("FormsType_Is_Null.json")]
        [DataRow("FormsType_Is_Whitespace.json")]
        [DataRow("HasUpdate_Is_Invalid.json")]
        [DataRow("Languages_0_EnglishName_Is_Empty.json")]
        [DataRow("Languages_0_EnglishName_Is_Invalid.json")]
        [DataRow("Languages_0_EnglishName_Is_Null.json")]
        [DataRow("Languages_0_EnglishName_Is_Whitespace.json")]
        [DataRow("Languages_0_NativeName_Is_Empty.json")]
        [DataRow("Languages_0_NativeName_Is_Invalid.json")]
        [DataRow("Languages_0_NativeName_Is_Null.json")]
        [DataRow("Languages_0_NativeName_Is_Whitespace.json")]
        [DataRow("Languages_0_ParentTag_Is_Empty.json")]
        [DataRow("Languages_0_ParentTag_Is_Invalid.json")]
        [DataRow("Languages_0_ParentTag_Is_Null.json")]
        [DataRow("Languages_0_ParentTag_Is_Whitespace.json")]
        [DataRow("Languages_0_TagThreeLetter_Is_Empty.json")]
        [DataRow("Languages_0_TagThreeLetter_Is_Invalid.json")]
        [DataRow("Languages_0_TagThreeLetter_Is_Null.json")]
        [DataRow("Languages_0_TagThreeLetter_Is_Whitespace.json")]
        [DataRow("Languages_0_Tag_Is_Empty.json")]
        [DataRow("Languages_0_Tag_Is_Invalid.json")]
        [DataRow("Languages_0_Tag_Is_Null.json")]
        [DataRow("Languages_0_Tag_Is_Whitespace.json")]
        [DataRow("Languages_Is_Null.json")]
        [DataRow("Project$IdPropertyName$_Is_Empty.json")]
        [DataRow("Project$IdPropertyName$_Is_Invalid.json")]
        [DataRow("Project$IdPropertyName$_Is_Null.json")]
        [DataRow("Project$IdPropertyName$_Is_Whitespace.json")]
        [DataRow("Project$IdPropertyName$_Is_Negative.json")]
        [DataRow("SeesionEndsIn_Is_Too_Long.json")]
        [DataRow("SeesionEndsIn_Is_Too_Low.json")]
        [DataRow("StartDate_Is_Empty.json")]
        [DataRow("StartDate_Is_Invalid_Format.json")]
        [DataRow("StartDate_Is_Null.json")]
        [DataRow("StartDate_Is_Whitespace.json")]
        [DataRow("Sum_Error.json")]
        public Task Should_Return_Unprocessable_Entity_If_Create_Request_Data_Contains_Invalid(string useCase)
        {
            return Client.AssertPostAsErrorAsync<ValidationProblemDetailsExtended>($"$BasePath$/v1/$DomainNamePluralLower$",
                                                                           useCase,
                                                                           useCase,
                                                                           differenceFunc: IgnoreAutoValues,
                                                                           [("$Unique$DomainName$Name$", Unique$DomainName$Name)]);
        }

        private IEnumerable<Difference> IgnoreAutoValues(IImmutableList<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.MemberPath.Contains($".{nameof($DomainName$.$IdPropertyName$)}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return difference;
            }
        }

        [TestCleanup]
        public Task CleanupAsync()
        {
            return Client.AssertDeleteAsync($"$BasePath$/v1/$DomainNamePluralLower$?$QueryPropertyNameLower$={Unique$DomainName$Name}");
        }
    }
}
