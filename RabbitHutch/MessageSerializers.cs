using System.Text.Json;

namespace RabbitHutch
{
    public delegate string MessageSerializerDelegate<T>(T input);

    public static class MessageSerializers
    {
        public static MessageSerializerDelegate<T> DefaultMessageSerializer<T>() => x => JsonSerializer.Serialize(x);
        public static MessageSerializerDelegate<T> CustomMessageSerializer<T>(Func<T, string> function) => new(function.Invoke);
    }
}
