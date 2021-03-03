/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Data.Transfer.Converters;

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
        ContentType             = "Container"
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"UniqueKey\":\"{sourceData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceData.ContentType}\"," +
        $"\"Attributes\":[]," +
        $"\"Relationships\":[]," +
        $"\"Children\":[]" +
        $"}}";

      var topicData = JsonSerializer.Deserialize<TopicData>(json);

      Assert.AreEqual<string?>(sourceData.Key, topicData?.Key);
      Assert.AreEqual<string?>(sourceData.UniqueKey, topicData?.UniqueKey);
      Assert.AreEqual<string?>(sourceData.ContentType, topicData?.ContentType);
      Assert.AreEqual<int?>(0, topicData?.Relationships.Count);
      Assert.AreEqual<int?>(0, topicData?.Attributes.Count);
      Assert.AreEqual<int?>(0, topicData?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: DERIVED TOPIC KEY: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string with a legacy <c>DerivedTopicKey</c> and attempts to deserialize it as a <see cref="TopicData"/>
    ///   class.
    /// </summary>
    [TestMethod]
    #pragma warning disable CS0618 // Type or member is obsolete
    public void Deserialize_DerivedTopicKey_ReturnsExpectedResults() {

      var sourceData             = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container",
        DerivedTopicKey         = "Root:Meta:Test"
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"UniqueKey\":\"{sourceData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceData.ContentType}\"," +
        $"\"DerivedTopicKey\":\"{sourceData.DerivedTopicKey}\"," +
        $"\"Attributes\":[]," +
        $"\"Relationships\":[]," +
        $"\"Children\":[]" +
        $"}}";

      var topicData = JsonSerializer.Deserialize<TopicData>(json);

      Assert.AreEqual<string?>(sourceData.Key, topicData?.Key);
      Assert.AreEqual<string?>(sourceData.UniqueKey, topicData?.UniqueKey);
      Assert.AreEqual<string?>(sourceData.ContentType, topicData?.ContentType);
      Assert.AreEqual<string?>(sourceData.DerivedTopicKey, topicData?.DerivedTopicKey);
      Assert.AreEqual<int?>(0, topicData?.Relationships.Count);
      Assert.AreEqual<int?>(0, topicData?.Attributes.Count);
      Assert.AreEqual<int?>(0, topicData?.Children.Count);

    }
    #pragma warning restore CS0618 // Type or member is obsolete

    /*==========================================================================================================================
    | TEST: DESERIALIZE: KEY/VALUES PAIR: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a JSON string and attempts to deserialize it as a <see cref="KeyValuesPair"/> class.
    /// </summary>
    [TestMethod]
    public void Deserialize_KeyValuesPair_ReturnsExpectedResults() {

      var sourceData            = new KeyValuesPair() {
        Key                     = "Test"
      };
      sourceData.Values.Add("Root:Web");

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"Values\":[\"Root:Web\"]" +
        $"}}";

      var keyValuesPair         = JsonSerializer.Deserialize<KeyValuesPair>(json);

      Assert.AreEqual<string?>(sourceData.Key, keyValuesPair?.Key);
      Assert.AreEqual<int?>(sourceData.Values.Count, keyValuesPair?.Values.Count);
      Assert.AreEqual<string?>(sourceData.Values.FirstOrDefault(), keyValuesPair?.Values.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: RELATIONSHIP DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a JSON string representing the legacy <c>RelationshipData</c> class (which used a <c>Relationships</c> array),
    ///   and attempts to deserialize it as a <see cref="KeyValuesPair"/> class, ensuring that the <see cref="
    ///   KeyValuesPairConverter"/> properly translates the <c>Relationships</c> array to the <see cref="KeyValuesPair.Values"/>
    ///   collection.
    /// </summary>
    [TestMethod]
    public void Deserialize_RelationshipData_ReturnsExpectedResults() {

      var sourceData            = new KeyValuesPair() {
        Key                     = "Test"
      };
      sourceData.Values.Add("Root:Web");

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"Relationships\":[\"Root:Web\"]" +
        $"}}";

      var keyValuesPair         = JsonSerializer.Deserialize<KeyValuesPair>(json);

      Assert.AreEqual<string?>(sourceData.Key, keyValuesPair?.Key);
      Assert.AreEqual<int?>(sourceData.Values.Count, keyValuesPair?.Values.Count);
      Assert.AreEqual<string?>(sourceData.Values.FirstOrDefault(), keyValuesPair?.Values.FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: RECORD DATA: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="RecordData"/> class.
    /// </summary>
    [TestMethod]
    public void Deserialize_RecordData_ReturnsExpectedResults() {

      var sourceData            = new RecordData() {
        Key                     = "Test",
        LastModified            = new DateTime(2021, 02, 16, 16, 06, 25)
      };

      var json = $"{{" +
        $"\"Key\":\"{sourceData.Key}\"," +
        $"\"Value\":null," +
        $"\"LastModified\":\"{sourceData.LastModified:s}\"" +
        $"}}";


      var recordData = JsonSerializer.Deserialize<RecordData>(json);

      Assert.AreEqual<string?>(sourceData.Key, recordData?.Key);
      Assert.AreEqual<string?>(sourceData.Value, recordData?.Value);
      Assert.AreEqual<DateTime?>(sourceData.LastModified, recordData?.LastModified);

    }

    /*==========================================================================================================================
    | TEST: DESERIALIZE: TOPIC GRAPH: RETURNS EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a json string and attempts to deserialize it as a <see cref="TopicData"/> class with child objects.
    /// </summary>
    [TestMethod]
    public void Deserialize_TopicGraph_ReturnsExpectedResults() {

      var lastModified          = new DateTime(2021, 02, 16, 16, 06, 25);

      var sourceTopicData       = new TopicData() {
        Key                     = "Test",
        UniqueKey               = "Root:Test",
        ContentType             = "Container"
      };
      var sourceRelationshipData= new KeyValuesPair() {
        Key                     = "Test"
      };
      var sourceAttributeData   = new RecordData() {
        Key                     = "Test",
        LastModified            = lastModified
      };
      var sourceReferenceData   = new RecordData() {
        Key                     = "Test",
        Value                   = "Root:Reference",
        LastModified            = lastModified
      };
      var sourceChildTopicData  = new TopicData() {
        Key                     = "Child",
        UniqueKey               = "Root:Test:Child",
        ContentType             = "Container"
      };

      sourceRelationshipData.Values.Add("Root:Web");
      sourceTopicData.Relationships.Add(sourceRelationshipData);
      sourceTopicData.Attributes.Add(sourceAttributeData);
      sourceTopicData.Children.Add(sourceChildTopicData);

      var json = $"{{" +
        $"\"Key\":\"{sourceTopicData.Key}\"," +
        $"\"UniqueKey\":\"{sourceTopicData.UniqueKey}\"," +
        $"\"ContentType\":\"{sourceTopicData.ContentType}\"," +
        $"\"Attributes\":[" +
          $"{{" +
            $"\"Key\":\"{sourceAttributeData.Key}\"," +
            $"\"Value\":null," +
            $"\"LastModified\":\"{sourceAttributeData.LastModified:s}\"" +
          $"}}"+
        $"]," +
        $"\"Relationships\":[" +
          $"{{" +
            $"\"Key\":\"{sourceRelationshipData.Key}\"," +
            $"\"Values\":[\"Root:Web\"]" +
          $"}}" +
        $"]," +
        $"\"References\":[" +
          $"{{" +
            $"\"Key\":\"{sourceReferenceData.Key}\"," +
            $"\"Value\":\"{sourceReferenceData.Value}\"," +
            $"\"LastModified\":\"{sourceReferenceData.LastModified:s}\"" +
          $"}}"+
        $"]," +
        $"\"Children\":[" +
          $"{{" +
            $"\"Key\":\"{sourceChildTopicData.Key}\"," +
            $"\"UniqueKey\":\"{sourceChildTopicData.UniqueKey}\"," +
            $"\"ContentType\":\"{sourceChildTopicData.ContentType}\"," +
            $"\"Attributes\":[]," +
            $"\"Relationships\":[]," +
            $"\"Children\":[]" +
          $"}}" +
        $"]" +
        $"}}";


      var topicData             = JsonSerializer.Deserialize<TopicData>(json);
      var relationshipData      = topicData?.Relationships.FirstOrDefault();
      var referenceData         = topicData?.References.FirstOrDefault();
      var attributeData         = topicData?.Attributes.FirstOrDefault();
      var childTopicData        = topicData?.Children.FirstOrDefault();

      Assert.AreEqual<string?>(sourceTopicData.Key, topicData?.Key);
      Assert.AreEqual<string?>(sourceTopicData.UniqueKey, topicData?.UniqueKey);
      Assert.AreEqual<string?>(sourceTopicData.ContentType, topicData?.ContentType);
      Assert.AreEqual<int>(1, sourceTopicData.Relationships.Count);
      Assert.AreEqual<int>(1, sourceTopicData.Attributes.Count);
      Assert.AreEqual<int>(1, sourceTopicData.Children.Count);

      Assert.AreEqual<string?>(sourceRelationshipData.Key, relationshipData?.Key);
      Assert.AreEqual<int?>(sourceRelationshipData.Values.Count, relationshipData?.Values.Count);
      Assert.AreEqual<string?>(sourceRelationshipData.Values.FirstOrDefault(), relationshipData?.Values.FirstOrDefault());

      Assert.AreEqual<string?>(sourceReferenceData.Key, referenceData?.Key);
      Assert.AreEqual<string?>(sourceReferenceData.Value, referenceData?.Value);
      Assert.AreEqual<DateTime?>(sourceReferenceData.LastModified, referenceData?.LastModified);

      Assert.AreEqual<string?>(sourceAttributeData.Key, attributeData?.Key);
      Assert.AreEqual<string?>(sourceAttributeData.Value, attributeData?.Value);
      Assert.AreEqual<DateTime?>(sourceAttributeData.LastModified, attributeData?.LastModified);

      Assert.AreEqual<string?>(sourceChildTopicData.Key, childTopicData?.Key);
      Assert.AreEqual<string?>(sourceChildTopicData.UniqueKey, childTopicData?.UniqueKey);
      Assert.AreEqual<string?>(sourceChildTopicData.ContentType, childTopicData?.ContentType);
      Assert.AreEqual<int>(0, sourceChildTopicData.Relationships.Count);
      Assert.AreEqual<int>(0, sourceChildTopicData.Children.Count);
    }

  } //Class
} //Namespace