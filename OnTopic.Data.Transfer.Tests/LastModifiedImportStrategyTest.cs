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
    | TEST: IMPORT AS INHERIT: TOPIC DATA WITH LAST MODIFIED BY: SKIPS EXISTING VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicData"/> with a newer <c>LastModifiedBy</c> value and ensures that the existing
    ///   <c>LastModifiedBy</c> attribute value is retained when using <see cref="LastModifiedImportStrategy.Inherit"/>. In this
    ///   case, it will be defaulting to <see cref="ImportStrategy.Add"/>.
    /// </summary>
    [TestMethod]
    public void ImportAsInherit_TopicDataWithLastModifiedBy_SkipsExistingValue() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Attributes.SetValue("LastModifiedBy", "Old Value");

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModifiedBy",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Attributes.SetValue("LastModifiedBy", "Old Value");

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModifiedBy",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Attributes.SetValue("LastModifiedBy", "Old Value");

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModifiedBy",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };

      topic.Attributes.SetValue("LastModifiedBy", "Old Value");

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModifiedBy",
          Value                 = "New Value",
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };
      var oldTime               = DateTime.Now.AddDays(-2).ToString();

      topic.Attributes.SetValue("LastModified", oldTime);

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = DateTime.Now.AddDays(1).ToString(),
          LastModified          = DateTime.Now.AddDays(1)
        }
      );

      topic.Import(topicData);

      Assert.AreEqual(oldTime, topic.Attributes.GetValue("LastModified"));

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };
      var tomorrow              = DateTime.Now.AddDays(1);

      topic.Attributes.SetValue("LastModified", tomorrow.ToString());

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = tomorrow.ToString(),
          LastModified          = tomorrow
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };
      var tomorrow              = DateTime.Now.AddDays(1);

      topic.Attributes.SetValue("LastModified", tomorrow.ToString());

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = tomorrow.ToString(),
          LastModified          = tomorrow
        }
      );

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

      var topic                 = TopicFactory.Create("Test", "Container");
      var topicData             = new TopicData() {
        Key                     = topic.Key,
        UniqueKey               = topic.GetUniqueKey(),
        ContentType             = "Page"
      };
      var yesterday             = DateTime.Now.AddDays(-1);
      var tomorrow              = DateTime.Now.AddDays(1);

      topic.Attributes.SetValue("LastModified", yesterday.ToString());

      topicData.Attributes.Add(
        new AttributeData() {
          Key                   = "LastModified",
          Value                 = tomorrow.ToString(),
          LastModified          = tomorrow
        }
      );

      topic.Import(
        topicData,
        new ImportOptions() {
          LastModifiedStrategy  = LastModifiedImportStrategy.TargetValue
        }
      );

      Assert.AreEqual(yesterday.ToString(), topic.Attributes.GetValue("LastModified"));

    }

  } //Class
} //Namespace