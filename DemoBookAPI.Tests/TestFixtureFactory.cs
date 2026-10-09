using AutoFixture;

namespace DemoBookAPI.Tests
{
    internal static class TestFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}
