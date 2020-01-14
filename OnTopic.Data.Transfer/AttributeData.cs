/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="AttributeData"/> class provides an intermediary data transfer object for facilitating the interchange of
  ///   <see cref="AttributeValue"/> objects with JSON data.
  /// </summary>
  /// <remarks>
  ///   Having a separate class for this serializing topic data introduces some overhead in converting the topic graph to and
  ///   from <see cref="TopicData"/> objects, but in turn greatly simplifies how the serialization process works, and provides
  ///   necessary flexibility in the import process to better account for merging data and handling potential conflicts.
  /// </remarks>
  public class AttributeData {

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the key of the attribute.
    /// </summary>
    public string? Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the current value of the attribute.
    /// </summary>
    public string? Value { get; set; }

    /*==========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the the last time the <see cref="AttributeData"/> instance was updated.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.MinValue;

  } //Class
} //Namespace