/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Attributes;
using OnTopic.Metadata;

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | ENUM: IMPORT STRATEGY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Enum that specifies the import strategy to use with e.g., <see cref="TopicExtensions.Import(Topic, TopicData,
  ///   TopicImportOptions)"/>.
  /// </summary>
  public enum ImportStrategy {

    /*==========================================================================================================================
    | ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports all records that don't require modifying existing attributes. Adds new topics, attributes, and relationships,
    ///   but doesn't update any existing attributes.
    /// </summary>
    /// <remarks>
    ///   This is the safest of the actual import strategies, in that it will add any missing data, but always defers to any
    ///   existing attributes. It is useful, for instance, for cases where both the source and the target database started from
    ///   a shared authoritative source, but both have made modifications in the interim; in that case, it prioritizes
    ///   modifications made to the target database, while still importing any <i>additional</i> modifications that are missing.
    /// </remarks>
    Add                         = 1,

    /*==========================================================================================================================
    | MERGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports all records <i>unless</i> the <see cref="AttributeData.LastModified"/> is <i>older</i> than the target <see
    ///   cref="AttributeValue.LastModified"/>.
    /// </summary>
    /// <remarks>
    ///   This is generally the <i>preferred</i> import strategy. It imports all new topics, relationships, and attributes,
    ///   while also updating any attributes that were changes <i>after</i> the current values. Further, any attribute values
    ///   that it <i>does</i> overwrite will be recoverable as part of the attribute versioning schema. That said, it is still
    ///   subject to potential version conflicts—e.g., if both the target database and the source database made subsequent
    ///   modifications to a common data source, the modifications in the source database will get overwritten if the
    ///   modifications to the target database happened earlier. Be aware that this will <i>not</i> overwrite the <see
    ///   cref="Topic.ContentType"/> since it is not versioned and, therefore, it is not possible to reliably evaluate the
    ///   relative age of the records.
    /// </remarks>
    Merge                       = 2,

    /*==========================================================================================================================
    | OVERWRITE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports all records, <i>even if</i> the <see cref="AttributeData.LastModified"/> is <i>older</i> than the target <see
    ///   cref="AttributeValue.LastModified"/>.
    /// </summary>
    /// <remarks>
    ///   This will effectively <i>overwrite</i> any attributes that already exist in the database, even if they were
    ///   modified <i>after</i> the source <see cref="AttributeData"/>. For example, if the <see cref="TopicData"/> starts at
    ///   <code>Root:Configuration</code>, any modifications to out-of-the-box titles, descriptions, visibility, sort order, &c.
    ///   will be overwritten with the new values. Any attribute values that it overwrites will still be recoverable as part of
    ///   the attribute versioning schema, with the exception of <see cref="Topic.ContentType"/>, which is excluded from
    ///   versioning.
    /// </remarks>
    Overwrite                   = 3,

    /*==========================================================================================================================
    | REPLACE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports all records, <i>and</i> deletes any topics, attributes, or relationships that aren't reflected in the source
    ///   <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will effectively <i>eliminate</i> any and all customizations done within the scope of the import. For example, if
    ///   the <see cref="TopicData"/> starts at <code>Root:Configuration</code>, this will delete any custom <see
    ///   cref="ContentTypeDescriptor"/>s, metadata registrations, &c. made to the database. This should be used with great
    ///   care. Any attribute values that it overwrites will still be recoverable as part of
    ///   the attribute versioning schema, with the exception of <see cref="Topic.ContentType"/>, which is excluded from
    ///   versioning.
    /// </remarks>
    Replace                     = 4

  } //Enum
} //Namespace