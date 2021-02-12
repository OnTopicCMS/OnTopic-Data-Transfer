/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | CLASS: EXPORT OPTIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides options for configuring the <see cref="TopicExtensions.Export(Topic)"/> method.
  /// </summary>
  /// <remarks>
  ///   When exporting a <see cref="Topic"/> graph into a new <see cref="TopicData"/> object, there are a few considerations
  ///   should be taken into account, such as whether or not to include children. The <see cref="ExportOptions"/> class provides
  ///   a means for setting these configuration options.
  /// </remarks>
  public class ExportOptions {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     bool                            _includeNestedTopics;

    /*==========================================================================================================================
    | INCLUDE EXTERNAL REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether relationships pointing to <see cref="Topic"/>s outside the scope of the export should be included.
    /// </summary>
    /// <remarks>
    ///   By default, only relationships which point to <see cref="Topic"/>s within the currently scoped export will be
    ///   included as relationships. This avoids any potential issues matching topics when importing the data into an existing
    ///   topic graph. Optionally, however, callers may force all external references to be included, even if they aren't
    ///   represented in the currently scoped export. This is only recommended for cases where the caller is confident that the
    ///   external references will be available in the target database—as might be the case, for example, when combining the
    ///   exporting with other exports. It may also make when the purpose of the export is to act as a backup for a portion of a
    ///   topic graph, with the expected target being the same topic graph should a bulk-restore be required.
    /// </remarks>
    public bool IncludeExternalReferences { get; set; }

    /*==========================================================================================================================
    | EXPORT SCOPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines what the scope of the current export is.
    /// </summary>
    /// <remarks>
    ///   The <see cref="ExportScope"/> property is set internally by <see cref="TopicExtensions.Export(Topic, ExportOptions)"
    ///   /> on the initial call—but not on any recursive calls. The value is set to the <see cref="Topic.GetUniqueKey()"/> of
    ///   the initial <see cref="Topic"/> upon which <see cref="TopicExtensions.Export(Topic, ExportOptions)"/> is being called.
    ///   This is used in conjunction with <see cref="IncludeExternalReferences"/>; when <see cref="IncludeExternalReferences"/>
    ///   is set to <c>true</c>, the <see cref="ExportScope"/> is used to determine whether or not an external reference—such
    ///   as a relationship—is within scope or not by comparing the relationship's <see cref="Topic.GetUniqueKey()"/> to the
    ///   <see cref="ExportScope"/>.
    /// </remarks>
    [NotNull]
    internal string? ExportScope { get; set; }

    /*==========================================================================================================================
    | INCLUDE NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether any nested topics on the current <see cref="Topic"/> should be included in the export.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, only the current <see cref="Topic"/> is exported. Optionally, a caller may choose to also include any
    ///     nested topics under the current <see cref="Topic"/>. While these are technically separate <see cref="Topics"/>, it
    ///     may be desirable to include them since they are so closely related to the current <see cref="Topic"/>.
    ///   </para>
    ///   <para>
    ///     <see cref="IncludeNestedTopics"/> is <i>always</i> set to <c>true</c> if <see cref="IncludeChildTopics"/> is
    ///     enabled. That said, it can optionally be enabled even if <see cref="IncludeChildTopics"/> is set to <c>false</c>.
    ///   </para>
    /// </remarks>
    public bool IncludeNestedTopics {
      get => IncludeChildTopics || _includeNestedTopics;
      set => _includeNestedTopics = value;
    }

    /*==========================================================================================================================
    | INCLUDE CHILD TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether all child topics should be included in the export. This is recursive.
    /// </summary>
    /// <remarks>
    ///   By default, only the current <see cref="Topic"/> is exported. Optionally, a caller may choose to also include any
    ///   children under the current <see cref="Topic"/>. If enabled, this will be recursive and thus include <i>all</i>
    ///   descendents, including any nested topics.
    /// </remarks>
    public bool IncludeChildTopics { get; set; }

    /*==========================================================================================================================
    | TRANSLATE LEGACY TOPIC REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether attributes that appear to be topic reference should be mapped to its fully qualified unique key.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Strongly typed topic asociations, such as <see cref="Topic.Parent"/>, <see cref="Topic.Relationships"/>, and <see
    ///     cref="Topic.References"/>—which includes <see cref="Topic.BaseTopic"/>—are all translated from from <see cref="Topic
    ///     .Id"/> to <see cref="Topic.GetUniqueKey"/>. This ensures that the references can be repopulated on import even
    ///     though the <see cref="Topic.Id"/> will be different in each database.
    ///   </para>
    ///   <para>
    ///     In addition, however, there may be attributes that <i>behave</i> like topic associations, but aren't formally stored
    ///     using the aforementioned structures. Notably, these were used by previous versions of OnTopic Editor to stored
    ///     values derived from the <c>TopicList</c>, <c>TokenizedTopicList</c>, and <c>TopicReference</c> attribute types. By
    ///     convention, those references were named with a suffix of <c>ID</c> (e.g., <c>RootTopicID</c>). Optionally, the
    ///     export can attempt to map these to a <see cref="Topic.GetUniqueKey"/>, thus allowing them to maintain referential
    ///     integrity between databases.
    ///   </para>
    ///   <para>
    ///     As of OnTopic 5.0.0, these references should be upgraded to <see cref="Topic.References"/>, which are fully
    ///     supported by the OnTopic Data Transfer library via <see cref="TopicData.References"/>. As such, this option is only
    ///     needed for databases that continue to use the legacy format, potentially in order to maintain backward
    ///     compatibility.
    ///   </para>
    ///   <para>
    ///     It is important to note that this can introduce false positives. For example, if a database includes an attribute
    ///     referring to an external identifier, and whose name ends with <c>Id</c>, that value will be interpreted as a topic
    ///     association, assuming the value maps to an existing <see cref="Topic.Id"/>. As such, this option should only be
    ///     enabled when it's known to be necessary to maintain backward compatibility.
    ///   </para>
    /// </remarks>
    public bool TranslateLegacyTopicReferences { get; set; }

    [Obsolete("The TranslateTopicPointers option has been renamed to TranslateLegacyTopicReferences", true)]
    public bool TranslateTopicPointers { get; set; }

  } //Class
} //Namespace