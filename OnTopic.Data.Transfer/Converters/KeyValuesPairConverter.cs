/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer.Converters {

  /*============================================================================================================================
  | CLASS: KEY/VALUES PAIR CONVERTER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides instructions for serializing or deserializing a <see cref="KeyValuesPair"/> instance.
  /// </summary>
  /// <remarks>
  ///   The converter allows backward compatibility with legacy conventions which used <c>Relationships</c> instead of <c>
  ///   Values</c>.
  /// </remarks>
  class KeyValuesPairConverter: JsonConverter<KeyValuesPair> {

    /*==========================================================================================================================
    | METHOD: READ
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deserializes the data from JSON.
    /// </summary>
    public override KeyValuesPair Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (reader.TokenType != JsonTokenType.StartObject) {
        throw new JsonException();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve data
      \-----------------------------------------------------------------------------------------------------------------------*/
      var key                   = (string?)null;
      var values                = (string[]?)null;

      while (reader.Read()) {
        if (reader.TokenType == JsonTokenType.EndObject) {
          break;
        }
        if (reader.TokenType != JsonTokenType.PropertyName) {
          throw new JsonException();
        }
        var propertyName = reader.GetString();
        if (propertyName is null) {
          continue;
        }
        if (propertyName.Equals("Key", StringComparison.OrdinalIgnoreCase)) {
          key                   = JsonSerializer.Deserialize<String>(ref reader, options);
        }
        else if (
          propertyName.Equals("Relationships", StringComparison.OrdinalIgnoreCase) ||
          propertyName.Equals("Values", StringComparison.OrdinalIgnoreCase)
        ) {
          values                = JsonSerializer.Deserialize<String[]>(ref reader, options);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate data
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(key, nameof(key));

      /*------------------------------------------------------------------------------------------------------------------------
      | Create data
      \-----------------------------------------------------------------------------------------------------------------------*/
      var relationship = new KeyValuesPair() {
        Key                     = key,
        Values                  = new(values?? Array.Empty<string>())
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return data
      \-----------------------------------------------------------------------------------------------------------------------*/
      return relationship;

    }

    /*==========================================================================================================================
    | METHOD: WRITE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Serializes the data from JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, KeyValuesPair relationshipData, JsonSerializerOptions options) {
      writer.WriteStartObject();
      writer.WritePropertyName("Key");
      writer.WriteStringValue(relationshipData.Key.ToString());
      writer.WriteStartArray("Values");
      foreach (var uniqueKey in relationshipData.Values) {
        writer.WriteStringValue(uniqueKey);
      }
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

  }
}