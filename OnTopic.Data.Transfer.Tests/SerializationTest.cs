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

      var json = JsonSerializer.Serialize(topicData);

      Assert.AreEqual<string>(expected, json);

    }

    /*==========================================================================================================================
    | TEST: SERIALIZE: KEY/VALUES PAIR: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="KeyValuesPair"/>, serializes it, and confirms the resulting JSON.
    /// </summary>
    [TestMethod]
    public void Serialize_KeyValuesPair_ReturnsExpectedResults() {

      var keyValuesPair         = new KeyValuesPair() {
        Key                     = "Test"
      };
      keyValuesPair.Values.Add("Root:Web");

      var expected = $"{{" +
        $"\"Key\":\"{keyValuesPair.Key}\"," +
        $"\"Values\":[\"Root:Web\"]" +
        $"}}";

      var json = JsonSerializer.Serialize(keyValuesPair);

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

      var recordData            = new RecordData() {
        Key                     = "Test",
        LastModified            = new DateTime(2021, 02, 16, 16, 06, 25)
      };

      var expected = $"{{" +
        $"\"Key\":\"{recordData.Key}\"," +
        $"\"Value\":null," +
        $"\"LastModified\":\"{recordData.LastModified:s}\"" +
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

      var lastModified          = new DateTime(2021, 02, 16, 16, 06, 25);

      var topicData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container"
      };
      var relationshipData      = new KeyValuesPair() {
        Key                     = "Test"
      };
      var referenceData         = new RecordData() {
        Key                     = "Test",
        LastModified            = lastModified
      };
      var attributeData         = new RecordData() {
        Key                     = "Test",
        LastModified            = lastModified
      };
      var childTopicData        = new TopicData() {
        Key                     = "Child",
        UniqueKey               = "Root:Test:Child",
        ContentType             = "Container"
      };

      relationshipData.Values.Add("Root:Web");
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
            $"\"LastModified\":\"{attributeData.LastModified:s}\"" +
          $"}}"+
        $"]," +
        $"\"Relationships\":[" +
          $"{{" +
            $"\"Key\":\"{relationshipData.Key}\"," +
            $"\"Values\":[\"Root:Web\"]" +
          $"}}" +
        $"]," +
        $"\"References\":[" +
          $"{{" +
            $"\"Key\":\"{referenceData.Key}\"," +
            $"\"LastModified\":\"{referenceData.LastModified:s}\"" +
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