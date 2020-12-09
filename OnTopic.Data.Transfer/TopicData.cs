/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: TOPIC DATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicData"/> class provides an intermediary data transfer object for facilitating the interchange of
  ///   <see cref="Topic"/> entities with JSON data.
  /// </summary>
  /// <remarks>
  ///   Having a separate class for this serializing topic data introduces some overhead in converting the topic graph to and
  ///   from <see cref="TopicData"/> objects, but in turn greatly simplifies how the serialization process works, and provides
  ///   necessary flexibility in the import process to better account for merging data and handling potential conflicts.
  ///   Likewise, unlike the <see cref="Topic"/> class, the <see cref="TopicData"/> makes no effort to validate the data; it is
  ///   exclusively intended as a temporary, lightweight data interchange format, allowing the actual data migration process to
  ///   be handled by a separate service.
  /// </remarks>
  public class TopicData {

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="Key"/> attribute, the primary text identifier for the topic.
    /// </summary>
    [NotNull, DisallowNull]
    public string? Key { get; set; }

    /*==========================================================================================================================
    | UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the full, hierarchical identifier for the <see cref="Topic"/>, including parent keys.
    /// </summary>
    /// <remarks>
    ///   The value for the UniqueKey property is a collated, colon-delimited representation of the topic and its parent(s).
    ///   Example: "Root:Configuration:ContentTypes:Page".
    /// </remarks>
    [NotNull, DisallowNull]
    public string? UniqueKey { get; set; }

    /*==========================================================================================================================
    | CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the associated <see cref="Topic"/> represents.
    /// </summary>
    [NotNull, DisallowNull]
    public string? ContentType { get; set; }

    /*==========================================================================================================================
    | DERIVED TOPIC KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the <see cref="UniqueKey"/> of a <see cref="Topic"/> that the associated <see cref="Topic"/> should derive from.
    /// </summary>
    public string? DerivedTopicKey { get; set; }

    /*==========================================================================================================================
    | ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of <see cref="AttributeData"/> representing the attributes from the associated <see
    ///   cref="Topic"/> object.
    /// </summary>
    public AttributeDataCollection Attributes { get; set; } = new();

    /*==========================================================================================================================
    | RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of <see cref="RelationshipData"/> representing the relationships from the associated <see
    ///   cref="Topic"/> object.
    /// </summary>
    public RelationshipDataCollection Relationships { get; set; } = new();

    /*==========================================================================================================================
    | CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of <see cref="TopicData"/> objects representing the children of the associated <see
    ///   cref="Topic"/>.
    /// </summary>
    public List<TopicData> Children { get; set; } = new();

  } //Class
} //Namespace