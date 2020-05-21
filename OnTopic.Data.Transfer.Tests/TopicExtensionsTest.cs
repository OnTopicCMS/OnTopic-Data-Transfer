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
    | TEST: EXPORT: DERIVED TOPIC: MAPS DERIVED TOPIC KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with a <see cref="Topic.DerivedTopic"/> and ensures that the <see
    ///   cref="TopicData.DerivedTopicKey"/> is set correctly.
    /// </summary>
    [TestMethod]
    public void Export_DerivedTopic_MapsDerivedTopicKey() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var derivedTopic          = TopicFactory.Create("Derived", "Container", rootTopic);
      topic.DerivedTopic        = derivedTopic;

      var topicData             = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<string>(topic.DerivedTopic.GetUniqueKey(), topicData.DerivedTopicKey);

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

      topic.Relationships.SetTopic("Related", relatedTopic);

      var topicData             = rootTopic.Export(
        new ExportOptions() {
          IncludeChildTopics    = true
        }
      );

      var childTopicData        = topicData.Children.FirstOrDefault()?? new TopicData();

      Assert.IsNotNull(topicData);
      Assert.IsNotNull(childTopicData);
      Assert.AreEqual<int>(1, childTopicData.Relationships.Count);
      Assert.AreEqual<string>("Root:Related", childTopicData.Relationships.FirstOrDefault().Relationships.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH EXTERNAL REFERENCES: TOPIC WITH RELATIONSHIPS: INCLUDE EXTERNAL REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with several <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="TopicData.Relationships"/> collection <i>does</i> include external references—i.e., relationships that point
    ///   to <see cref="Topic"/>s outside of the current export scope—when permitted with the <see
    ///   cref="ExportOptions.IncludeExternalReferences"/> option.
    /// </summary>
    [TestMethod]
    public void ExportWithExternalReferences_TopicWithRelationships_ExcludesExternalReferences() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic          = TopicFactory.Create("Related", "Container", rootTopic);

      topic.Relationships.SetTopic("Related", relatedTopic);

      var topicData             = topic.Export(
        new ExportOptions() {
          IncludeExternalReferences = true
        }
      );

      Assert.IsNotNull(topicData);
      Assert.AreEqual<int>(1, topicData.Relationships.Count);
      Assert.AreEqual<string>("Root:Related", topicData.Relationships.FirstOrDefault().Relationships.FirstOrDefault());

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

      var topic                 = TopicFactory.Create("Topic", "Container");

      //Manually setting using non-standard casing to evaluate case insensitivity
      topic.Attributes.SetValue("parentId", "5");
      topic.Attributes.SetValue("topicId", "6");
      topic.Attributes.SetValue("anotherId", "7");

      var topicData             = topic.Export();

      Assert.AreEqual<int>(1, topicData.Attributes.Count);

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
    ///   Creates a <see cref="TopicData"/> with a <see cref="TopicData.DerivedTopicKey"/> and ensures that the <see
    ///   cref="Topic.DerivedTopic"/> is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_DerivedTopicKey_MapsDerivedTopic() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var derivedTopic          = TopicFactory.Create("Derived", "Container", 5, rootTopic);

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType,
        DerivedTopicKey         = derivedTopic.GetUniqueKey()
      };

      topic.Import(topicData);

      Assert.IsNotNull(topic.DerivedTopic);
      Assert.AreEqual<string>("5", topic.Attributes.GetValue("TopicID"));
      Assert.AreEqual(derivedTopic, topic.DerivedTopic);

    }

    /*==========================================================================================================================
    | TEST: IMPORT: INVALID DERIVED TOPIC KEY: MAINTAINS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a <see cref="TopicData.DerivedTopicKey"/> that is invalid and ensures that the
    ///   <see cref="Topic.DerivedTopic"/> is not updated.
    /// </summary>
    [TestMethod]
    public void Import_InvalidDerivedTopicKey_MaintainsExistingValue() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var derivedTopic          = TopicFactory.Create("Derived", "Container", 5, rootTopic);

      topic.DerivedTopic        = derivedTopic;

      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType,
        DerivedTopicKey         = "Root:InvalidKey"
      };

      topic.Import(topicData);

      Assert.IsNotNull(topic.DerivedTopic);
      Assert.AreEqual<string>("5", topic.Attributes.GetValue("TopicID"));
      Assert.AreEqual(derivedTopic, topic.DerivedTopic);

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
        new AttributeData() {
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
        new AttributeData() {
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
        new AttributeData() {
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
      var relationshipData      = new RelationshipData() {
        Key                   = "Related"
      };

      topicData.Relationships.Add(relationshipData);
      relationshipData.Relationships.Add(relatedTopic.GetUniqueKey());

      topic.Import(topicData);

      Assert.AreEqual(relatedTopic, topic.Relationships.GetTopics("Related")?.FirstOrDefault());

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
      var relationshipData      = new RelationshipData() {
        Key                   = "Related"
      };

      topic.Relationships.SetTopic("Related", relatedTopic1);

      topicData.Relationships.Add(relationshipData);
      relationshipData.Relationships.Add(relatedTopic2.GetUniqueKey());

      topic.Import(topicData);

      Assert.AreEqual(relatedTopic1, topic.Relationships.GetTopics("Related")?.FirstOrDefault());
      Assert.AreEqual(relatedTopic2, topic.Relationships.GetTopics("Related")?.LastOrDefault());

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
        new TopicData() {
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
        new TopicData() {
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
        new TopicData() {
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

  } //Class
} //Namespace