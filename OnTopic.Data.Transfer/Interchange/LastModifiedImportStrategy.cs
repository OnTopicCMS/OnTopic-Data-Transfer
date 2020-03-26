/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | ENUM: LAST MODIFIED IMPORT STRATEGY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Enum that specifies the import strategy to use with the <c>LastUpdated</c> and <c>LastUpdatedBy</c> properties.
  /// </summary>
  /// <remarks>
  ///   When importing a topic graph via <see cref="TopicExtensions.Import(Topic, TopicData, ImportOptions)"/>, the existing
  ///   <c>LastUpdated</c> and <c>LastUpdatedBy</c> fields merit special consideration. It may not be appropriate, for example,
  ///   to overwrite the local <c>LastUpdatedBy</c> (author) when importing from an external topic graph. Similarly, it may be
  ///   preferred to maintain the existing <c>LastUpdated</c> date instead of updating all affected topics to the current date.
  ///   The <see cref="LastModifiedImportStrategy"/> provides control over these options.
  /// </remarks>
  public enum LastModifiedImportStrategy {

    /*==========================================================================================================================
    | INHERIT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Falls back to the <see cref="ImportStrategy"/> specified as part of the <see cref="ImportOptions.Strategy"/>. This is
    ///   the default value.
    /// </summary>
    /// <remarks>
    ///   When <see cref="Inherit"/> is chosen, existing values will be retained if <see cref="ImportStrategy.Add"/> is
    ///   selected, overwritten if <see cref="ImportStrategy.Overwrite"/> or <see cref="ImportStrategy.Replace"/> are selected,
    ///   and conditionally overwritten based on the <see cref="AttributeData.LastModified"/> date if <see
    ///   cref="ImportStrategy.Merge"/> is selected.
    /// </remarks>
    Inherit                     = 1,

    /*==========================================================================================================================
    | TARGET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The existing value will always be maintained, regardless of the <see cref="ImportOptions.Strategy"/>.
    /// </summary>
    /// <remarks>
    ///   When importing from an external topic graph, such as a reference graph, the values for <c>LastUpdated</c> and
    ///   <c>LastUpdatedBy</c> may not be relevant to the target topic graph, and especially if those values are being exposed
    ///   publicly as part of e.g. a page template. In that case, always deferring to the <see cref="TargetValue"/> may make
    ///   sense, independent of whether other values were updated.
    /// </remarks>
    TargetValue                 = 2,

    /*==========================================================================================================================
    | CURRENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   If any values have been overwritten, then update to the current value, independent of the source or target values.
    /// </summary>
    /// <remarks>
    ///   When updating the <c>LastModified</c> value, the current time will be used, even if it is after the date of either the
    ///   source or the target topic. When updating the <c>LastModifiedBy</c> value, the currently authenticated user will be
    ///   used, even if it is different than either the source or the target topic. This treats the import the same as though
    ///   the current user went through and manually updated each of the topics.
    /// </remarks>
    Current                     = 3,

    /*==========================================================================================================================
    | SYSTEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   If any values have been overwritten, then update to the current system, independent of the source or target values.
    /// </summary>
    /// <remarks>
    ///   When updating the <c>LastModified</c> value, the current time will be used, even if it is after the date of either the
    ///   source or the target topic. When updating the <c>LastModifiedBy</c> value, the <c>System</c> user will be used, even
    ///   if it is different than either the source or the target topic. This is useful for cases where the current user is not
    ///   relevant, but it's still preferred to distinguish the <c>LastModified</c> and <c>LastModifiedBy</c> values from either
    ///   the preexisting source or target values. For instance, this may be used with an automated job or when the
    ///   <c>Configuration</c> tree is being updated by a third-party developer.
    /// </remarks>
    System                      = 4

  } //Enum
} //Namespace