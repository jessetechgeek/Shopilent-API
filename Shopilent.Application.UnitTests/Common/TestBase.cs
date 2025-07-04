using Shopilent.Application.UnitTests.Testing;

namespace Shopilent.Application.UnitTests.Common;

/// <summary>
/// Base class for all unit tests providing common functionality and setup
/// </summary>
public abstract class TestBase
{
    protected readonly TestFixture Fixture;
    protected readonly CancellationToken CancellationToken;

    protected TestBase()
    {
        Fixture = new TestFixture();
        CancellationToken = CancellationToken.None;
    }
}