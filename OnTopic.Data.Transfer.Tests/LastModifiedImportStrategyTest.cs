/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Data.Transfer.Interchange;

namespace OnTopic.Data.Transfer.Tests {

  /*============================================================================================================================
  | CLASS: LAST MODIFIED IMPORT STRATEGY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicExtensions"/> class using customizations of the <see
  ///   cref="LastModifiedImportStrategy"/> options.
  /// </summary>
  [TestClass]
  public class LastModifiedImportStrategyTest {

    /*==========================================================================================================================
    | HELPER: GET TOPIC WITH NEWER TOPIC DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    private Tuple<Topic, TopicData> GetTopicWithNewerTopicData(DateTime? targetDate = null, DateTime? sourceDate = null) {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };
      targetDate                ??= DateTime.Now.AddDays(1);
      sourceDate                ??= DateTime.Now.AddHours(1);

      topic.Attributes.SetValue("LastModifiedBy", "Old Value");
      topic.Attributes.SetValue("LastModified", targetDate.ToString());

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModifiedBy",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = sourceDate.ToString(),
          LastModified          = sourceDate?? DateTime.Now.AddHours(1)
        }
      );

      return new Tuple<Topic, TopicData>(topic, topicData);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS INHERIT: TOPIC DATA WITH LAST MODIFIED BY: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModifiedBy</c> value and ensures that the existing
    ///   <c>LastModifiedBy</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.Inherit"/>. In this
    ///   case, it will be defaulting to <see cref="ImportStrategy.Add"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsInherit_TopicDataWithLastModifiedBy_SkipsExistingValue() {

      var (topic, topicData)    = GetTopicWithNewerTopicData();

      topic.Import(topicData);

      Assert.AreEqual("Old Value", topic.Attributes.GetValue("LastModifiedBy"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS CURRENT: TOPIC DATA WITH LAST MODIFIED: REPLACES NEWER VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModifiedBy</c> value and ensures that the imported value is set
    ///   to <see cref="ImportOptions.CurrentUser"/> instead when using <see cref="LastModifiedImportStrategy.Current"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsCurrent_TopicDataWithLastModifiedBy_ReplacesNewerValue() {

      var (topic, topicData)    = GetTopicWithNewerTopicData();

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedByStrategy  = LastModifiedImportStrategy.Current,
          CurrentUser           = "Jeremy"
        }
      );

      Assert.AreEqual("Jeremy", topic.Attributes.GetValue("LastModifiedBy"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS SYSTEM: TOPIC DATA WITH LAST MODIFIED BY: REPLACES NEWER VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModifiedBy</c> value and ensures that the imported value is set
    ///   to <c>System</c> instead when using <see cref="LastModifiedImportStrategy.System"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsSystem_TopicDataWithLastModifiedBy_ReplacesNewerValue() {

      var (topic, topicData)    = GetTopicWithNewerTopicData();

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedByStrategy = LastModifiedImportStrategy.System,
          CurrentUser           = "Jeremy"
        }
      );

      Assert.AreEqual("System", topic.Attributes.GetValue("LastModifiedBy"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS TARGET VALUE: TOPIC DATA WITH LAST MODIFIED BY: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModifiedBy</c> value and ensures that the existing
    ///   <c>LastModifiedBy</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.TargetValue"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsTargetValue_TopicDataWithLastModifiedBy_SkipsExistingValue() {

      var (topic, topicData)    = GetTopicWithNewerTopicData();

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedByStrategy = LastModifiedImportStrategy.TargetValue
        }
      );

      Assert.AreEqual("Old Value", topic.Attributes.GetValue("LastModifiedBy"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS INHERIT: TOPIC DATA WITH LAST MODIFIED: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the existing
    ///   <c>LastModified</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.Inherit"/>. In this
    ///   case, it will be defaulting to <see cref="ImportStrategy.Add"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsInherit_TopicDataWithLastModified_SkipsExistingValue() {

      var oldTime               = DateTime.Now.AddDays(-2);
      var (topic, topicData)    = GetTopicWithNewerTopicData(oldTime);

      topic.Import(topicData);

      Assert.AreEqual(oldTime.ToString(), topic.Attributes.GetValue("LastModified"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS CURRENT: TOPIC DATA WITH LAST MODIFIED: REPLACES NEWER VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the imported value is set to
    ///   the current time instead when using <see cref="LastModifiedImportStrategy.Current"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsCurrent_TopicDataWithLastModified_ReplacesNewerValue() {

      var tomorrow              = DateTime.Now.AddDays(1);
      var (topic, topicData)    = GetTopicWithNewerTopicData(tomorrow, tomorrow);

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.System
        }
      );

      Assert.IsTrue(topic.Attributes.GetDateTime("LastModified", tomorrow) < DateTime.Now);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS SYSTEM: TOPIC DATA WITH LAST MODIFIED: REPLACES NEWER VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the imported value is set to
    ///   <c>System</c> instead when using <see cref="LastModifiedImportStrategy.System"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsSystem_TopicDataWithLastModified_ReplacesNewerValue() {

      var tomorrow              = DateTime.Now.AddDays(1);
      var (topic, topicData)    = GetTopicWithNewerTopicData(tomorrow, tomorrow);

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.System,
        }
      );

      Assert.IsTrue(topic.Attributes.GetDateTime("LastModified", tomorrow) <= DateTime.Now);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS TARGET VALUE: TOPIC DATA WITH LAST MODIFIED: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the existing
    ///   <c>LastModified</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.TargetValue"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsTargetValue_TopicDataWithLastModified_SkipsExistingValue() {

      var yesterday             = DateTime.Now.AddDays(-1);
      var (topic, topicData)    = GetTopicWithNewerTopicData(yesterday);

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.TargetValue
        }
      );

      Assert.AreEqual(yesterday.ToString(), topic.Attributes.GetValue("LastModified"));

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS SYSTEM: TOPIC DATA WITH CHANGES: REPLACES NEWER VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the existing
    ///   <c>LastModified</c> attribute value is overwritten when using <see cref="LastModifiedImportStrategy.System"/> and
    ///   other values have changed.
    /// </summary>
    [TestMethod]
    public void ImportAsSystem_TopicDataWithChanges_ReplacesNewerValue() {

      var topic                 = TopicFactory.Create("Test", "Container", 1);
      var relatedTopic          = TopicFactory.Create("Related", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var tomorrow              = DateTime.Now.AddDays(1);
      var nextWeek              = DateTime.Now.AddDays(7);
      var relationshipData      = new RelationshipData() {
        Key                   = "Related"
      };

      topic.Relationships.SetTopic("Related", relatedTopic);
      topic.Attributes.SetValue("LastModified", tomorrow.ToString(), false);

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = nextWeek.ToString(),
          LastModified          = nextWeek
        }
      );

      topicData.Relationships.Add(relationshipData);
      relationshipData.Relationships.Add(relatedTopic.GetUniqueKey() + ":New");

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.System
        }
      );

      Assert.IsTrue(topic.Attributes.GetDateTime("LastModified", nextWeek) <= DateTime.Now);

    }

    /*==========================================================================================================================
    | TEST: IMPORT AS SYSTEM: TOPIC DATA WITHOUT CHANGES: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModified</c> value and ensures that the existing
    ///   <c>LastModified</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.System"/> and no
    ///   values have changed.
    /// </summary>
    [TestMethod]
    public void ImportAsSystem_TopicDataWithoutChanges_SkipsExistingValue() {


      var topic                 = TopicFactory.Create("Test", "Container", 1);
      var relatedTopic          = TopicFactory.Create("Related", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = topic.ContentType
      };
      var tomorrow              = DateTime.Now.AddDays(1);
      var nextWeek              = DateTime.Now.AddDays(7);
      var relationshipData      = new RelationshipData() {
        Key                   = "Related"
      };

      topic.Relationships.SetTopic("Related", relatedTopic);
      topic.Attributes.SetValue("LastModified", tomorrow.ToString(), false);

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = nextWeek.ToString(),
          LastModified          = nextWeek
        }
      );

      topicData.Relationships.Add(relationshipData);
      relationshipData.Relationships.Add(relatedTopic.GetUniqueKey());

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.System
        }
      );

      Assert.AreEqual(tomorrow.ToString(), topic.Attributes.GetValue("LastModified"));

    }

  } //Class
} //Namespace