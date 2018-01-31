using System;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Special rule for DBParameter. </summary>
    [Flags]
    public enum EColumnRule {
        /// <summary> No any special rule. </summary>
        None = 0,
        
        /// <summary> Use the varchar type when type is string.( Default is use nvarchar )( only in SQLServer ). </summary>
        UseVarCharType = 1,
        
        /// <summary> Use the string to convert between db and Enum value( only affect Enum type Field/Property ). </summary>
        EnumToString = 2,
        
        /// <summary> Use the Description to convert between db and Enum value( only affect Enum type Field/Property ). </summary>
        EnumDescription = 8,
        
        /// <summary> Use the Description to convert between db and Enum value with ignoreCase( only affect Enum type Field/Property ). </summary>
        EnumDescriptionIgnoreCase = 16 | EnumDescription,
        
        /// <summary> Use the <see cref="EnumStringValue"/> to convert between db and Enum value( only affect Enum type Field/Property ). </summary>
        EnumStringValue = 32,
        
        /// <summary> Use the <see cref="EnumStringValue"/> to convert between db and Enum value with ignoreCase( only affect Enum type Field/Property ). </summary>
        EnumStringValueIgnoreCase = 64 | EnumStringValue,
    } // public enum EColumnRule

} // namespace PGLibrary.Database
