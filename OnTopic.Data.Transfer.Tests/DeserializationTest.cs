/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: DESERIALIZATION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for evaluating the deserialization to <see cref="TopicData"/> and related classes.
  /// </summary>
  [TestClass]
  public class DeserializationTest {

    /*==========================================================================================================================
    | TEST: DESERIALIZE: TOPIC DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="TopicData"/> class.
    /// </summary>
    [TestMethod]
    public void Deserialize_TopicData_ReturnsExpectedResults() {

      var sourceData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container",
        BaseTopicKey            = "Root:Meta:Test"
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"UniqueKey\":\"{sourceData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceData.ContentType}\"," +
        $"\"BaseTopicKey\":\"{sourceData.BaseTopicKey}\"," +
        $"\"Attributes\":[]," +
        $"\"Relationships\":[]," +
        $"\"Children\":[]" +
        $"}}";

      var topicData = JsonSerializer.Deserialize<TopicData>(json);

      Assert.AreEqual<string>(sourceData.Key, topicData.Key);
      Assert.AreEqual<string>(sourceData.UniqueKey, topicData.UniqueKey);
      Assert.AreEqual<string>(sourceData.ContentType, topicData.ContentType);
      Assert.AreEqual<string>(sourceData.BaseTopicKey, topicData.BaseTopicKey);
      Assert.AreEqual<int>(0, topicData.Relationships.Count);
      Assert.AreEqual<int>(0, topicData.Attributes.Count);
      Assert.AreEqual<int>(0, topicData.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: DERIVED TOPIC KEY: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string with a legacy <c>DerivedTopicKey</c> and attempts to deserialize it as a <see cref="TopicData"/>
    ///   class.
    /// </summary>
    [TestMethod]
    public void Deserialize_DeriedTopicKey_ReturnsExpectedResults() {

      var sourceData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container",
        BaseTopicKey            = "Root:Meta:Test"
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"UniqueKey\":\"{sourceData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceData.ContentType}\"," +
        $"\"DerivedTopicKey\":\"{sourceData.BaseTopicKey}\"," +
        $"\"Attributes\":[]," +
        $"\"Relationships\":[]," +
        $"\"Children\":[]" +
        $"}}";

      var topicData = JsonSerializer.Deserialize<TopicData>(json);

      Assert.AreEqual<string>(sourceData.Key, topicData.Key);
      Assert.AreEqual<string>(sourceData.UniqueKey, topicData.UniqueKey);
      Assert.AreEqual<string>(sourceData.ContentType, topicData.ContentType);
      Assert.AreEqual<string>(sourceData.BaseTopicKey, topicData.BaseTopicKey);
      Assert.AreEqual<int>(0, topicData.Relationships.Count);
      Assert.AreEqual<int>(0, topicData.Attributes.Count);
      Assert.AreEqual<int>(0, topicData.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: RELATIONSHIP DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="RelationshipData"/> class.
    /// </summary>
    [TestMethod]
    public void Deserialize_RelationshipData_ReturnsExpectedResults() {

      var sourceData            = new RelationshipData() {
        Key                     = "Test"
      };
      sourceData.Relationships.Add("Root:Web");

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"Relationships\":[\"Root:Web\"]" +
        $"}}";

      var relationshipData = JsonSerializer.Deserialize<RelationshipData>(json);

      Assert.AreEqual<string>(sourceData.Key, relationshipData.Key);
      Assert.AreEqual<int>(sourceData.Relationships.Count, relationshipData.Relationships.Count);
      Assert.AreEqual<string>(sourceData.Relationships.FirstOrDefault(), relationshipData.Relationships.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: ATTRIBUTE DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="AttributeData"/> class.
    /// </summary>
    [TestMethod]
    public void Deserialize_AttributeData_ReturnsExpectedResults() {

      var sourceData            = new AttributeData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"Value\":null," +
        $"\"LastModified\":\"{sourceData.LastModified:o}\"" +
        $"}}";


      var attributeData = JsonSerializer.Deserialize<AttributeData>(json);

      Assert.AreEqual<string>(sourceData.Key, attributeData.Key);
      Assert.AreEqual<string>(sourceData.Value, attributeData.Value);
      Assert.AreEqual<DateTime>(sourceData.LastModified, attributeData.LastModified);

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: TOPIC GRAPH: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="TopicData"/> class with child objects.
    /// </summary>
    [TestMethod]
    public void Deserialize_TopicGraph_ReturnsExpectedResults() {

      var sourceTopicData       = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container"
      };
      var sourceRelationshipData= new RelationshipData() {
        Key                     = "Test"
      };
      var sourceAttributeData   = new AttributeData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };
      var sourceChildTopicData  = new TopicData() {
        Key                     = "Child",
        UniqueKey               = "Root:Test:Child",
        ContentType             = "Container"
      };

      sourceRelationshipData.Relationships.Add("Root:Web");
      sourceTopicData.Relationships.Add(sourceRelationshipData);
      sourceTopicData.Attributes.Add(sourceAttributeData);
      sourceTopicData.Children.Add(sourceChildTopicData);

      var json = $"{{" +
        $"\"Key\":\"{sourceTopicData.Key}\"," +
        $"\"UniqueKey\":\"{sourceTopicData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceTopicData.ContentType}\"," +
        $"\"BaseTopicKey\":null," +
        $"\"Attributes\":[" +
          $"{{" +
            $"\"Key\":\"{sourceAttributeData.Key}\"," +
            $"\"Value\":null," +
            $"\"LastModified\":\"{sourceAttributeData.LastModified:o}\"" +
          $"}}"+
        $"]," +
        $"\"Relationships\":[" +
          $"{{" +
            $"\"Key\":\"{sourceRelationshipData.Key}\"," +
            $"\"Relationships\":[\"Root:Web\"]" +
          $"}}" +
        $"]," +
        $"\"Children\":[" +
          $"{{" +
            $"\"Key\":\"{sourceChildTopicData.Key}\"," +
            $"\"UniqueKey\":\"{sourceChildTopicData.UniqueKey}\"," +
            $"\"ContentType\":\"{sourceChildTopicData.ContentType}\"," +
            $"\"BaseTopicKey\":null," +
            $"\"Attributes\":[]," +
            $"\"Relationships\":[]," +
            $"\"Children\":[]" +
          $"}}" +
        $"]" +
        $"}}";


      var topicData = JsonSerializer.Deserialize<TopicData>(json);
      var relationshipData = topicData.Relationships.FirstOrDefault();
      var attributeData = topicData.Attributes.FirstOrDefault();
      var childTopicData = topicData.Children.FirstOrDefault();

      Assert.AreEqual<string>(sourceTopicData.Key, topicData.Key);
      Assert.AreEqual<string>(sourceTopicData.UniqueKey, topicData.UniqueKey);
      Assert.AreEqual<string>(sourceTopicData.ContentType, topicData.ContentType);
      Assert.AreEqual<string>(sourceTopicData.BaseTopicKey, topicData.BaseTopicKey);
      Assert.AreEqual<int>(1, sourceTopicData.Relationships.Count);
      Assert.AreEqual<int>(1, sourceTopicData.Attributes.Count);
      Assert.AreEqual<int>(1, sourceTopicData.Children.Count);

      Assert.AreEqual<string>(sourceRelationshipData.Key, relationshipData.Key);
      Assert.AreEqual<int?>(sourceRelationshipData.Relationships.Count, relationshipData.Relationships.Count);
      Assert.AreEqual<string>(sourceRelationshipData.Relationships.FirstOrDefault(), relationshipData.Relationships.FirstOrDefault());

      Assert.AreEqual<string>(sourceAttributeData.Key, attributeData.Key);
      Assert.AreEqual<string>(sourceAttributeData.Value, attributeData.Value);
      Assert.AreEqual<DateTime>(sourceAttributeData.LastModified, attributeData.LastModified);

      Assert.AreEqual<string>(sourceChildTopicData.Key, childTopicData.Key);
      Assert.AreEqual<string>(sourceChildTopicData.UniqueKey, childTopicData.UniqueKey);
      Assert.AreEqual<string>(sourceChildTopicData.ContentType, childTopicData.ContentType);
      Assert.AreEqual<string>(sourceChildTopicData.BaseTopicKey, childTopicData.BaseTopicKey);
      Assert.AreEqual<int>(0, sourceChildTopicData.Relationships.Count);
      Assert.AreEqual<int>(0, sourceChildTopicData.Children.Count);
    }

  } //Class
} //Namespace