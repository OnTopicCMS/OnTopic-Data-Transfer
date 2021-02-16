/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: MULTIMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="MultiMap"/> class provides a <see cref="KeyedCollection"/> of <see cref="KeyValuesPair"/> objects, thus
  ///   supporting a many-to-many mapping.
  /// </summary>
  /// <remarks>
  ///   The <see cref="MultiMap"/> is intended to model the <see cref="TopicMultiMap"/>—though instead of a exposing a
  ///   collection of <see cref="Topic"/> references, it instead exposes a list of <see cref="String"/>s intended to map to <see
  ///   cref="TopicData.UniqueKey"/>s, thus providing a serializable format for handling e.g. <see cref="TopicData.Relationships
  ///   "/>.
  /// </remarks>
  public class MultiMap: KeyedCollection<string, KeyValuesPair> {

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the <see cref="MultiMap"/> to identify the appropriate <see cref="Key"/> from each <see
    ///   cref="KeyValuesPair"/> object.
    /// </summary>
    /// <param name="item">The <see cref="KeyValuesPair"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(KeyValuesPair item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key?? "";
    }

  } //Class
} //Namespace