/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Collections.Specialized;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: RECORD DATA
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="RecordData"/> class provides an intermediary data transfer object for facilitating the interchange of
  ///   <see cref="TrackedRecord{T}"/> objects with JSON data.
  /// </summary>
  /// <remarks>
  ///   Having a separate class for this serializing topic data introduces some overhead in converting the topic graph to and
  ///   from <see cref="TopicData"/> objects, but in turn greatly simplifies how the serialization process works, and provides
  ///   necessary flexibility in the import process to better account for merging data and handling potential conflicts.
  /// </remarks>
  public class RecordData {

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Key"/> of the <see cref="RecordData"/>.
    /// </summary>
    [NotNull, DisallowNull]
    public string? Key { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="Value"/> of the <see cref="RecordData"/>.
    /// </summary>
    public string? Value { get; set; }

    /*==========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the the last time the <see cref="RecordData"/> instance was updated.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.MinValue;

  } //Class
} //Namespace