/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

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