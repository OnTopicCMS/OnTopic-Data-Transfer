/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | CLASS: UNRESOLVED ASSOCIATION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Tracks unresolved associations so that they can be resolved later in the import process.
  /// </summary>
  /// <remarks>
  ///   During the <see cref="TopicExtensions.Import(Topic, TopicData, ImportOptions?)"/> process, associations—such as
  ///   relationships or topic references—may be present in the <see cref="TopicData"/> graph that aren't yet available in the
  ///   <see cref="Topic"/> graph. This can happen, for instance, because a <see cref="TopicData"/> instance is associated with
  ///   another <see cref="TopicData"/> instance that hasn't yet been imported, due to its location in the <see cref="TopicData"
  ///   /> graph. The <see cref="UnresolvedAssociation"/> class helps address this by tracking the <see cref="Key"/>,
  ///   <see cref="AssociationType"/>, <see cref="SourceTopic"/>, and <see cref="TargetTopicKey"/> needed to later create the
  ///   association.
  /// </remarks>
  internal record UnresolvedAssociation {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new instance of a <see cref="UnresolvedAssociation"/> record.
    /// </summary>
    /// <param name="key">The <see cref="Key"/> used to identify the association.</param>
    /// <param name="type">
    ///   The <see cref="AssociationType"/> determines if the association is a relationship or a topic reference.
    /// </param>
    /// <param name="source">The <see cref="TopicData.UniqueKey"/> of the source <see cref="TopicData"/>.</param>
    /// <param name="target">The <see cref="TopicData.UniqueKey"/> of the targer <see cref="TopicData"/>.</param>
    internal UnresolvedAssociation(AssociationType type, string key, Topic source, string target) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(type,   nameof(type));
      Contract.Requires(key,    nameof(key));
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      SourceTopic               = source;
      Key                       = key;
      AssociationType           = type;
      TargetTopicKey            = target;

    }

    /*==========================================================================================================================
    | PROPERTY: ASSOCIATION TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the collection that the association originates from, such as <see cref="TopicData.Relationships"/> or <see
    ///   cref="TopicData.References"/>.
    /// </summary>
    internal AssociationType AssociationType { get; init; }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The key used for the association.
    /// </summary>
    /// <remarks>
    ///   This maps to either the <see cref="RecordData.Key"/>, if the association is from <see cref="TopicData.References"/>,
    ///   or <see cref="KeyValuesPair.Key"/>, if the association is from <see cref="TopicData.Relationships"/>.
    /// </remarks>
    internal string Key { get; init; }

    /*==========================================================================================================================
    | PROPERTY: SOURCE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="Topic"/> that the association should be created on.
    /// </summary>
    internal Topic SourceTopic { get; init; }

    /*==========================================================================================================================
    | PROPERTY: TARGET TOPIC KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="TopicData.UniqueKey"/> of the target <see cref="Topic"/> that the association points to.
    /// </summary>
    internal string TargetTopicKey { get; init; }

  } //Enum
} //Namespace