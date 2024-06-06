using System.Text.Json;

namespace RabbitHutch.Tests
{
    /// <summary>
    /// A test class for the consumer.
    /// </summary>
    public class TestClass
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Deserialize the byte array to a <see cref="TestClass"/> instance
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static TestClass? Deserialize(byte[] bytes) => JsonSerializer.Deserialize<TestClass>(bytes);

        /// <summary>
        /// Create a new instance of the <see cref="TestClass"/> class.
        /// </summary>
        public TestClass()
        {
            Id = Guid.NewGuid();
            Name = "Test";
        }
    }
}