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

  } //Class
} //Namespace