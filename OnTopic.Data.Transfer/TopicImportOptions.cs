/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Metadata;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: TOPIC IMPORT OPTIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides options for configuring the <see cref="TopicExtensions.Import(Topic, TopicData)"/> method.
  /// </summary>
  /// <remarks>
  ///   When importing <see cref="TopicData"/> into an existing <see cref="Topic"/> object, there are a lot of potential
  ///   conflicts that can happen, such as existing children, topics, or relationships. By default, the import method will take
  ///   the safest approach; it will add new child topics, attribute values, and relationships, but won't overwrite, update, or
  ///   delete existing ones. Additionally, it will throw an error if it is unable to locate a relationship target. The <see
  ///   cref="TopicImportOptions"/> class allows that behavior to be overridden, allowing more invasive imports to be performed.
  /// </remarks>
  public class TopicImportOptions {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     bool?                           _deleteUnmatchedAttributes;
    private                     bool?                           _deleteUnmatchedRelationships;
    private                     bool?                           _deleteUnmatchedChildren;
    private                     bool?                           _deleteUnmatchedNestedTopics;
    private                     bool?                           _overwriteContentType;

    /*==========================================================================================================================
    | IMPORT STRATEGY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the default <see cref="ImportStrategy"/>, which controls the default valuesfor many of the subsequent
    ///   import configuration settings.
    /// </summary>
    /// <remarks>
    ///   The <see cref="Strategy"/> defaults to the safest option—<see cref="ImportStrategy.Add"/>—which will never overwrite
    ///   any existing attributes.
    /// </remarks>
    public ImportStrategy Strategy { get; set; } = ImportStrategy.Add;

    /*==========================================================================================================================
    | DELETE UNMATCHED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Delete attributes in the target database which don't exist in the source <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will delete any existing attributes that aren't <i>also</i> represented in the source <see cref="TopicData"/>
    ///   graph. You should only use this if you are confident that the source data is comprehensive (i.e., doesn't just include
    ///   updates relevant to a particular feature).
    /// </remarks>
    public bool DeleteUnmatchedAttributes {
      get => _deleteUnmatchedAttributes?? Strategy.Equals(ImportStrategy.Replace);
      set => _deleteUnmatchedAttributes = value;
    }

    /*==========================================================================================================================
    | DELETE UNMATCHED RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Delete relationeships in the target database which don't exist in the source <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will delete any existing relationships that aren't <i>also</i> represented in the source <see cref="TopicData"/>
    ///   graph. This potentially includes any relationships that are using relationship keys not represented by the matched
    ///   <see cref="TopicData"/>.
    /// </remarks>
    public bool DeleteUnmatchedRelationships {
      get => _deleteUnmatchedRelationships?? Strategy.Equals(ImportStrategy.Replace);
      set => _deleteUnmatchedRelationships = value;
    }

    /*==========================================================================================================================
    | DELETE UNMATCHED CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Delete child topics the target database which don't exist in the source <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will delete any existing topics that aren't <i>also</i> represented in the source <see cref="TopicData"/>
    ///   graph. You should only use this if you are confident that the source data is comprehensive (i.e., doesn't just include
    ///   updates relevant to a particular feature).
    /// </remarks>
    public bool DeleteUnmatchedChildren {
      get => _deleteUnmatchedChildren?? Strategy.Equals(ImportStrategy.Replace);
      set => _deleteUnmatchedChildren = value;
    }

    /*==========================================================================================================================
    | DELETE UNMATCHED NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Delete nested topics in the target database which don't exist in the source <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will delete any existing nested topics that aren't <i>also</i> represented in the source <see cref="TopicData"/>
    ///   graph. You should only use this if you are confident that the source data is comprehensive (i.e., doesn't just include
    ///   updates relevant to a particular feature).
    /// </remarks>
    public bool DeleteUnmatchedNestedTopics {
      get => _deleteUnmatchedNestedTopics?? Strategy.Equals(ImportStrategy.Replace);
      set => _deleteUnmatchedNestedTopics = value;
    }

    /*==========================================================================================================================
    | OVERWRITE CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Update the <see cref="Topic.ContentType"/> if the source <see cref="TopicData.ContentType"/> value is different.
    /// </summary>
    /// <remarks>
    ///   Changing the <see cref="Topic.ContentType"/> will alter not only what <see cref="Topic.Attributes"/> are exposed on
    ///   the editor but, more importantly, may also change what template is displayed for that <see cref="Topic"/>, assuming
    ///   it is accessible by users. If the new <see cref="ContentTypeDescriptor"/> doesn't have a template associated with it,
    ///   then this could be a breaking change. As such, it is only enabled, by default, for cases where the <see
    ///   cref="Strategy"/> is set to <see cref="ImportStrategy.Overwrite"/> or <see cref="ImportStrategy.Replace"/>.
    /// </remarks>
    public bool OverwriteContentType {
      get => _overwriteContentType?? Strategy.Equals(ImportStrategy.Overwrite) || Strategy.Equals(ImportStrategy.Replace);
      set => _overwriteContentType = value;
    }

  } //Class
} //Namespace