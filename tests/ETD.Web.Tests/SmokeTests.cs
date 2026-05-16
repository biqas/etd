namespace ETD.Web.Tests;

public class SmokeTests
{
    [Test]
    public async Task Math_Works()
    {
        var result = 1 + 1;
        await Assert.That(result).IsEqualTo(2);
    }
}
