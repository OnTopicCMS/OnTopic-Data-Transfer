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
  ///   Provides unit tests for the <see cref="TopicExtensions"/> class using customizations of the <see
  ///   cref="TopicImportOptions"/>.
  /// </summary>
  [TestClass]
  public class TopicImportOptionsTest {

    /*==========================================================================================================================
    | TEST: IMPORT: MISMATCHED CONTENT TYPE: OVERWRITES WHEN PERMITTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a different <see cref="TopicData.ContentType"/> than on the target <see
    ///   cref="Topic.ContentType"/>, and assures that it is overwritten when permitted.
    /// </summary>
    [TestMethod]
    public void Import_MismatchedContentType_OverwritesWhenPermitted() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Import(
        topicData,
        new TopicImportOptions() {
          OverwriteContentType  = true
        }
      );

      Assert.AreEqual<string>(topicData.ContentType, topic.ContentType);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS MERGE: TOPIC DATA WITH ATTRIBUTES: UPDATE OLDER VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void ImportAsMerge_TopicDataWithAttributes_UpdatesOlderValues() {

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
          LastModified          = DateTime.Now.AddTicks(1)
        }
      );

      topic.Import(
        topicData,
        new TopicImportOptions() {
          Strategy              = ImportStrategy.Merge
        }
      );

      Assert.AreEqual<string>("New Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: SKIPS NEWER VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void ImportAsMerge_TopicDataWithAttributes_SkipsNewerValues() {

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
          LastModified          = DateTime.Now.AddSeconds(-5)
        }
      );

      topic.Import(
        topicData,
        new TopicImportOptions() {
          Strategy              = ImportStrategy.Merge
        }
      );

      Assert.AreEqual<string>("Original Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH ATTRIBUTES: REPLACES NEWER VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Attributes"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void ImportAsOverwrite_TopicDataWithAttributes_ReplacesNewerValue() {

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
          LastModified          = DateTime.Now.AddSeconds(-5)
        }
      );

      topic.Import(
        topicData,
        new TopicImportOptions() {
          Strategy              = ImportStrategy.Overwrite
        }
      );

      Assert.AreEqual<string>("New Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS REPLACE: TOPIC DATA WITH RELATIONSHIPS: DELETES ORPHANED RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="Topic.Relationships"/> and ensures that the <see
    ///   cref="Topic.Attributes"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void ImportAsReplace_TopicDataWithRelationships_DeletesOrphanedRelationships() {

      var rootTopic             = TopicFactory.Create("Root", "Container");
      var topic                 = TopicFactory.Create("Test", "Container", rootTopic);
      var relatedTopic1         = TopicFactory.Create("Related1", "Container", rootTopic);
      var relatedTopic2         = TopicFactory.Create("Related2", "Container", rootTopic);
      var relatedTopic3         = TopicFactory.Create("Related3", "Container", rootTopic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var relationshipData      = new RelationshipData() {
        Key                   = "Related"
      };

      topic.Relationships.SetTopic("Related", relatedTopic1);
      topic.Relationships.SetTopic("Related", relatedTopic2);
      topic.Relationships.SetTopic("Cousin",  relatedTopic3);

      topicData.Relationships.Add(relationshipData);
      relationshipData.Relationships.Add(relatedTopic1.GetUniqueKey());

      topic.Import(
        topicData,
        new TopicImportOptions() {
          Strategy              = ImportStrategy.Replace
        }
      );

      Assert.AreEqual(relatedTopic1, topic.Relationships.GetTopics("Related")?.FirstOrDefault());
      Assert.AreEqual<int>(1, topic.Relationships.GetTopics("Related").Count);
      Assert.AreEqual<int>(0, topic.Relationships.GetTopics("Cousin").Count);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS REPLACE: TOPIC DATA WITH CHILD: DELETES ORPHANED CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.Children"/> and ensures that a new <see cref="Topic"/>
    ///   is created under the target <see cref="Topic"/> corresponding to the child <see cref="TopicData"/> object.
    /// </summary>
    [TestMethod]
    public void ImportAsReplace_TopicDataWithChild_DeletedOrphanedChildren() {

      var topic                 = TopicFactory.Create("Test", "Container");
      _                         = TopicFactory.Create("Child1", "Container", topic);
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

      topic.Import(
        topicData,
        new TopicImportOptions() {
          Strategy              = ImportStrategy.Replace
        }
      );

      Assert.AreEqual<int>(1, topic.Children.Count);
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