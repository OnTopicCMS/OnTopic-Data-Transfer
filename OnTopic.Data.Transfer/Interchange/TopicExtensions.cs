/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;

namespace OnTopic.Data.Transfer.Interchange {

  /*============================================================================================================================
  | CLASS: TOPIC EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicExtensions"/> class provides extension methods for <see cref="Topic"/> for converting entities to
  ///   and from <see cref="TopicData"/> data transfer objects.
  /// </summary>
  /// <remarks>
  ///   The serialization and deserialization of <see cref="TopicData"/> is intended to be a simple, fast, and predictable
  ///   process since it's simply translating the JSON representation into a .NET class. By contrast, converting a <see
  ///   cref="TopicData"/> graph into a <see cref="Topic"/> graph—or vice versa—requires far more care. As such, these extension
  ///   methods should really be viewed as the heart of the import/export process; this is where all of the business logic,
  ///   user options, and safety checks will take place.
  /// </remarks>
  public static class TopicExtensions {

    /*==========================================================================================================================
    | RESERVED ATTRIBUTE KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents attribute keys that should not be stored in the <see cref="TopicData.Attributes"/> collection.
    /// </summary>
    private static string[] ReservedAttributeKeys { get; } = new string[] {
      "Key",
      "ParentID",
      "ContentType",
      "TopicID"
    };

    /*==========================================================================================================================
    | EXPORT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exports a <see cref="Topic"/> entity—and, potentially, its descendants—into a <see cref="TopicData"/> data transfer
    ///   object.
    /// </summary>
    /// <param name="topic">The source <see cref="Topic"/> to operate off of.</param>
    /// <param name="options">An optional <see cref="ExportOptions"/> object to specify export settings.</param>
    public static TopicData Export(this Topic topic, [NotNull]ExportOptions? options = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default options
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options == null) {
        options                 = new ExportOptions();
      }
      options.ExportScope       ??= topic.GetUniqueKey();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set primary properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicData             = new TopicData {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType,
        DerivedTopicKey         = topic.DerivedTopic?.GetUniqueKey()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = topic.Attributes.Where(a =>
        !ReservedAttributeKeys.Any(r =>
          r.Equals(a.Key, StringComparison.InvariantCultureIgnoreCase)
        )
      );

      foreach (var attribute in attributes) {
        topicData.Attributes.Add(
          new AttributeData() {
            Key                 = attribute.Key,
            Value               = getAttributeValue(attribute),
            LastModified        = attribute.LastModified
          }
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var relationship in topic.Relationships) {
        var relationshipData    = new RelationshipData() {
          Key                   = relationship.Name,
        };
        foreach (var relatedTopic in relationship) {
          if (
            options.IncludeExternalReferences ||
            relatedTopic.GetUniqueKey().StartsWith(options.ExportScope, StringComparison.InvariantCultureIgnoreCase)
          ) {
            relationshipData.Relationships.Add(relatedTopic.GetUniqueKey());
          }
        }
        if (relationshipData.Relationships.Count > 0) {
          topicData.Relationships.Add(relationshipData);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        if (
          options.IncludeChildTopics ||
          topic.ContentType == "List" ||
          options.IncludeNestedTopics &&
          childTopic.ContentType == "List"
        ) {
          topicData.Children.Add(
            childTopic.Export(options)
          );
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topicData;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      string? getAttributeValue(AttributeValue attribute) =>
        options.TranslateTopicPointers && attribute.Key.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase)?
          GetUniqueKey(topic, attribute.Value, options) :
          attribute.Value;

    }

    /*==========================================================================================================================
    | IMPORT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports a <see cref="TopicData"/> data transfer object—and, potentially, its descendants—into an existing <see
    ///   cref="Topic"/> entity.
    /// </summary>
    /// <param name="topic">The source <see cref="Topic"/> to operate off of.</param>
    /// <param name="options">An optional <see cref="ImportOptions"/> object to specify import settings.</param>
    public static void Import(this Topic topic, TopicData topicData, [NotNull]ImportOptions? options = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(topicData, nameof(topicData));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default options
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options == null) {
        options                 = new ImportOptions() {
          Strategy              = ImportStrategy.Add
        };
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Detect mismatches
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.GetUniqueKey() != topicData.UniqueKey) {
        throw new ArgumentException(
          $"A topic with the unique key of '{topicData.UniqueKey}' cannot be created under a topic with the unique key of " +
          $"{topic.GetUniqueKey()}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set primary properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options.OverwriteContentType) {
        topic.ContentType       = topicData.ContentType;
      }

      if (topicData.DerivedTopicKey?.Length > 0) {
        topic.DerivedTopic      = topic.GetByUniqueKey(topicData.DerivedTopicKey)?? topic.DerivedTopic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedAttributes) {
        var unmatchedAttributes = topic.Attributes.Where(a1 =>
          !ReservedAttributeKeys.Contains(a1.Key) &&
          !topicData.Attributes.Any(a2 => a1.Key == a2.Key)
        );
        foreach (var attribute in unmatchedAttributes.ToArray()) {
          topic.Attributes.Remove(attribute);
        };
      }

      //Update records based on the source collection
      foreach (var attribute in topicData.Attributes) {
        if (useCustomMergeRules(attribute)) continue;
        var matchedAttribute = topic.Attributes.FirstOrDefault(a => a.Key == attribute.Key);
        if (matchedAttribute != null && isStrategy(ImportStrategy.Add)) continue;
        if (matchedAttribute?.LastModified >= attribute.LastModified && isStrategy(ImportStrategy.Merge)) continue;
        topic.Attributes.SetValue(
          attribute.Key,
          getAttributeValue(attribute)
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle special rules for LastModified(By) attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Attributes.IsDirty()) {

        switch (options.LastModifiedStrategy) {
          case LastModifiedImportStrategy.Current:
            topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            break;
          case LastModifiedImportStrategy.System:
            topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            break;
        }

        switch (options.LastModifiedByStrategy) {
          case LastModifiedImportStrategy.Current:
            topic.Attributes.SetValue("LastModifiedBy", options.CurrentUser);
            break;
          case LastModifiedImportStrategy.System:
            topic.Attributes.SetValue("LastModifiedBy", "System");
            break;
        }

        if (topic.Attributes.GetValue("LastModified", null) == null) {
          topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.CurrentCulture));
        }

        if (topic.Attributes.GetValue("LastModifiedBy", null) == null) {
          topic.Attributes.SetValue("LastModifiedBy", options.CurrentUser);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedRelationships) {
        topic.Relationships.Clear();
      }

      //Update records based on the source collection
      foreach (var relationship in topicData.Relationships) {
        foreach (var relatedTopicKey in relationship.Relationships) {
          var relatedTopic = topic.GetByUniqueKey(relatedTopicKey);
          if (relationship.Key != null && relatedTopic != null) {
            topic.Relationships.SetTopic(relationship.Key, relatedTopic);
          };
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedChildren || options.DeleteUnmatchedNestedTopics) {
        foreach (var child in topic.Children.Where(t1 => !topicData.Children.Any(t2 => t1.Key == t2.Key)).ToArray()) {
          if (
            topic.ContentType == "List" && options.DeleteUnmatchedNestedTopics ||
            topic.ContentType != "List" && options.DeleteUnmatchedChildren
          ) {
            topic.Children.Remove(child);
          }
        };
      }

      //Update records based on the source collection
      foreach (var childTopicData in topicData.Children) {
        var childTopic = topic?.Children.GetTopic(childTopicData.Key);
        if (childTopic == null) {
          childTopic = TopicFactory.Create(childTopicData.Key, childTopicData.ContentType, topic);
        }
        childTopic.Import(childTopicData, options);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Is strategy?
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool isStrategy(params ImportStrategy[] strategies) => strategies.Contains<ImportStrategy>(options!.Strategy);

      /*------------------------------------------------------------------------------------------------------------------------
      | Is custom merge rules?
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool useCustomMergeRules(AttributeData attribute) =>
        (attribute.Key == "LastModified" && !options!.LastModifiedStrategy.Equals(LastModifiedImportStrategy.Inherit)) ||
        (attribute.Key == "LastModifiedBy" && !options!.LastModifiedByStrategy.Equals(LastModifiedImportStrategy.Inherit));

      /*------------------------------------------------------------------------------------------------------------------------
      | Get attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      string? getAttributeValue(AttributeData attribute) =>
        attribute.Key.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase)?
          GetTopicId(topic, attribute.Value) :
          attribute.Value;

    }

    /*==========================================================================================================================
    | GET UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <c>TopicID</c>, lookup the topic in the topic graph and return the fully-qualified value. If no value can be
    ///   found, the original <c>TopicID</c> is returned.
    /// </summary>
    /// <param name="topic">The source <see cref="Topic"/> to operate off of.</param>
    /// <param name="topicId">The <see cref="Topic.Id"/> to retrieve the <see cref="Topic.GetUniqueKey"/> for.</param>
    /// <param name="options">An optional <see cref="ExportOptions"/> object to specify export settings.</param>
    private static string? GetUniqueKey(Topic topic, string? topicId, ExportOptions options) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ignore empty or non-numeric values
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(topicId) || !Int32.TryParse(topicId, out var id)) {
        return topicId;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish scope
      \-----------------------------------------------------------------------------------------------------------------------*/
      var uniqueKey             = topic.GetRootTopic().FindFirst(t => t.Id == id)?.GetUniqueKey();
      var exportScope           = options.IncludeExternalReferences? "Root" : options.ExportScope;

      /*------------------------------------------------------------------------------------------------------------------------
      | Return in-scope values
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (uniqueKey?.StartsWith(exportScope, StringComparison.InvariantCultureIgnoreCase)?? false) {
        return uniqueKey;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Default to null
      \-----------------------------------------------------------------------------------------------------------------------*/
      return null;

    }

    /*==========================================================================================================================
    | GET TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <c>UniqueKey</c>, lookup the topic in the topic graph and return the <c>TopicID</c>. If no value can be
    ///   found, the original <c>UniqueKey</c> is returned.
    /// </summary>
    /// <param name="topic">The source <see cref="Topic"/> to operate off of.</param>
    /// <param name="uniqueKey">The <see cref="Topic.GetUniqueKey"/> to retrieve the <see cref="Topic.Id"/> for.</param>
    private static string? GetTopicId(Topic topic, string? uniqueKey) {
      if (!String.IsNullOrEmpty(uniqueKey) && uniqueKey!.StartsWith("Root", StringComparison.InvariantCultureIgnoreCase)) {
        return topic.GetByUniqueKey(uniqueKey)?.Id.ToString(CultureInfo.CurrentCulture)?? uniqueKey;
      }
      return uniqueKey;
    }

  } //Class
} //Namespace