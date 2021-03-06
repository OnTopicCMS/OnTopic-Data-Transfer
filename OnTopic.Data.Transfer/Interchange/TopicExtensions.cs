﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
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
      if (options is null) {
        options                 = new();
      }
      options.ExportScope       ??= topic.GetUniqueKey();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set primary properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicData             = new TopicData {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = topic.Attributes.Where(a =>
        !ReservedAttributeKeys.Any(r =>
          r.Equals(a.Key, StringComparison.OrdinalIgnoreCase)
        )
      );

      foreach (var attribute in attributes) {
        var attributeValue      = getAttributeValue(attribute);
        if (String.IsNullOrEmpty(attributeValue)) {
          continue;
        }
        topicData.Attributes.Add(
          new() {
            Key                 = attribute.Key,
            Value               = attributeValue,
            LastModified        = attribute.LastModified
          }
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set topic references
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var reference in topic.References) {
        if (
          !options.IncludeExternalAssociations &&
          !(reference.Value?.GetUniqueKey().StartsWith(options.ExportScope, StringComparison.InvariantCultureIgnoreCase)?? true)
        ) {
          continue;
        }
        topicData.References.Add(
          new() {
            Key               = reference.Key,
            Value             = reference.Value?.GetUniqueKey(),
            LastModified      = reference.LastModified
          }
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var relationship in topic.Relationships) {
        var relationshipData    = new KeyValuesPair() {
          Key                   = relationship.Key,
        };
        foreach (var relatedTopic in relationship.Values) {
          if (
            options.IncludeExternalAssociations ||
            relatedTopic.GetUniqueKey().StartsWith(options.ExportScope, StringComparison.InvariantCultureIgnoreCase)
          ) {
            relationshipData.Values.Add(relatedTopic.GetUniqueKey());
          }
        }
        if (relationshipData.Values.Count > 0) {
          topicData.Relationships.Add(relationshipData);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        if (
          options.IncludeChildTopics ||
          topic.ContentType is "List" ||
          options.IncludeNestedTopics &&
          childTopic.ContentType is "List"
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
      string? getAttributeValue(AttributeRecord attribute) =>
        options.TranslateLegacyTopicReferences && attribute.Key.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase)?
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
    /// <param name="topicData">The source <see cref="TopicData"/> graph to import into the <paramref name="topic"/>.</param>
    /// <param name="options">An optional <see cref="ImportOptions"/> object to specify import settings.</param>
    public static void Import(this Topic topic, TopicData topicData, [NotNull]ImportOptions? options = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      var unresolvedAssociations = new List<UnresolvedAssociation>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle first pass
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Import(topicData, options, unresolvedAssociations);

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to resolve outstanding assocations
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var association in unresolvedAssociations) {

        //Attempt to find the target association
        var target              = topic.GetByUniqueKey(association.TargetTopicKey);

        //If the association STILL can't be resolved, skip it
        if (target is null) {
          continue;
        }

        //Wire up relationships
        else if (association.AssociationType is AssociationType.Relationship) {
          association.SourceTopic.Relationships.SetValue(association.Key, target);
        }

        //Wire up topic references
        else {
          association.SourceTopic.References.SetValue(association.Key, target);
        }

      }

    }

    /// <summary>
    ///   Imports a <see cref="TopicData"/> data transfer object—and, potentially, its descendants—into an existing <see
    ///   cref="Topic"/> entity. Track unmatched relationships.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     While traversing a topic graph with many new topics, scenarios emerge where the topic graph cannot be fully
    ///     reconstructed since the relationships and referenced topics may refer to new topics that haven't yet been imported.
    ///     To mitigate that, this overload accepts and populates a cache of such associations, so that they can be recreated
    ///     afterwards.
    ///   </para>
    ///   <para>
    ///     This does <i>not</i> address the scenario where implicit topic pointers (i.e., attributes ending in <c>Id</c>)
    ///     cannot be resolved because the target topics haven't yet been saved—and, therefore, the <see cref="Topic.
    ///     GetUniqueKey"/> cannot be translated to a <see cref="Topic.Id"/>. There isn't any obvious way to address this via
    ///     <see cref="Import(Topic, TopicData, ImportOptions?)"/> directly.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The target <see cref="Topic"/> to write data to.</param>
    /// <param name="topicData">The source <see cref="TopicData"/> to import data from.</param>
    /// <param name="options">An optional <see cref="ImportOptions"/> object to specify import settings.</param>
    /// <param name="unresolvedAssociations">A list of associations that could not be resolved on the first </param>
    private static void Import(
      this Topic topic,
      TopicData topicData,
      [NotNull]ImportOptions? options,
      List<UnresolvedAssociation> unresolvedAssociations
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(topicData, nameof(topicData));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default options
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (options is null) {
        options                 = new() {
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

      #pragma warning disable CS0618 // Type or member is obsolete
      if (topicData.DerivedTopicKey?.Length > 0 && !topicData.References.Contains("BaseTopic")) {
        topicData.References.Add(
          new() {
            Key                 = "BaseTopic",
            Value               = topicData.DerivedTopicKey
          }
        );
      }
      #pragma warning restore CS0618 // Type or member is obsolete

      /*------------------------------------------------------------------------------------------------------------------------
      | Migrate legacy topic references
      >-------------------------------------------------------------------------------------------------------------------------
      | Previous versions of the OnTopic Data Transfer library identified attributes that ended with `Id` and had a value that
      | mapped to a `topic.Id` and translated their values to `topic.GetUniqueKey()` so that they could be translated back to
      | topic references. As of OnTopic 5 and OnTopic Data Transfer 3, this functionality is now formalized as `topic.
      | References`. The following provides legacy support for this encoding by migrating these attributes to `topicData.
      | References`, thus allowing them to be handled the same way as other topic references.
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in topicData.Attributes.ToList()) {
        if (
          attribute.Value is not null &&
          attribute.Key.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase) &&
          attribute.Value.StartsWith("Root", StringComparison.InvariantCultureIgnoreCase)
        ) {
          var referenceKey = attribute.Key.Substring(0, attribute.Key.Length-2);
          if (referenceKey is "Topic") {
            referenceKey = "BaseTopic";
          }
          if (!topicData.References.Contains(referenceKey)) {
            attribute.Key = referenceKey;
            topicData.Attributes.Remove(attribute);
            topicData.References.Add(attribute);
          }
        }
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
        if (matchedAttribute is not null && isStrategy(ImportStrategy.Add)) continue;
        if (matchedAttribute?.LastModified >= attribute.LastModified && isStrategy(ImportStrategy.Merge)) continue;
        topic.Attributes.SetValue(
          attribute.Key,
          attribute.Value
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle special rules for LastModified(By) attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Attributes.IsDirty()) {

        if (options.LastModifiedStrategy is LastModifiedImportStrategy.Current or LastModifiedImportStrategy.System) {
          topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.CurrentCulture));
        }

        switch (options.LastModifiedByStrategy) {
          case LastModifiedImportStrategy.Current:
            topic.Attributes.SetValue("LastModifiedBy", options.CurrentUser);
            break;
          case LastModifiedImportStrategy.System:
            topic.Attributes.SetValue("LastModifiedBy", "System");
            break;
        }

        if (topic.Attributes.GetValue("LastModified", null) is null) {
          topic.Attributes.SetValue("LastModified", DateTime.Now.ToString(CultureInfo.CurrentCulture));
        }

        if (topic.Attributes.GetValue("LastModifiedBy", null) is null) {
          topic.Attributes.SetValue("LastModifiedBy", options.CurrentUser);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedRelationships) {
        foreach (var relationship in topic.Relationships) {
          topic.Relationships.Clear(relationship.Key);
        }
      }

      //Update records based on the source collection
      foreach (var relationship in topicData.Relationships) {
        foreach (var relatedTopicKey in relationship.Values) {
          var relatedTopic = topic.GetByUniqueKey(relatedTopicKey);
          if (relationship.Key is not null && relatedTopic is not null) {
            topic.Relationships.SetValue(relationship.Key, relatedTopic);
          }
          else {
            unresolvedAssociations.Add(new(AssociationType.Relationship, relatedTopicKey, topic, relatedTopicKey));
          }
        }
      }


      /*------------------------------------------------------------------------------------------------------------------------
      | Set topic references
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedReferences) {
        var unmatchedReferences = topic.References.Where(a1 =>
          !topicData.References.Any(a2 => a1.Key == a2.Key)
        );
        foreach (var reference in unmatchedReferences.ToArray()) {
          topic.References.Remove(reference);
        };
      }

      //Update records based on the source collection
      foreach (var reference in topicData.References) {
        var matchedReference = topic.References.FirstOrDefault(a => a.Key == reference.Key);
        if (matchedReference is not null && isStrategy(ImportStrategy.Add)) continue;
        if (matchedReference?.LastModified >= reference.LastModified && isStrategy(ImportStrategy.Merge)) continue;
        var referencedTopic = topic.GetByUniqueKey(reference.Key);
        if (reference.Value is null || referencedTopic != null) {
          topic.References.SetValue(
            reference.Key,
            referencedTopic
          );
        }
        else {
          unresolvedAssociations.Add(new(AssociationType.Reference, reference.Key, topic, reference.Value));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/

      //First delete any unmatched records, if appropriate
      if (options.DeleteUnmatchedChildren || options.DeleteUnmatchedNestedTopics) {
        foreach (var child in topic.Children.Where(t1 => !topicData.Children.Any(t2 => t1.Key == t2.Key)).ToArray()) {
          if (
            topic.ContentType is "List" && options.DeleteUnmatchedNestedTopics ||
            topic.ContentType is not "List" && options.DeleteUnmatchedChildren
          ) {
            topic.Children.Remove(child);
          }
        };
      }

      //Update records based on the source collection
      foreach (var childTopicData in topicData.Children) {
        var childTopic = topic?.Children.GetValue(childTopicData.Key);
        if (childTopic is null) {
          childTopic = TopicFactory.Create(childTopicData.Key, childTopicData.ContentType, topic);
        }
        childTopic.Import(childTopicData, options, unresolvedAssociations);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Is strategy?
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool isStrategy(params ImportStrategy[] strategies) => strategies.Contains<ImportStrategy>(options!.Strategy);

      /*------------------------------------------------------------------------------------------------------------------------
      | Is custom merge rules?
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool useCustomMergeRules(RecordData attribute) =>
        (attribute.Key is "LastModified" && options!.LastModifiedStrategy is not LastModifiedImportStrategy.Inherit) ||
        (attribute.Key is "LastModifiedBy" && options!.LastModifiedByStrategy is not LastModifiedImportStrategy.Inherit);

    }

    /*==========================================================================================================================
    | GET UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <c>TopicID</c>, lookup the topic in the topic graph and return the fully-qualified value. If no value can be
    ///   found, the original <c>TopicID</c> is returned.
    /// </summary>
    /// <remarks>
    ///   Note that this function is <i>exclusively</i> required for maintaining backward compatibility the <see cref="
    ///   ExportOptions.TranslateLegacyTopicReferences"/> option/ With the release of OnTopic 5.0.0, and OnTopic Data Transfer
    ///   2.0.0, implementers should prefer the use of <see cref="Topic.References"/>. The <see cref="GetUniqueKey(Topic,
    ///   String?, ExportOptions)"/> method continues to be included primarily for backward compatibility with legacy database
    ///   configurations.
    /// </remarks>
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
      var exportScope           = options.IncludeExternalAssociations? "Root" : options.ExportScope;

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

  } //Class
} //Namespace