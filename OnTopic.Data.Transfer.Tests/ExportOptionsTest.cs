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
        new ExportOptions() {
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
        new ExportOptions() {
          IncludeNestedTopics   = true
        }
      );

      Assert.AreEqual<int>(1, topicData.Children.Count);
      Assert.AreEqual(nestedTopicList.GetUniqueKey(), topicData.Children.FirstOrDefault().UniqueKey);
      Assert.AreEqual<int?>(1, topicData.Children.FirstOrDefault()?.Children.Count);
      Assert.AreEqual(nestedTopic.GetUniqueKey(), topicData.Children.FirstOrDefault()?.Children.FirstOrDefault()?.UniqueKey);

    }

  } //Class
} //Namespace