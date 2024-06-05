using System.Text.Json;

namespace RabbitHutch.Core.Serialization
{
    /// <summary>
    /// The MessageDeserializerDelegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public delegate T? MessageDeserializerDelegate<T>(string input);

    /// <summary>
    /// The MessageDeserializerFromBytesDelegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public delegate T? MessageDeserializerFromBytesDelegate<T>(byte[] input);

    /// <summary>
    /// Provides a set of static methods for <see cref="MessageDeserializerDelegate{T}"/> and <see cref="MessageDeserializerFromBytesDelegate{T}"/>.
    /// </summary>
    public static class MessageDeserializers
    {
        /// <summary>
        /// Creates a default message deserializer using System.Text.Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MessageDeserializerDelegate<T> DefaultMessageDeserializer<T>() => x => JsonSerializer.Deserialize<T>(x);

        /// <summary>
        /// Creates a default message deserializer using System.Text.Json with specific options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MessageDeserializerDelegate<T> DefaultMessageDeserializer<T>(JsonSerializerOptions options) => x => JsonSerializer.Deserialize<T>(x, options);

        /// <summary>
        /// Creates a default message deserializer from byte array using System.Text.Json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MessageDeserializerFromBytesDelegate<T> DefaultMessageDeserializerFromBytes<T>() => x => JsonSerializer.Deserialize<T>(x);

        /// <summary>
        /// Creates a default message deserializer from byte array using System.Text.Json with specific options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MessageDeserializerFromBytesDelegate<T> DefaultMessageDeserializerFromBytes<T>(JsonSerializerOptions options) => x => JsonSerializer.Deserialize<T>(x, options);

        /// <summary>
        /// Creates a custom message deserializer with a specific function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static MessageDeserializerDelegate<T> CustomMessageDeserializer<T>(Func<string, T> function) => new(function.Invoke);

        /// <summary>
        /// Creates a custom message deserializer from byte array with a specific function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static MessageDeserializerFromBytesDelegate<T> CustomMessageDeserializerFromBytes<T>(Func<byte[], T> function) => new(function.Invoke);
    }
}
