/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
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
        $"\"References\":[]," +
        $"\"Children\":[]" +
        $"}}";

      #if NET5_0
      var json = JsonSerializer.Serialize(topicData);
      #else
      var json = JsonSerializer.Serialize(topicData, new() { IgnoreNullValues = true });
      #endif

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
    | TEST: SERIALIZE: RECORD DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="RecordData"/>, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_RecordData_ReturnsExpectedResults() {

      var recordData         = new RecordData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };

      var expected = $"{{" +
        $"\"Key\":\"{recordData.Key}\"," +
        $"\"Value\":null," +
        $"\"LastModified\":\"{recordData.LastModified:o}\"" +
        $"}}";

      var json = JsonSerializer.Serialize(recordData);

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
      var referenceData         = new RecordData() {
        Key                     = "Test",
        LastModified            = DateTime.Now
      };
      var attributeData         = new RecordData() {
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
      topicData.References.Add(referenceData);
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
        $"\"References\":[" +
          $"{{" +
            $"\"Key\":\"{referenceData.Key}\"," +
            $"\"LastModified\":\"{referenceData.LastModified:o}\"" +
          $"}}"+
        $"]," +
        $"\"Children\":[" +
          $"{{" +
            $"\"Key\":\"{childTopicData.Key}\"," +
            $"\"UniqueKey\":\"{childTopicData.UniqueKey}\"," +
            $"\"ContentType\":\"{childTopicData.ContentType}\"," +
            $"\"Attributes\":[]," +
            $"\"Relationships\":[]," +
            $"\"References\":[]," +
            $"\"Children\":[]" +
          $"}}" +
        $"]" +
        $"}}";

      var json = JsonSerializer.Serialize(topicData, new() { IgnoreNullValues = true });

      Assert.AreEqual<string>(expected, json);

    }

  } //Class
} //Namespace