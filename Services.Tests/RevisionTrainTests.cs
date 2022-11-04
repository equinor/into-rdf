using System.IO;
using Services.ValidationServices.RevisionTrainValidationServices;
using Xunit;

namespace Services.Tests;

public class RevisionTrainTests
{
    private readonly IRevisionTrainValidator _revisionTrainValidator;

        public RevisionTrainTests(IRevisionTrainValidator revisionTrainValidator)
        {
            _revisionTrainValidator = revisionTrainValidator;
        }

    [Fact]
    public void TestRevisionTrainContent()
    {
        var testFile = "TestData/revisionTestTrain.ttl";
        string turtle = File.ReadAllText(testFile, System.Text.Encoding.Default);

        Assert.True(_revisionTrainValidator.ValidateRevisionTrain(turtle).Conforms);
    }

}