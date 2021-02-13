/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DATA COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="AttributeDataCollection"/> class provides a <see cref="KeyedCollection"/> of <see cref="RecordData"/>
  ///   objects.
  /// </summary>
  public class AttributeDataCollection: KeyedCollection<string, RecordData> {

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the <see cref="AttributeDataCollection"/> to identify the appropriate <see cref="Key"/>
    ///   from each <see cref="RecordData"/> object.
    /// </summary>
    /// <param name="item">The <see cref="RecordData"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(RecordData item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key?? "";
    }

  } //Class
} //Namespace