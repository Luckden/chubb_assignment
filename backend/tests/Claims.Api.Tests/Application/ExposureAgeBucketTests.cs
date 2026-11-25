using Claims.Api.Application.Exposure;

namespace Claims.Api.Tests.Application;

/// <summary>Age buckets for claimed exposure (D7). Pure unit tests: no database, no Docker.</summary>
public class ExposureAgeBucketTests
{
    [Theory]
    [InlineData(0, ExposureService.UpToAWeek)]
    [InlineData(7, ExposureService.UpToAWeek)]
    [InlineData(8, ExposureService.UpToAMonth)]
    [InlineData(30, ExposureService.UpToAMonth)]
    [InlineData(31, ExposureService.OverAMonth)]
    public void D7_AgeBucket_DaysSinceSubmission_ReturnsItsBucket(int days, string expected) =>
        Assert.Equal(expected, ExposureService.AgeBucket(days));
}
