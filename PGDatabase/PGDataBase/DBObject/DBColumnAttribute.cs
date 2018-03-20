#pragma warning disable CS1587
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using PGCafe;
using PGCafe.Object.OldVersionUse;

namespace PGLibrary.Database {

    /// <summary>
    /// Set the field to be a DB Column, to use extension in PGLibrary.Database.
    /// if need specify ColumnName, set ColumnName property.
    /// if need skip this field of DB Column, try set the ConvertType property.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false )]
    public class DBColumnAttribute : Attribute {

        #region Static & Const

        /// <summary> Default attribute instance with null ColumnName. </summary>
        public static DBColumnAttribute Default = new DBColumnAttribute();

        #endregion

        #region Property & Constructor

        /// <summary> Specific column name, if null, get the field's name to instead it. </summary>
        public string ColumnName { get; set; }

        /// <summary> determine this field's is primary key or not. </summary>
        public bool PrimaryKey { get; set; } = false;

        /// <summary> determine this field's behavior between class and DB. </summary>
        public EConvertType ConvertType { get; set; } = EConvertType.Twoway;

        /// <summary> determine this field has any ColumnRule or not. </summary>
        public EColumnRule ColumnRule { get; set; } = EColumnRule.None;

        /// <summary> determine the key when convert string to enum of method EnumStringValue. </summary>
        public string EnumStringValueKey { get; set; }

        /// <summary> Set the field to be a DB Column, to use extension in PGLibrary.Database. </summary>
        /// <param name="Name"> Specify column name, if null, use the field's name to instead it </param>
        /// <param name="ConvertType"> define this field how to convert between class and DB. </param>
        public DBColumnAttribute( string Name = null, EConvertType ConvertType = EConvertType.Twoway ) {
            this.ColumnName = Name;
            this.ConvertType = ConvertType;
        } // public DBColumnAttribute( string Name = null, EConvertType ConvertType = EConvertType.Twoway )

        /// <summary> Set the field to be a DB Column, to use extension in PGLibrary.Database.  </summary>
        /// <param name="ConvertType"> define this field how to convert between class and DB. </param>
        public DBColumnAttribute( EConvertType ConvertType ) {
            this.ConvertType = ConvertType;
        } // public DBColumnAttribute( EConvertType ConvertType )

        #endregion

        #region GetCorrespondMember

        private static Dictionary<Type, ReadOnlyDictionary<string, ReadOnlyCollection<_DBColumnAttribute_MemberInfo>>> sCorrespondMemberList = null;

