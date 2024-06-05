namespace RabbitHutch.Core.Delegates
{
    /// <summary>
    /// The new message callback delegate.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate Task<bool> AsyncNewMessageCallbackDelegate<T>(T message);

    public static class MessageReceivedDelegates
    {
        /// <summary>
        /// The default new message callback.
        /// Always returns true. Don't use this.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AsyncNewMessageCallbackDelegate<T> DefaultNewMessageCallback<T>() => async message => await Task.FromResult(true);

        /// <summary>
        /// Creates an AsyncNewMessageCallbackDelegate from a Func<T, bool>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static AsyncNewMessageCallbackDelegate<T> From<T>(Func<T, bool> func) => async message => await Task.FromResult(func(message));
    }
}
