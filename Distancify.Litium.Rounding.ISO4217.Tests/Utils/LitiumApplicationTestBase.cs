using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Utils
{
    /// <summary>
    ///     Test base class that automatic initiate the application.
    /// </summary>
    [Collection("Litium Application collection")]
    public abstract class LitiumApplicationTestBase : global::Litium.Xunit.ApplicationTestBase
    {
    }

    [CollectionDefinitionAttribute("Litium Application collection")]
    public class LitiumApplicationCollection : ICollectionFixture<global::Litium.Xunit.CollectionFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}