        /// <summary> Get all member of class Type which has turn on the DBColumnAttribute, and return the correspond column name. </summary>
        /// <param name="FromType"> Type to analyze member. </param>
        /// <returns> Dictionary with Key = ColumnName, Value = Match Fields/Propertys info. ( it may have many member to match one column name ) </returns>
        internal static ReadOnlyDictionary<string, ReadOnlyCollection<_DBColumnAttribute_MemberInfo>> GetCorrespondMember( Type FromType ) {
            // try find in exist list in [sCorrespondMemberList].
            if ( sCorrespondMemberList == null )
                sCorrespondMemberList = new Dictionary<Type, ReadOnlyDictionary<string, ReadOnlyCollection<_DBColumnAttribute_MemberInfo>>>();

            /// if has found, return it.
            if ( sCorrespondMemberList.ContainsKey( FromType ) )
                return sCorrespondMemberList[FromType];

            /// if no found, calculate it
            /// 
            // result list of field/property.
            var tmpList = new Dictionary<string, ReadOnlyCollectionBuilder<_DBColumnAttribute_MemberInfo>>( StringComparer.OrdinalIgnoreCase );

            /// get result form fields.
            var fields = FromType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

            // filter CompilerGenerated field.
            var fields_WithoutCompilerGeneratedField =
                fields.Where( item => !item.GetCustomAttributes( typeof( CompilerGeneratedAttribute ), false ).Any() ).ToArray();

            foreach ( var field in fields_WithoutCompilerGeneratedField ) {

                // try get attribute from field, if no attribute of field, try get class attribute.
                var attr = field.GetCustomAttributes( typeof( DBColumnAttribute ), false ).FirstOrDefault()?.As<DBColumnAttribute>();

                string columnName;
                DBColumnAttribute columnAttr;
                if ( attr != null ) { // get column name from attribute if swtich is on.
                    columnName = attr.ConvertType != EConvertType.NotDBColumn ? ( attr.ColumnName ?? field.Name ) : null;
                    columnAttr = attr;
                } // if
                else { // determine the field is Column or not by DBTableAttribute.
                    var classAttr = DBTableAttribute.GetCustomAttributesFromClass( field.DeclaringType );
                    // if has no DBTableAttribute, or DBTableAttribute says Column has contains field, set Twoway.
                    if ( classAttr?.DefaultColumns.HasFlag( EDefaultColumns.Field ) ?? true ) {
                        columnName = field.Name;
                        columnAttr = Default;
                    } // if
                    else { // if has DBTableAttribute, but DBTableAttribute says not Column is not contain Field, set NotDBColumn.
                        columnName = null;
                        columnAttr = null;
                    } // else
                } // else

                // if the field is Column, add to result.
                if ( columnName != null ) {
                    if ( !tmpList.ContainsKey( columnName ) ) // if no key of column name, add the key first.
                        tmpList.Add( columnName, new ReadOnlyCollectionBuilder<_DBColumnAttribute_MemberInfo>() );

                    // add the info to column name's list.
                    tmpList[columnName].Add( new _DBColumnAttribute_MemberInfo( field.Name, field.FieldType, columnAttr, field.SetValue, field.GetValue ) );
                } // if
            } // foreach


            /// get result from properties.
            var properties = FromType.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            foreach ( var property in properties ) {
                // try get attribute from field,  if no attribute of field, get attribute from class.
                var attr = property.GetCustomAttributes( typeof( DBColumnAttribute ), false ).FirstOrDefault()?.As<DBColumnAttribute>();

                string columnName;
                DBColumnAttribute columnAttr;
                if ( attr != null ) { // get column name from attribute if swtich is on.
                    columnName = ( !attr?.ConvertType.Equals( EConvertType.NotDBColumn ) ?? false ) ? ( attr.ColumnName ?? property.Name ) : null;
                    columnAttr = attr;
                } // if
                else { // determine the property is Column or not by DBTableAttribute.
                    var classAttr = DBTableAttribute.GetCustomAttributesFromClass( property.DeclaringType );
                    // if has no DBTableAttribute, or DBTableAttribute says Column has contains property, set Twoway.
                    if ( classAttr?.DefaultColumns.HasFlag( EDefaultColumns.Property ) ?? true ) {
                        columnName = property.Name;
                        columnAttr = Default;
                    } // if
                    else { // if has DBTableAttribute, but DBTableAttribute says not Column is not contain property, set NotDBColumn.
                        columnName = null;
                        columnAttr = null;
                    } // else
                } // else

                // if the Property is Column, add to result.
                if ( columnName != null ) {
                    if ( !tmpList.ContainsKey( columnName ) ) // if no key of column name, add the key first.
                        tmpList.Add( columnName, new ReadOnlyCollectionBuilder<_DBColumnAttribute_MemberInfo>() );

                    // add the info to column name's list.
                    /// ** To match the needed type, use anonymous method call [SetValue] and [GetValue] with null value of [index] parameter.
                    /// ** in Framework 4.5, PropertyInfo has add the new method of [SetValue] and [GetValue] that without [index] parameter.
                    tmpList[columnName].Add(
                        new _DBColumnAttribute_MemberInfo(
                            property.Name,
                            property.PropertyType,
                            columnAttr,
                            property.CanWrite ? new Action<object, object>( ( arg1, arg2 ) => property.SetValue( arg1, arg2, null ) ) : null,
                            property.CanRead ? new Func<object, object>( ( arg1 ) => property.GetValue( arg1, null ) ) : null
                            ) );
                } // if
            } // foreach

            // convert tmpList to ReadOnlyCollection Values.
            var tmpReulst = new Dictionary<string, ReadOnlyCollection<_DBColumnAttribute_MemberInfo>>();
            foreach ( var item in tmpList )
                tmpReulst[item.Key] = item.Value.ToReadOnlyCollection();

            /// save the result for next call this method with same type.
            sCorrespondMemberList.Add( FromType,
                new ReadOnlyDictionary<string, ReadOnlyCollection<_DBColumnAttribute_MemberInfo>>( tmpReulst ) );

            return sCorrespondMemberList[FromType];
        } // internal static ReadOnlyDictionary<string,ReadOnlyCollection<_DBColumnAttribute_MemberInfo>> GetCorrespondMember( Type FromType )

