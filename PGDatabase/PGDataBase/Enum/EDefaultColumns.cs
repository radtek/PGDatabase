using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    /// <summary> Declared how to determine the member is column or not. </summary>
    [Flags]
    public enum EDefaultColumns {
        /// <summary> All of member is not columns by default. </summary>
        None = 0,
        /// <summary> All of property is columns by default. </summary>
        Property = 1,
        /// <summary> All of field is columns by default. </summary>
        Field = 2,
        /// <summary> All of member is columns by default. </summary>
        All = Property | Field,
    } // public enum EDefaultColumns
} // namespace PGLibrary.Database
