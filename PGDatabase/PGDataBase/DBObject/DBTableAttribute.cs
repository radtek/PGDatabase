using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe;

namespace PGLibrary.Database {
    
    /// <summary>
        /// Set the class to be a DB Table, to use extension in PGLibrary.Database.
        /// * When class add this attribute, all field/property of this class will be DBColumn by there name.
        /// => to avoid some field/property to be DBColumn, add the DBColumnAttribute to that field/property and set ConvertType property of DBColumnAttribute.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class DBTableAttribute : Attribute {

        #region Property & Constructor

        /// <summary> Determine how to determine the member is column or not. </summary>
        public EDefaultColumns DefaultColumns { get; set; } = EDefaultColumns.All;

        /// <summary> Specify a table name, if null, use the class's name to instead it. </summary>
        public string TableName { get; set; }

        #endregion

        #region Static member
        
        private static Dictionary<Type,DBTableAttribute> sCorrespondList = new Dictionary<Type, DBTableAttribute>();

        /// <summary> Get DBTableAttribute from type of class. </summary>
        /// <param name="classType"> type to found attribute. </param>
        internal static DBTableAttribute GetCustomAttributesFromClass( Type classType ){
            if ( sCorrespondList.ContainsKey( classType ) ) return sCorrespondList[classType];
            else {
                var attr = classType.GetCustomAttributes( typeof( DBTableAttribute ), true ).FirstOrDefault()?.As<DBTableAttribute>();
                sCorrespondList.Add( classType, attr );
                return attr;
            } // else
        } // internal static DBTableAttribute GetCustomAttributesFromClass( Type classType )

        /// <summary> Get the Table name from DBTableAttribute from type. </summary>
        /// <param name="classType"> Type of class to get Table name. </param>
        /// <returns> Table name from DBTableAttribute or null. </returns>
        public static DBTable GetDBTable( Type classType ) {
            return new DBTable( GetCustomAttributesFromClass( classType )?.TableName ?? classType.Name );
        } // public static DBTable GetDBTable( Type classType )

        /// <summary> Get the Table name from DBTableAttribute from type. </summary>
        /// <typeparam name="T">Type of class to get Table name</typeparam>
        /// <returns> Table name from DBTableAttribute or null. </returns>
        public static DBTable GetDBTable<T>() {
            return new DBTable( GetCustomAttributesFromClass( typeof( T ) )?.TableName ?? typeof( T ).Name );
        } // public static DBTable GetDBTable<T>()

        
        /// <summary>
        /// Clear all cache of DBTableAttribute.
        /// * Useually use after changed attribute or it's value at runtime. ( EX: use TypeDescriptor to change )
        /// * Clear cache to re-get the DBTableAttribute of specify type.
        /// * Note: should call <see cref="DBColumnAttribute.ClearCache()"/> also.
        /// </summary>
        public static void ClearCache() {
            sCorrespondList.Clear();
        } // public static void ClearCache()
        
        /// <summary>
        /// Clear specify cache of DBTableAttribute with class type.
        /// * Useually use after changed attribute or it's value at runtime. ( EX: use TypeDescriptor to change )
        /// * Clear cache to re-get the DBTableAttribute of specify type.
        /// * Note: should call <see cref="DBColumnAttribute.ClearCache(Type[])"/> also.
        /// </summary>
        /// <param name="types">The types in cache to clear.</param>
        public static void ClearCache( params Type[] types ) {
            foreach ( var type in types )
                sCorrespondList.Remove( type );
        } // public static void ClearCache( params Type[] types )

        #endregion

    } // public class DBTableAttribute : Attribute
} // namespace PGLibrary.Database