        /// <summary>
        /// Clear all cache of DBColumnAttributes.
        /// * Useually use after changed attribute or it's value at runtime. ( EX: use TypeDescriptor to change )
        /// * Clear cache to re-get the Columns name from DBColumnAttribute in specify type.
        /// * Note: should call <see cref="DBTableAttribute.ClearCache()"/> also.
        /// </summary>
        public static void ClearCache() {
            sCorrespondMemberList.Clear();
        } // public static void ClearCache()

        /// <summary>
        /// Clear specify cache of DBColumnAttributes with class type.
        /// * Useually use after changed attribute or it's value at runtime. ( EX: use TypeDescriptor to change )
        /// * Clear cache to re-get the Columns name from DBColumnAttribute in specify type.
        /// * Note: should call <see cref="DBTableAttribute.ClearCache(Type[])"/> also.
        /// </summary>
        /// <param name="types">The types in cache to clear.</param>
        public static void ClearCache( params Type[] types ) {
            foreach ( var type in types )
                sCorrespondMemberList.Remove( type );
        } // public static void ClearCache( params Type[] types )

        #endregion

    } // public class DBColumnAttribute : Attribute



    /// <summary>
    /// Temp class of internal method use.
    /// save the necessary value of field and property.
    /// </summary>
    internal class _DBColumnAttribute_MemberInfo {

        public string Name; // Field/Property Name.
        public Type Type; // Type of Field/Property.
        public DBColumnAttribute ColumnAttribute; // DBColumnAttribute of Field/Property.
        public Action<object, object> SetValue; // SetValue method of Field/Property. ( if is readonly Property, this will be null ).
        public Func<object, object> GetValue; // GetValue method of Field/Property. ( if is writeonly Property, this will be null ).

        /// <summary> Constuctor </summary>
        /// <param name="Name"> Field/Property Name </param>
        /// <param name="Type"> Type of Field/Property </param>
        /// <param name="ColumnAttribute"> DBColumnAttribute of Field/Property </param>
        /// <param name="SetValue"> SetValue method of Field/Property. ( if is readonly Property, pass null value ) </param>
        /// <param name="GetValue"> GetValue method of Field/Property. ( if is writeonly Property, pass null value ) </param>
        public _DBColumnAttribute_MemberInfo( string Name, Type Type, DBColumnAttribute ColumnAttribute, Action<object, object> SetValue, Func<object, object> GetValue ) {
            this.Name = Name;
            this.Type = Type;
            this.SetValue = SetValue;
            this.GetValue = GetValue;
            this.ColumnAttribute = ColumnAttribute;
        } // public _DBColumnAttribute_MemberInfo( string Name, Type Type, DBColumnAttribute ColumnAttribute, Action<object,object> SetValue, Func<object,object> GetValue )

    } // internal class _DBColumnAttribute_MemberInfo

} // namespace PGLibrary.Database
