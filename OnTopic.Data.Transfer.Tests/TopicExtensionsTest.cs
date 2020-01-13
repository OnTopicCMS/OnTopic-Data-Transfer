/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC EXTENSIONS TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Topic"/> class.
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

      var topicData             = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<int>(1, topicData.Relationships.Count);
      Assert.AreEqual<string>("Root:Related", topicData.Relationships.FirstOrDefault().Relationships.FirstOrDefault());

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
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: SETS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void Import_TopicDataWithAttributes_SetsAttributes() {

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

  } //Class
} //Namespace