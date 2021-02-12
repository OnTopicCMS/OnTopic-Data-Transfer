/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Data.Transfer.Interchange;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: EXPORT OPTIONS TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicExtensions"/> class using customizations of the <see
  ///   cref="ExportOptions"/>.
  /// </summary>
  [TestClass]
  public class ExportOptionsTest {

    /*==========================================================================================================================
    | TEST: EXPORT WITH CHILDREN: TOPIC WITH CHILD: INCLUDES CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with <see cref="Topic.Children"/> and ensures that the resulting <see cref="TopicData"/>
    ///   includes <see cref="TopicData.Children"/> when permitted.
    /// </summary>
    [TestMethod]
    public void ExportWithChildren_TopicWithChild_IncludesChildren() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var childTopic            = TopicFactory.Create("Child1", "Container", topic);

      var topicData             = topic.Export(
        new() {
          IncludeChildTopics    = true
        }
      );

      Assert.AreEqual<int>(1, topicData.Children.Count);
      Assert.AreEqual(childTopic.GetUniqueKey(), topicData.Children.FirstOrDefault().UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH NESTED TOPICS: TOPIC WITH NESTED TOPICS: INCLUDES NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with nested topics and ensures that the resulting <see cref="TopicData"/>  includes them
    ///   as <see cref="TopicData.Children"/> when permitted.
    /// </summary>
    [TestMethod]
    public void ExportWithNestedTopic_TopicWithNestedTopics_IncludesNestedTopics() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var nestedTopicList       = TopicFactory.Create("NestedTopics", "List", topic);
      var nestedTopic           = TopicFactory.Create("NestedTopic1", "Page", nestedTopicList);
      _                         = TopicFactory.Create("Child1", "Container", topic);

      var topicData             = topic.Export(
        new() {
          IncludeNestedTopics   = true
        }
      );

      Assert.AreEqual<int>(1, topicData.Children.Count);
      Assert.AreEqual(nestedTopicList.GetUniqueKey(), topicData.Children.FirstOrDefault().UniqueKey);
      Assert.AreEqual<int?>(1, topicData.Children.FirstOrDefault()?.Children.Count);
      Assert.AreEqual(nestedTopic.GetUniqueKey(), topicData.Children.FirstOrDefault()?.Children.FirstOrDefault()?.UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: EXPORT: TOPIC WITH RELATIONSHIPS: EXCLUDES EXTERNAL REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with several <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="TopicData.Relationships"/> collection does <i>not</i> include external references—i.e., relationships that refer
    ///   to <see cref="Topic"/>s outside of the current export scope.
    /// </summary>
    [TestMethod]
    public void Export_TopicWithRelationships_ExcludesExternalReferences() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic          = TopicFactory.Create("Related", "Container", rootTopic);

      topic.Relationships.SetValue("Related", relatedTopic);

      var topicData             = topic.Export();

      Assert.IsNotNull(topicData);
      Assert.AreEqual<int>(0, topicData.Relationships.Count);

    }

    /*==========================================================================================================================
    | TEST: EXPORT WITH LEGACY TOPIC REFERENCES: EXTERNAL TOPIC REFERENCE: EXPORTS UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> with an arbitrary <see cref="AttributeValue"/> that references another topic. Confirms
    ///   that it is converted to a <see cref="Topic.GetUniqueKey"/> if valid, and is otherwise left as is.
    /// </summary>
    [TestMethod]
    public void ExportWithLegacyTopicReferences_ExternalTopicReference_ExportsUniqueKey() {

      var parentTopic           = TopicFactory.Create("Root", "Container", 5);
      var topic                 = TopicFactory.Create("Topic", "Container", parentTopic);

      topic.Attributes.SetValue("SomeId", "5");

      var topicData = topic.Export(
        new() {
          IncludeExternalAssociations                           = true,
          TranslateLegacyTopicReferences                        = true
        }
      );

      topicData.Attributes.TryGetValue("SomeId", out var someAttribute);

      Assert.AreEqual<string>("Root", someAttribute.Value);

    }

  } //Class
} //Namespace