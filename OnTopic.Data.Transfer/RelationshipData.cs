/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using OnTopic.Data.Transfer.Converters;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP DATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="RelationshipData"/> class provides an intermediary data transfer object for facilitating the interchange
  ///   of <see cref="NamedTopicCollection"/> objects with JSON data.
  /// </summary>
  /// <remarks>
  ///   Having a separate class for this serializing topic data introduces some overhead in converting the topic graph to and
  ///   from <see cref="TopicData"/> objects, but in turn greatly simplifies how the serialization process works, and provides
  ///   necessary flexibility in the import process to better account for merging data and handling potential conflicts.
  /// </remarks>
  [JsonConverter(typeof(RelationshipDataConverter))]
  public class RelationshipData {

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the key of the relationship.
    /// </summary>
    [NotNull, DisallowNull]
    public string? Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a collection of unique keys associated with related <see cref="Topic"/> entities.
    /// </summary>
    public Collection<string> Relationships { get; init; } = new();

  } //Class
} //Namespace