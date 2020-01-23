/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
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
    private                     bool                            _includeNestedTopics            = false;

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
    public bool IncludeExternalReferences { get; set; } = false;

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
      get => IncludeChildTopics? true : _includeNestedTopics;
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
    public bool IncludeChildTopics { get; set; } = false;

  } //Class
} //Namespace