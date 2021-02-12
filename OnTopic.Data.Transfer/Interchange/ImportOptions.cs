/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | CLASS: IMPORT OPTIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides options for configuring the <see cref="TopicExtensions.Import(Topic, TopicData)"/> method.
  /// </summary>
  /// <remarks>
  ///   When importing <see cref="TopicData"/> into an existing <see cref="Topic"/> object, there are a lot of potential
  ///   conflicts that can happen, such as existing children, topics, or relationships. By default, the import method will take
  ///   the safest approach; it will add new child topics, attribute values, and relationships, but won't overwrite, update, or
  ///   delete existing ones. Additionally, it will throw an error if it is unable to locate a relationship target. The <see
  ///   cref="ImportOptions"/> class allows that behavior to be overridden, allowing more invasive imports to be performed.
  /// </remarks>
  public class ImportOptions {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     bool?                           _deleteUnmatchedAttributes;
    private                     bool?                           _deleteUnmatchedRelationships;
    private                     bool?                           _deleteUnmatchedReferences;
    private                     bool?                           _deleteUnmatchedChildren;
    private                     bool?                           _deleteUnmatchedNestedTopics;
    private                     bool?                           _overwriteContentType;

    /*==========================================================================================================================
    | IMPORT STRATEGY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the default <see cref="ImportStrategy"/>, which controls the default values for many of the subsequent
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
      get => _deleteUnmatchedAttributes?? Strategy is ImportStrategy.Replace;
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
      get => _deleteUnmatchedRelationships?? Strategy is ImportStrategy.Replace;
      set => _deleteUnmatchedRelationships = value;
    }

    /*==========================================================================================================================
    | DELETE UNMATCHED REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Delete references in the target database which don't exist in the source <see cref="TopicData"/> graph.
    /// </summary>
    /// <remarks>
    ///   This will delete any existing references that aren't <i>also</i> represented in the source <see cref="TopicData"/>
    ///   graph.
    /// </remarks>
    public bool DeleteUnmatchedReferences {
      get => _deleteUnmatchedReferences?? Strategy is ImportStrategy.Replace;
      set => _deleteUnmatchedReferences = value;
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
      get => _deleteUnmatchedChildren?? Strategy is ImportStrategy.Replace;
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
      get => _deleteUnmatchedNestedTopics?? Strategy is ImportStrategy.Replace;
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
      get => _overwriteContentType?? Strategy is ImportStrategy.Overwrite or ImportStrategy.Replace;
      set => _overwriteContentType = value;
    }

    /*==========================================================================================================================
    | CURRENT USER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Optionally provides the current user to be used for the <c>LastModifiedBy</c> attribute, when appropriate.
    /// </summary>
    /// <remarks>
    ///   If supplied, the <see cref="CurrentUser"/> will be used when either the <see cref="LastModifiedByStrategy"/> is set to
    ///   <see cref="LastModifiedImportStrategy.Current"/>, or there is no <c>LastModifiedBy</c> value. Any exception to the
    ///   latter rule is if the <see cref="LastModifiedByStrategy"/> is set to <see cref="LastModifiedImportStrategy.Current"/>;
    ///   in that case, the <c>LastModifiedBy</c> attribute will be set to <c>System</c>.
    /// </remarks>
    public string CurrentUser { get; set; } = "System";

    /*==========================================================================================================================
    | LAST MODIFIED STRATEGY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the <see cref="LastModifiedImportStrategy"/>, allowing the <c>LastModified</c> value to be handled
    ///   independent of other values.
    /// </summary>
    /// <remarks>
    ///   When importing values from an external topic graph, it may be preferrable to maintain the existing value for the
    ///   <c>LastModified</c> date, overwrite it with the current time, or maintain the standard <see cref="Strategy"/> used for
    ///   other attributes. The <see cref="LastModifiedStrategy"/> offers that flexibility.
    /// </remarks>
    public LastModifiedImportStrategy LastModifiedStrategy { get; set; } = LastModifiedImportStrategy.Inherit;

    /*==========================================================================================================================
    | LAST MODIFIED BY STRATEGY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the <see cref="LastModifiedImportStrategy"/>, allowing the <c>LastModifiedBy</c> value to be handled
    ///   independent of other values.
    /// </summary>
    /// <remarks>
    ///   When importing values from an external topic graph, it may be preferrable to maintain the existing value for the
    ///   <c>LastModifiedBy</c> user, overwrite it with the current user, or credit the change to the <c>System</c>. The
    ///   <see cref="LastModifiedByStrategy"/> offers that flexibility.
    /// </remarks>
    public LastModifiedImportStrategy LastModifiedByStrategy { get; set; } = LastModifiedImportStrategy.Inherit;

  } //Class
} //Namespace