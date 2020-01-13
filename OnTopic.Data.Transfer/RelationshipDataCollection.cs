/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Transfer {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP DATA COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="RelationshipDataCollection"/> class provides a <see cref="KeyedCollection"/> of <see
  ///   cref="RelationshipData"/> objects.
  /// </summary>
  public class RelationshipDataCollection: KeyedCollection<string, RelationshipData> {

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the <see cref="RelationshipDataCollection"/> to identify the appropriate <see
    ///   cref="Key"/> from each <see cref="RelationshipData"/> object.
    /// </summary>
    /// <param name="item">The <see cref="RelationshipData"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(RelationshipData item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key?? "";
    }

  } //Class
} //Namespace