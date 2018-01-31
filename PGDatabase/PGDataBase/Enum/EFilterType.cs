using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    /// <summary> Determine which property should be select to <see cref="DBParameter"/>. </summary>
    internal enum EFilterType {
        /// <summary> Only select property which the property name is in the specify list. </summary>
        IncludeList,
        
        /// <summary> Select all property but the property name is not in the specify list. </summary>
        ExcludeList,

        /// <summary> Select the primary key only.( PropertyList will not used ) </summary>
        PrimaryKey,

        /// <summary> Select all property but not primary key.( PropertyList will not used ) </summary>
        NonPrimaryKey,

        /// <summary> Select all property.( PropertyList will not used ) </summary>
        All,
    } // internal enum EFilterType

} // namespace PGLibrary.Database
