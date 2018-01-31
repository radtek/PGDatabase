using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    /// <summary> Define this field how to convert between class and DB. </summary>
    [Flags]
    public enum EConvertType {

        /// <summary> the field not DBColumn, skip any convert. </summary>
        NotDBColumn = 0x0,

        /// <summary> the field only use when convert it to DB. </summary>
        ToDB = 0x1,

        /// <summary> the field only use when convert it from DB. </summary>
        FromDB = 0x2,

        /// <summary> the field will use when convert it from/to DB. </summary>
        Twoway = ToDB | FromDB,

    } // public enum EConvertType
} // namespace PGLibrary.Database
