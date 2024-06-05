using System.Text.Json.Serialization;

namespace RabbitHutch.Core.Routing
{
    /// <summary>
    /// Represents a RabbitMQ Binding.
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the VHost.
        /// </summary>
        [JsonPropertyName("vhost")]
        public string? VHost { get; set; }

        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        /// <summary>
        /// Gets or sets the Destination Type.
        /// </summary>
        [JsonPropertyName("destination_type")]
        public string? DestinationType { get; set; }

        /// <summary>
        /// Gets or sets the RoutingKey.
        /// </summary>
        [JsonPropertyName("routing_key")]
        public string? RoutingKey { get; set; }

        /// <summary>
        /// Gets or sets the Arguments.
        /// </summary>
        [JsonPropertyName("arguments")]
        public Dictionary<string, string>? Arguments { get; set; }

        /// <summary>
        /// Gets or sets the PropertiesKey.
        /// </summary>
        [JsonPropertyName("properties_key")]
        public string? PropertiesKey { get; set; }
    }
}
