/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using OnTopic.Collections.Specialized;
using OnTopic.Data.Transfer.Converters;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: KEY/VALUES PAIR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="KeyValuesPair"/> class provides an intermediary data transfer object for facilitating the interchange
  ///   of <see cref="KeyValuesPair{TKey, TValue}"/> objects with JSON data.
  /// </summary>
  /// <remarks>
  ///   Unlike the <see cref="KeyValuesPair{TKey, TValue}"/> class which is used by the <see cref="TopicMultiMap"/>, the <see
  ///   cref="KeyValuesPair"/> data transfer object maps to a collection of strings representing <see cref="TopicData.UniqueKey"
  ///   /> references, thus providing a serializable format for e.g. <see cref="TopicData.Relationships"/>.
  /// </remarks>
  [JsonConverter(typeof(KeyValuesPairConverter))]
  public class KeyValuesPair {

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Key"/> of the <see cref="KeyValuesPair"/>.
    /// </summary>
    [NotNull, DisallowNull]
    public string? Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Values"/> of the <see cref="KeyValuesPair"/>.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="Values"/> collection can contain any <see cref="String"/> value, it is intended to represent <see
    ///   cref="TopicData.UniqueKey"/> references.
    /// </remarks>
    public Collection<string> Values { get; init; } = new();

  } //Class
} //Namespace