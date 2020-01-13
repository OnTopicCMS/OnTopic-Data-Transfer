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

  } //Class
} //Namespace