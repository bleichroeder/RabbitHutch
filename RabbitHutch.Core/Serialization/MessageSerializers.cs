using System.Text.Json;

namespace RabbitHutch.Core.Serialization
{
    /// <summary>
    /// The MessageSerializerDelegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public delegate string MessageSerializerDelegate<T>(T input);

    /// <summary>
    /// Provides a set of static methods for <see cref="MessageSerializerDelegate{T}"/>.
    /// </summary>
    public static class MessageSerializers
    {
        /// <summary>
        /// Creates a default message serializer using System.Text.Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MessageSerializerDelegate<T> DefaultMessageSerializer<T>() => item => JsonSerializer.Serialize(item);

        /// <summary>
        /// Creates a default message serializer using System.Text.Json with specific options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MessageSerializerDelegate<T> DefaultMessageSerializer<T>(JsonSerializerOptions options) => item => JsonSerializer.Serialize(item, options);

        /// <summary>
        /// Creates a custom message serializer with a specific function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static MessageSerializerDelegate<T> CustomMessageSerializer<T>(Func<T, string> function) => new(function.Invoke);
    }
}
