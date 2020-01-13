/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;

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
    public static void Import(this Topic topic, TopicData topicData) {

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
      topic.ContentType         = topic.ContentType;

      if (topicData.DerivedTopicKey?.Length > 0) {
        topic.DerivedTopic      = rootTopic.FindByUniqueKey(topicData.DerivedTopicKey);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in topicData.Attributes) {
        var matchedAttribute = topic.Attributes.FirstOrDefault(a => a.Key == attribute.Key);
        if (attribute.Key?.Length > 0 && (matchedAttribute?.LastModified?? DateTime.MinValue) < attribute.LastModified) {
          topic.Attributes.SetValue(
            attribute.Key,
            attribute.Value
          );
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
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
      foreach (var childTopicData in topicData.Children) {
        if (childTopicData.Key?.Length > 0 && childTopicData.ContentType?.Length > 0) {
          var childTopic = topic.Children.GetTopic(childTopicData.Key);
          if (childTopic == null) {
            childTopic = TopicFactory.Create(childTopicData.Key, childTopicData.ContentType, topic);
          }
          childTopic.Import(childTopicData);
        }
      }

    }

    /*==========================================================================================================================
    | FIND BY UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root <see cref="Topic"/>, finds the <see cref="Topic"/> with the supplied .
    /// </summary>
    private static Topic? FindByUniqueKey(this Topic topic, string uniqueKey) {
      if (String.IsNullOrEmpty(uniqueKey)) {
        return topic;
      }

          uniqueKey             = uniqueKey.Replace("Root:", "");
      var firstColon            = uniqueKey.IndexOf(":");
      var firstKey              = uniqueKey.Substring(0, firstColon > 0? firstColon : uniqueKey.Length);
      var subsequentKey         = firstColon > 0? uniqueKey.Substring(firstColon+1) : "";

      return topic.Children.GetTopic(firstKey)?.FindByUniqueKey(subsequentKey);

    }

  } //Class
} //Namespace