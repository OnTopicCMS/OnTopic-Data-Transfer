/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer {

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
      "ParentId",
      "ContentType",
      "TopicId"
    };

    /*==========================================================================================================================
    | EXPORT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exports a <see cref="Topic"/> entity—and, potentially, its descendants—into a <see cref="TopicData"/> data transfer
    ///   object.
    /// </summary>
    public static TopicData Export(this Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

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
      foreach (var attribute in topic.Attributes.Where(a => !ReservedAttributeKeys.Any(r => r == a.Key))) {
        topicData.Attributes.Add(
          new AttributeData() {
            Key                 = attribute.Key,
            Value               = attribute.Value,
            LastModified        = attribute.LastModified
          }
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var relationship in topic.Relationships) {
        var relationshipData = new RelationshipData() {
          Key                 = relationship.Name,
        };
        relationshipData.Relationships.AddRange(relationship.Select(r => r.GetUniqueKey()).ToList());
        topicData.Relationships.Add(relationshipData);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        topicData.Children.Add(
          childTopic.Export()
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topicData;

    }

    /*==========================================================================================================================
    | IMPORT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Imports a <see cref="TopicData"/> data transfer object—and, potentially, its descendants—into an existing <see
    ///   cref="Topic"/> entity.
    /// </summary>
    public static void Import(this Topic topic, TopicData topicData, [NotNull]TopicImportOptions? options = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(topicData, nameof(topicData));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default options
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options == null) {
        options                 = new TopicImportOptions() {
          Strategy              = ImportStrategy.Add
        };
      }

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
      | Identify root topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = topic;

      while (rootTopic.Parent != null) {
        rootTopic = rootTopic.Parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set primary properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options.OverwriteContentType) {
        topic.ContentType       = topicData.ContentType;
      }

      if (topicData.DerivedTopicKey?.Length > 0) {
        topic.DerivedTopic      = rootTopic.FindByUniqueKey(topicData.DerivedTopicKey)?? topic.DerivedTopic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedAttributes) {
        foreach(var attribute in topic.Attributes.Where(a1 => !topicData.Attributes.Any(a2 => a1.Key == a2.Key)).ToArray()) {
          topic.Attributes.Remove(attribute);
        };
      }

      //Update records based on the source collection
      foreach (var attribute in topicData.Attributes) {
        var matchedAttribute = topic.Attributes.FirstOrDefault(a => a.Key == attribute.Key);
        if (matchedAttribute != null && isStrategy(ImportStrategy.Add)) continue;
        if (matchedAttribute?.LastModified >= attribute.LastModified && isStrategy(ImportStrategy.Merge)) continue;
        topic.Attributes.SetValue(
          attribute.Key,
          attribute.Value
        );
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
          var relatedTopic = rootTopic.FindByUniqueKey(relatedTopicKey);
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
            child.ContentType == "List" && options.DeleteUnmatchedNestedTopics ||
            child.ContentType != "List" && options.DeleteUnmatchedChildren
          ) {
            topic.Children.Remove(child);
          }
        };
      }

      if (options.DeleteUnmatchedNestedTopics) {
        foreach (var child in topic.Children.Where(t => t.ContentType == "List")) {
          topic.Children.Remove(child);
        };
      }

      //Update records based on the source collection
      foreach (var childTopicData in topicData.Children) {
        var childTopic = topic?.Children.GetTopic(childTopicData.Key);
        if (childTopic == null) {
          childTopic = TopicFactory.Create(childTopicData.Key, childTopicData.ContentType, topic);
        }
        childTopic.Import(childTopicData);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Is strategy?
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool isStrategy(params ImportStrategy[] strategies) => strategies.Contains<ImportStrategy>(options!.Strategy);

    }

    /*==========================================================================================================================
    | FIND BY UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root <see cref="Topic"/>, finds the <see cref="Topic"/> with the supplied .
    /// </summary>
    private static Topic? FindByUniqueKey(this Topic topic, string uniqueKey) {

      if (uniqueKey == null || uniqueKey.Length == 0) {
        return topic;
      }

          uniqueKey             = uniqueKey.Replace("Root:", "");
      var firstColon            = uniqueKey.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
      var firstKey              = uniqueKey.Substring(0, firstColon > 0? firstColon : uniqueKey.Length);
      var subsequentKey         = firstColon > 0? uniqueKey.Substring(firstColon+1) : "";

      return topic.Children.GetTopic(firstKey)?.FindByUniqueKey(subsequentKey);

    }

  } //Class
} //Namespace