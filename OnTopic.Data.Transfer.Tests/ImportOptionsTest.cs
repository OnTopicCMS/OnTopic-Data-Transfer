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
  ///   cref="ImportOptions"/>.
  /// </summary>
  [TestClass]
  public class ImportOptionsTest {

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
        new() {
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
        new() {
          Key                   = "Attribute",
          Value                 = "New Value",
          LastModified          = DateTime.UtcNow.AddTicks(300)
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Merge
        }
      );

      Assert.AreEqual<string?>("New Value", topic.Attributes.GetValue("Attribute"));

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
        new() {
          Key                   = "Attribute",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddSeconds(-5)
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Merge
        }
      );

      Assert.AreEqual<string?>("Original Value", topic.Attributes.GetValue("Attribute"));

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
        new() {
          Key                   = "Attribute",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddSeconds(-5)
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Overwrite
        }
      );

      Assert.AreEqual<string?>("New Value", topic.Attributes.GetValue("Attribute"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT: TOPIC DATA WITH REFERENCES: SKIPS NEWER VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with <see cref="TopicData.References"/> and ensures that the <see cref="Topic.
    ///   References"/> collection is set correctly.
    /// </summary>
    [TestMethod]
    public void ImportAsMerge_TopicDataWithReferences_SkipsNewerValues() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var referencedTopic1      = TopicFactory.Create("Referenced1", "Container", topic);
      var referencedTopic2      = TopicFactory.Create("Referenced2", "Container", topic);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };

      topic.References.SetValue("Reference", referencedTopic1);
      topic.References.SetValue("OldReference", referencedTopic2, null, DateTime.UtcNow.AddDays(-1));

      topicData.References.Add(
        new() {
          Key                   = "Reference",
          Value                 = referencedTopic2.GetUniqueKey(),
          LastModified          = DateTime.UtcNow.AddDays(-1)
        }
      );

      topicData.References.Add(
        new() {
          Key                   = "OldReference",
          Value                 = referencedTopic1.GetUniqueKey(),
          LastModified          = DateTime.UtcNow
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Merge
        }
      );

      Assert.AreEqual<Topic?>(referencedTopic1, topic.References.GetValue("Reference"));
      Assert.AreEqual<Topic?>(referencedTopic1, topic.References.GetValue("OldReference"));

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
      var relationshipData      = new KeyValuesPair() {
        Key                   = "Related"
      };

      topic.Relationships.SetValue("Related", relatedTopic1);
      topic.Relationships.SetValue("Related", relatedTopic2);
      topic.Relationships.SetValue("Cousin",  relatedTopic3);

      topicData.Relationships.Add(relationshipData);
      relationshipData.Values.Add(relatedTopic1.GetUniqueKey());

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Replace
        }
      );

      Assert.AreEqual(relatedTopic1, topic.Relationships.GetValues("Related")?.FirstOrDefault());
      Assert.AreEqual<int>(1, topic.Relationships.GetValues("Related").Count);
      Assert.AreEqual<int>(0, topic.Relationships.GetValues("Cousin").Count);

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
        new() {
          Key                     = "Child2",
          UniqueKey               = topic.GetUniqueKey() + ":Child2",
          ContentType             = topic.ContentType
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Replace
        }
      );

      Assert.AreEqual<int>(1, topic.Children.Count);
      Assert.AreEqual(topicData.Children.FirstOrDefault().UniqueKey, topic.Children.FirstOrDefault().GetUniqueKey());
      Assert.AreEqual(topicData.Children.FirstOrDefault().ContentType, topic.Children.FirstOrDefault().ContentType);

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
      var childTopicData        = new TopicData() {
        Key                     = "Child",
        UniqueKey               = topic.GetUniqueKey() + ":Child",
        ContentType             = topic.ContentType
      };

      childTopic.Attributes.SetValue("Description", "Old Value");

      childTopicData.Attributes.Add(
        new() {
          Key                   = "Description",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

      topicData.Children.Add(childTopicData);

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Merge
        }
      );

      var mappedChild           = topic.Children.FirstOrDefault();

      Assert.IsNotNull(mappedChild);
      Assert.AreEqual(mappedChild.ContentType, childTopic.ContentType);
      Assert.AreEqual("New Value", mappedChild.Attributes.GetValue("Description"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS REPLACE: TOPIC DATA WITH NESTED TOPIC: DELETES ORPHANED CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with nested topics, and ensures that a new <see cref="Topic"/> is created under the
    ///   target <see cref="Topic"/> corresponding to the child <see cref="TopicData"/> object.
    /// </summary>
    [TestMethod]
    public void ImportAsReplace_TopicDataWithNestedTopic_DeletedOrphanedChildren() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var nestedTopics          = TopicFactory.Create("Nested", "List", topic);
      _                         = TopicFactory.Create("Nested1", "Page", nestedTopics, 1);
      var nestedTopic2          = TopicFactory.Create("Nested2", "Page", nestedTopics, 2);
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var nestedTopicsData      = new TopicData() {
        Key                     = nestedTopics.Key,
        UniqueKey               = nestedTopics.GetUniqueKey(),
        ContentType             = nestedTopics.ContentType
      };

      topicData.Children.Add(nestedTopicsData);

      nestedTopicsData.Children.Add(
        new() {
          Key                     = "Nested2",
          UniqueKey               = nestedTopic2.GetUniqueKey(),
          ContentType             = nestedTopic2.ContentType
        }
      );

      topic.Import(
        topicData,
        new() {
          Strategy              = ImportStrategy.Replace
        }
      );

      var mappedNestedTopic     = nestedTopics.Children.FirstOrDefault();

      Assert.IsNotNull(mappedNestedTopic);
      Assert.AreEqual<int>(1, nestedTopics.Children.Count);
      Assert.AreEqual(mappedNestedTopic, nestedTopic2);

    }

  } //Class
} //Namespace