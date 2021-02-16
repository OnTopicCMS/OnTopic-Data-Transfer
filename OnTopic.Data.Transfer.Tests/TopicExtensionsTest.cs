/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Data.Transfer.Interchange;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC EXTENSIONS TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicExtensions"/> class using the default <see cref="ImportOptions"/>.
  /// </summary>
  [TestClass]
  public class TopicExtensionsTest {

    /*==========================================================================================================================
    | TEST: EXPORT: BASIC TOPIC: MAPS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with basic attributes and ensures they are reflected on the resulting <see
    ///   cref="TopicData"/> obect.
    /// </summary>
    [TestMethod]
    public void Export_BasicTopic_MapsProperties() {
      var topic = TopicFactory.Create("Test", "Container");

      var topicData = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<string>(topic.Key, topicData.Key);
      Assert.AreEqual<string>(topic.ContentType, topicData.ContentType);
    }

    /*==========================================================================================================================
    | TEST: EXPORT: BASE TOPIC: MAPS REFERENCE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with a <see cref="Topic.BaseTopic"/> and ensures that a <see cref="RecordData"/> item
    ///   with a <see cref="RecordData.Key"/> of <c>BasedTopic</c> is correctly set.
    /// </summary>
    [TestMethod]
    public void Export_BaseTopic_MapsReferenceData() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var baseTopic             = TopicFactory.Create("Base", "Container", topic);
      topic.BaseTopic           = baseTopic;

      var topicData             = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<string>(topic.BaseTopic.GetUniqueKey(), topicData.References.FirstOrDefault()?.Value);

    }

    /*==========================================================================================================================
    | TEST: EXPORT: TOPIC WITH ATTRIBUTES: MAPS ATTRIBUTE DATA COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with <see cref="Topic.Attributes"/> and ensures that the <see
    ///   cref="TopicData.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Export_TopicWithAttributes_MapsAttributeDataCollection() {

      var topic                 = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Attribute", "Attribute Value");

      var topicData             = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<int>(1, topicData.Attributes.Count);
      Assert.AreEqual<string>("Attribute", topicData.Attributes.FirstOrDefault().Key);
      Assert.AreEqual<string>("Attribute Value", topicData.Attributes.FirstOrDefault().Value);

    }

    /*==========================================================================================================================
    | TEST: EXPORT: TOPIC WITH RELATIONSHIPS: MAPS RELATIONSHIP DATA COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with several <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="TopicData.Relationships"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Export_TopicWithRelationships_MapsRelationshipDataCollection() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic          = TopicFactory.Create("Related", "Container", rootTopic);

      topic.Relationships.SetValue("Related", relatedTopic);

      var topicData             = rootTopic.Export(
        new() {
          IncludeChildTopics    = true
        }
      );

      var childTopicData        = topicData.Children.FirstOrDefault()?? new TopicData();

      Assert.IsNotNull(topicData);
      Assert.IsNotNull(childTopicData);
      Assert.AreEqual<int>(1, childTopicData.Relationships.Count);
      Assert.AreEqual<string>("Root:Related", childTopicData.Relationships.FirstOrDefault().Values.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH EXTERNAL ASSOCIATIONS: TOPIC WITH RELATIONSHIPS: INCLUDE EXTERNAL REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with several <see cref="Topic.Relationships"/> and ensures that the <see cref="TopicData
    ///   .Relationships"/> collection <i>does</i> include external associations—i.e., relationships that reference <see cref="
    ///   Topic"/>s outside of the current export scope—when permitted with the <see cref="ExportOptions.
    ///   IncludeExternalAssociations"/> option.
    /// </summary>
    [TestMethod]
    public void ExportWithExternalAssociations_TopicWithRelationships_ExcludesExternalReferences() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic          = TopicFactory.Create("Related", "Container", rootTopic);

      topic.Relationships.SetValue("Related", relatedTopic);

      var topicData             = topic.Export(
        new() {
          IncludeExternalAssociations = true
        }
      );

      Assert.IsNotNull(topicData);
      Assert.AreEqual<int>(1, topicData.Relationships.Count);
      Assert.AreEqual<string>("Root:Related", topicData.Relationships.FirstOrDefault().Values.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: EXPORT: TOPIC WITH REFERENCES: MAPS REFERENCE DATA COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with several <see cref="Topic.References"/> and ensures that the <see cref="TopicData.
    ///   References"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Export_TopicWithReferences_MapsReferenceDataCollection() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var referencedTopic       = TopicFactory.Create("Referenced", "Container", rootTopic);

      topic.References.SetValue("Referenced", referencedTopic);

      var topicData             = rootTopic.Export(
        new() {
          IncludeChildTopics    = true
        }
      );

      var childTopicData        = topicData.Children.FirstOrDefault()?? new TopicData();

      Assert.IsNotNull(topicData);
      Assert.IsNotNull(childTopicData);
      Assert.AreEqual<int>(1, childTopicData.References.Count);
      Assert.AreEqual<string>("Root:Referenced", childTopicData.References.FirstOrDefault().Value);

    }

    /*==========================================================================================================================
    | TEST: EXPORT: TOPIC WITH NESTED TOPICS: EXCLUDES NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with <see cref="Topic.Children"/> and ensures that any nested topics are not included by
    ///   default.
    /// </summary>
    [TestMethod]
    public void Export_TopicWithNestedTopics_ExcludeNestedTopics() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var nestedTopicList       = TopicFactory.Create("NestedTopics", "List", topic);
      _                         = TopicFactory.Create("NestedTopic1", "Page", nestedTopicList);
      _                         = TopicFactory.Create("Child1", "Container", topic);

      var topicData             = topic.Export();

      Assert.AreEqual<int>(0, topicData.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: EXPORT: EXCLUDES RESERVED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with reserved attributes—such as <c>ParentID</c> and <c>TopicID</c>—and ensures that
    ///   they are not exported as attributes.
    /// </summary>
    [TestMethod]
    public void Export_ExcludesReservedAttributes() {

      var topic                 = TopicFactory.Create("Topic", "Container", 5);
      _                         = TopicFactory.Create("ChildA", "Container", topic, 6);
      _                         = TopicFactory.Create("ChildB", "Container", topic, 7);

      //Manually setting using non-standard casing to evaluate case insensitivity
      topic.Attributes.SetValue("parentId", "5");
      topic.Attributes.SetValue("topicId", "6");
      topic.Attributes.SetValue("anotherId", "8");

      var topicData             = topic.Export();

      Assert.AreEqual<int>(1, topicData.Attributes.Count);
      Assert.AreEqual<string>("8", topicData.Attributes.FirstOrDefault().Value);

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH LEGACY TOPIC REFERENCES: OUT OF SCOPE: SKIPS REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with an arbitrary <see cref="AttributeValue"/> that references a <see cref="Topic.Id"/>
    ///   outside of the <see cref="ExportOptions.ExportScope"/>. Confirms that the reference is maintained as an attribute, but
    ///   not added as a reference.
    /// </summary>
    [TestMethod]
    public void ExportWithLegacyTopicReferences_OutOfScope_SkipsReference() {

      var parentTopic           = TopicFactory.Create("Root", "Container", 5);
      var topic                 = TopicFactory.Create("Topic", "Container", parentTopic);

      topic.Attributes.SetValue("SomeId", "5");

      var topicData             = topic.Export();

      topicData.Attributes.TryGetValue("SomeId", out var someAttribute);
      topicData.References.TryGetValue("Some", out var someReference);

      Assert.IsNotNull(someAttribute);
      Assert.IsNull(someReference);

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH LEGACY TOPIC REFERENCES: MISSING TOPIC REFERENCE: SKIPS REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with an arbitrary <see cref="AttributeValue"/> that references a missing <see cref="
    ///   Topic.Id"/>. Confirms that the reference is maintained as an attribute, but not added as a reference.
    /// </summary>
    [TestMethod]
    public void ExportWithLegacyTopicReferences_MissingTopicReference_SkipsReference() {

      var topic                 = TopicFactory.Create("Topic", "Container");

      topic.Attributes.SetValue("InitialBid", "6");

      var topicData             = topic.Export();

      topicData.Attributes.TryGetValue("InitialBid", out var initialBidAttribute);
      topicData.References.TryGetValue("InitialB", out var initialBReference);
      topicData.References.TryGetValue("InitialB", out var initialBidReference);

      Assert.IsNotNull(initialBidAttribute);
      Assert.IsNull(initialBReference);
      Assert.IsNull(initialBidReference);

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH LEGACY TOPIC REFERENCES: INVALID TOPIC REFERENCE: EXPORTS ORIGINAL VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with an arbitrary <see cref="AttributeValue"/> that that contains a non-numeric (i.e.,
    ///   invalid) topic reference. Confirms that the original value is exported.
    /// </summary>
    [TestMethod]
    public void ExportWithLegacyTopicReferences_InvalidTopicReference_ExportsOriginalValue() {

      var topic                 = TopicFactory.Create("Topic", "Container");

      topic.Attributes.SetValue("Rigid", "True");

      var topicData             = topic.Export();

      topicData.Attributes.TryGetValue("Rigid", out var rigidAttribute);

      Assert.AreEqual<string>("True", rigidAttribute.Value);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: BASIC TOPIC DATA: MAPS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with basic properties and ensures they are reflected on the resulting <see
    ///   cref="Topic"/> obect.
    /// </summary>
    [TestMethod]
    public void Import_BasicTopicData_MapsProperties() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topic.Import(topicData);

      Assert.AreEqual<string>(topicData.ContentType, topic.ContentType);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: MISMATCHED CONTENT TYPE: MAINTAINS ORIGINAL VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a different <see cref="TopicData.ContentType"/> than on the target <see
    ///   cref="Topic.ContentType"/>, and assures that it is not overwritten when using the default <see
    ///   cref="ImportStrategy"/>.
    /// </summary>
    [TestMethod]
    public void Import_MismatchedContentType_MaintainsOriginalValue() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Import(topicData);

      Assert.AreNotEqual<string>(topicData.ContentType, topic.ContentType);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: DERIVED TOPIC KEY: MAPS DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with the legacy <see cref="TopicData.DerivedTopicKey"/> and ensures that the <see
    ///   cref="Topic.BaseTopic"/> is set correctly.
    /// </summary>
    [TestMethod]
    #pragma warning disable CS0618 // Type or member is obsolete
    public void Import_DerivedTopicKey_MapsDerivedTopic() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var baseTopic             = TopicFactory.Create("Base", "Container", rootTopic, 5);

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType,
        DerivedTopicKey         = baseTopic.GetUniqueKey()
      };

      topic.Import(topicData);

      Assert.IsNotNull(topic.BaseTopic);
      Assert.AreEqual(baseTopic, topic.BaseTopic);

    }
    #pragma warning restore CS0618 // Type or member is obsolete

    /*==========================================================================================================================
    | TEST: IMPORT: BASE TOPIC: MAPS BASE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a <see cref="RecordData"/> with the <see cref="RecordData.Key"/> of <c>
    ///   BaseTopic</c> in the <see cref="TopicData.References"/> collection, which references a topic in the topic graph.
    ///   Ensures that it is correctly wired up.
    /// </summary>
    [TestMethod]
    public void Import_BaseTopic_MapsBaseTopic() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var baseTopic             = TopicFactory.Create("BaseTopic", "Container", rootTopic);

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      var referencedTopicData   = new RecordData() {
        Key                     = "BaseTopic",
        Value                   = $"{baseTopic.GetUniqueKey()}"
      };

      topicData.References.Add(referencedTopicData);

      topic.Import(topicData);

      Assert.IsNotNull(topic.BaseTopic);
      Assert.AreEqual<Topic>(baseTopic, topic.BaseTopic);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: BASE TOPIC: MAPS NEW BASE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a <see cref="RecordData"/> with the <see cref="RecordData.Key"/> of <c>
    ///   BaseTopic</c> in the <see cref="TopicData.References"/> collection, which references a newly imported topic that
    ///   occurs later in the tree, ensuring that the <see cref="Topic.BaseTopic"/> is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_BaseTopic_MapsNewBaseTopic() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      var childTopicData        = new TopicData() {
        Key                     = "Child",
        UniqueKey               = $"{topicData.UniqueKey}:Child",
        ContentType             = "Container"
      };

      var baseTopicData         = new TopicData() {
        Key                     = "BaseTopic",
        UniqueKey               = $"{topicData.UniqueKey}:BaseTopic",
        ContentType             = "Container"
      };

      var baseTopicReference    = new RecordData() {
        Key                     = "BaseTopic",
        Value                   = $"{topicData.UniqueKey}:BaseTopic"
      };

      topicData.Children.Add(childTopicData);
      topicData.Children.Add(baseTopicData);
      childTopicData.References.Add(baseTopicReference);

      topic.Import(topicData);

      var childTopic            = topic.Children.FirstOrDefault();

      Assert.IsNotNull(childTopic.BaseTopic);
      Assert.AreEqual<string>(baseTopicData.Key, childTopic.BaseTopic?.Key);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: INVALID BASE TOPIC: MAINTAINS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a <see cref="TopicData.BaseTopicKey"/> that is invalid and ensures that the
    ///   <see cref="Topic.BaseTopic"/> is not updated.
    /// </summary>
    [TestMethod]
    public void Import_InvalidBaseTopic_MaintainsExistingValue() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var baseTopic             = TopicFactory.Create("Base", "Container", rootTopic, 5);

      topic.BaseTopic           = baseTopic;

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      var baseTopicReference    = new RecordData() {
        Key                     = "BaseTopic",
        Value                   = $"{topicData.UniqueKey}:BaseTopic"
      };

      topicData.References.Add(baseTopicReference);

      topic.Import(topicData);

      Assert.IsNotNull(topic.BaseTopic);
      Assert.AreEqual(baseTopic, topic.BaseTopic);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: SETS MISSING ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithAttributes_SetsMissingAttributes() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Attributes.Add(
        new() {
          Key                   = "Attribute",
          Value                 = "Attribute Value"
        }
      );

      topic.Import(topicData);

      Assert.AreEqual<string>("Attribute Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: SKIPS NEWER ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithAttributes_SkipsNewerAttributes() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topic.Attributes.SetValue("Attribute", "Original Value");

      topicData.Attributes.Add(
        new() {
          Key                   = "Attribute",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddSeconds(5)
        }
      );

      topic.Import(topicData);

      Assert.AreEqual<string>("Original Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: SKIPS OLDER ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is not updated if the source attribute is older than the target attribute.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithAttributes_SkipsOlderAttribute() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topic.Attributes.SetValue("Attribute", "Original Value");

      topicData.Attributes.Add(
        new() {
          Key                   = "Attribute",
          Value                 = "Older Value",
          LastModified          = DateTime.MinValue
        }
      );

      topic.Import(topicData);

      Assert.AreNotEqual<string>(topicData.Attributes.FirstOrDefault()?.Value, topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH RELATIONSHIPS: MAPS RELATIONSHIP COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithRelationships_MapsRelationshipCollection() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic          = TopicFactory.Create("Related", "Container", rootTopic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var relationshipData      = new KeyValuesPair() {
        Key                   = "Related"
      };

      topicData.Relationships.Add(relationshipData);
      relationshipData.Values.Add(relatedTopic.GetUniqueKey());

      topic.Import(topicData);

      Assert.AreEqual(relatedTopic, topic.Relationships.GetValues("Related")?.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH RELATIONSHIPS: MAINTAINS EXISTING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithRelationships_MaintainsExisting() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic1         = TopicFactory.Create("Related1", "Container", rootTopic);
      var relatedTopic2         = TopicFactory.Create("Related2", "Container", rootTopic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var relationshipData      = new KeyValuesPair() {
        Key                   = "Related"
      };

      topic.Relationships.SetValue("Related", relatedTopic1);

      topicData.Relationships.Add(relationshipData);
      relationshipData.Values.Add(relatedTopic2.GetUniqueKey());

      topic.Import(topicData);

      Assert.AreEqual(relatedTopic1, topic.Relationships.GetValues("Related")?.FirstOrDefault());
      Assert.AreEqual(relatedTopic2, topic.Relationships.GetValues("Related")?.LastOrDefault());

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH REFERENCES: MAPS REFERENCE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="Topic.References"/> and ensures that the <see cref="Topic.References
    ///   "/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithReferences_MapsReferenceCollection() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var referencedTopic       = TopicFactory.Create("Referenced", "Container", rootTopic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var referenceData         = new RecordData() {
        Key                     = "Referenced",
        Value                   = referencedTopic.GetUniqueKey()
      };

      topicData.References.Add(referenceData);

      topic.Import(topicData);

      Assert.AreEqual(referencedTopic, topic.References.GetValue("Referenced"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH REFERENCES: MAINTAINS EXISTING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="Topic.References"/> and ensures that the <see cref="Topic.References
    ///   "/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithReferences_MaintainsExisting() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var referencedTopic1      = TopicFactory.Create("Referenced1", "Container", rootTopic);
      var referencedTopic2      = TopicFactory.Create("Referenced2", "Container", rootTopic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var referenceData         = new RecordData() {
        Key                     = "Referenced",
        Value                   = referencedTopic2.GetUniqueKey()
      };

      topic.References.SetValue("Referenced", referencedTopic1);

      topicData.References.Add(referenceData);

      topic.Import(topicData);

      Assert.AreEqual(referencedTopic1, topic.References.GetValue("Referenced"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH CHILD: MAPS NEW TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Children"/> and ensures that a new <see cref="Topic"/>
    ///   is created under the target <see cref="Topic"/> corresponding to the child <see cref="TopicData"/> object.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithChild_MapsNewTopic() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Children.Add(
        new() {
          Key                     = "Child",
          UniqueKey               = topic.GetUniqueKey() + ":Child",
          ContentType             = topic.ContentType
        }
      );

      topic.Import(topicData);

      Assert.IsNotNull(topic.Children.FirstOrDefault());
      Assert.AreEqual(topicData.Children.FirstOrDefault().UniqueKey, topic.Children.FirstOrDefault().GetUniqueKey());

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH CHILD: MAPS EXISTING TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Children"/> and ensures that the existing <see
    ///   cref="Topic"/> under the target topic <see cref="Topic"/> is updated with the corresponding <see cref="TopicData"/>
    ///   object.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithChild_MapsExistingTopic() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var childTopic            = TopicFactory.Create("Child", "Container", topic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topicData.Children.Add(
        new() {
          Key                     = "Child",
          UniqueKey               = topic.GetUniqueKey() + ":Child",
          ContentType             = topic.ContentType
        }
      );

      topic.Import(topicData);

      Assert.IsNotNull(topic.Children.FirstOrDefault());
      Assert.AreEqual(topicData.Children.FirstOrDefault().ContentType, childTopic.ContentType);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH CHILD: SKIPS ORPHANED CHILD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Children"/>, maps it to a <see cref="Topic"/> with
    ///   existing <see cref="Topic.Children"/>, and ensures that those existing children are maintained.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithChild_SkipsOrphanedChild() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var child1                = TopicFactory.Create("Child1", "Container", topic);
      _                         = TopicFactory.Create("Child2", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Children.Add(
        new() {
          Key                     = "Child2",
          UniqueKey               = topic.GetUniqueKey() + ":Child2",
          ContentType             = topic.ContentType
        }
      );

      topic.Import(topicData);

      Assert.AreEqual<int>(2, topic.Children.Count);
      Assert.AreEqual(child1, topic.Children.FirstOrDefault());
      Assert.AreEqual(topicData.Children.FirstOrDefault().UniqueKey, topic.Children.LastOrDefault().GetUniqueKey());

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH LEGACY TOPIC REFERENCE: MAPS TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with an arbitrary <see cref="RecordData"/> that references another topic. Confirms
    ///   that it is converted to a <see cref="Topic.Id"/> if valid, and otherwise left as is.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithLegacyTopicReference_MapsTopicID() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Topic", "Container", rootTopic);
      var siblingTopic          = TopicFactory.Create("SiblingTopic", "Container", rootTopic, 5);

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Attributes.Add(
        new() {
          Key                   = "SomeId",
          Value                 = siblingTopic.GetUniqueKey()
        }
      );

      topic.Import(topicData);

      Assert.AreEqual<int>(5, topic.References.GetValue("Some")?.Id?? -1);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH MISSING LEGACY TOPIC REFERENCE: SKIPS ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with an arbitrary <see cref="RecordData"/> that references to a missing topic.
    ///   Confirms that the attribute is skipped.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithMissingLegacyTopicReference_SkipsAttribute() {

      var topic                 = TopicFactory.Create("Topic", "Container");

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Attributes.Add(
        new() {
          Key                   = "SomeId",
          Value                 = "Root:Missing:Legacy:Topic:Reference"
        }
      );

      topic.Import(topicData);

      Assert.IsNull(topic.Attributes.GetValue("SomeId", null));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH INVALID LEGACY TOPIC REFERENCE: IMPORTS ORIGINAL VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with an arbitrary <see cref="RecordData"/> that does not reference a topic key
    ///   (i.e., it doesn't start with <c>Root</c>). Confirms that the original attribute value is imported.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithInvalidLegacyTopicReference_ImportsOriginalValue() {

      var topic                 = TopicFactory.Create("Topic", "Container");

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topicData.Attributes.Add(
        new() {
          Key                   = "InitialBid",
          Value                 = "6"
        }
      );

      topic.Import(topicData);

      Assert.AreEqual<string>("6", topic.Attributes.GetValue("InitialBid"));

    }

  } //Class
} //Namespace