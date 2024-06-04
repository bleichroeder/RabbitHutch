namespace RabbitHutch
{
    public delegate string RoutingKeyGeneratorDelegate<T>(T input);

    public static class RoutingKeyGenerators
    {
        public static RoutingKeyGeneratorDelegate<T> DefaultRoutingKeyGenerator<T>() => x => "#";
        public static RoutingKeyGeneratorDelegate<T> CustomRoutingKeyGenerator<T>(Func<T, string> function) => new(function.Invoke);
    }
}
