/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: SERIALIZATION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for evaluating the serialization of <see cref="TopicData"/> and related classes.
  /// </summary>
  [TestClass]
  public class SerializationTest {

    /*==========================================================================================================================
    | TEST: SERIALIZE: TOPIC DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/>, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_TopicData_ReturnsExpectedResults() {

      var topicData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container"
      };

      var expected = $"{{" +
        $"\"Key\":\"{topicData.Key}\"," +
        $"\"UniqueKey\":\"{topicData.UniqueKey}\"," +
        $"\"ContentType\":\"{topicData.ContentType}\"," +
        $"\"Attributes\":[]," +
        $"\"Relationships\":[]," +
        $"\"Children\":[]" +
        $"}}";

      var json = JsonSerializer.Serialize(topicData, new() { IgnoreNullValues = true });

      Assert.AreEqual<string>(expected, json);

    }

    /*==========================================================================================================================
    | TEST: SERIALIZE: RELATIONSHIP DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="RelationshipData"/>, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_RelationshipData_ReturnsExpectedResults() {

      var relationshipData      = new RelationshipData() {
        Key                     = "Test"
      };
      relationshipData.Relationships.Add("Root:Web");

      var expected = $"{{" +
        $"\"Key\":\"{relationshipData.Key}\"," +
        $"\"Relationships\":[\"Root:Web\"]" +
        $"}}";

      var json = JsonSerializer.Serialize(relationshipData);

      Assert.AreEqual<string>(expected, json);

    }

    /*==========================================================================================================================
    | TEST: SERIALIZE: ATTRIBUTE DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="AttributeData"/>, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_AttributeData_ReturnsExpectedResults() {

      var attributeData         = new AttributeData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };

      var expected = $"{{" +
        $"\"Key\":\"{attributeData.Key}\"," +
        $"\"Value\":null," +
        $"\"LastModified\":\"{attributeData.LastModified:o}\"" +
        $"}}";

      var json = JsonSerializer.Serialize(attributeData);

      Assert.AreEqual<string>(expected, json);

    }

    /*==========================================================================================================================
    | TEST: SERIALIZE: TOPIC GRAPH: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with child objects, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_TopicGraph_ReturnsExpectedResults() {

      var topicData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container"
      };
      var relationshipData      = new RelationshipData() {
        Key                     = "Test"
      };
      var attributeData         = new AttributeData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };
      var childTopicData        = new TopicData() {
        Key                     = "Child",
        UniqueKey               = "Root:Test:Child",
        ContentType             = "Container"
      };

      relationshipData.Relationships.Add("Root:Web");
      topicData.Relationships.Add(relationshipData);
      topicData.Attributes.Add(attributeData);
      topicData.Children.Add(childTopicData);

      var expected = $"{{" +
        $"\"Key\":\"{topicData.Key}\"," +
        $"\"UniqueKey\":\"{topicData.UniqueKey}\"," +
        $"\"ContentType\":\"{topicData.ContentType}\"," +
        $"\"Attributes\":[" +
          $"{{" +
            $"\"Key\":\"{attributeData.Key}\"," +
            $"\"LastModified\":\"{attributeData.LastModified:o}\"" +
          $"}}"+
        $"]," +
        $"\"Relationships\":[" +
          $"{{" +
            $"\"Key\":\"{relationshipData.Key}\"," +
            $"\"Relationships\":[\"Root:Web\"]" +
          $"}}" +
        $"]," +
        $"\"Children\":[" +
          $"{{" +
            $"\"Key\":\"{childTopicData.Key}\"," +
            $"\"UniqueKey\":\"{childTopicData.UniqueKey}\"," +
            $"\"ContentType\":\"{childTopicData.ContentType}\"," +
            $"\"Attributes\":[]," +
            $"\"Relationships\":[]," +
            $"\"Children\":[]" +
          $"}}" +
        $"]" +
        $"}}";

      var json = JsonSerializer.Serialize(topicData, new() { IgnoreNullValues = true });

      Assert.AreEqual<string>(expected, json);

    }

  } //Class
} //Namespace