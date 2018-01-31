using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PGCafe;
using PGCafe.Object;

namespace PGLibrary.Database {

    /// <summary> Implement this without any member or method, just proviod some extension for entity and DataTable. </summary>
    public interface IDatabaseTable {
    } // public interface IDatabaseTable
    

    /// <summary> IDatabaseTable's Extension </summary>
    public static class IDatabaseTableExtension {
        
        #region ToEntity
        
        /// <summary>
        /// Parse DataRow's field to a class of T ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have set method.
        /// *.if parse fail, will set the default value to [property/field].
        /// *.return exception with all fail convert.
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="target"> target to copy value. </param>
        /// <param name="throwException"> throw the exception when convert failed.  if not throw, will return all exception. </param>
        public static List<Exception> ToEntity<T>( this DataRow source, T target, bool throwException = true )
            where T : class, IDatabaseTable {
            if ( target == null ) throw new ArgumentNullException( nameof( target ) );

            // get mapping member from target type.
            var membersHasColumnAttribute = DBColumnAttribute.GetCorrespondMember( typeof( T ) );

            List<Exception> result = null;
            foreach ( DataColumn column in source.Table.Columns ) {
                try {
                    // try get member from column name, if not exist, skip this column.
                    var members = membersHasColumnAttribute.GetValue( column.ColumnName );
                    if ( members == null) continue;

                    // get value object from source.
                    var valueObj = source[column];

                    // set value to all member that is mapping.
                    foreach ( var member in members.Where( item => item.ColumnAttribute.ConvertType.HasFlag( EConvertType.FromDB ) ) ) {
                        try {
                            if ( member.SetValue == null ) continue;

                            var memberUnderlyingType = Nullable.GetUnderlyingType( member.Type );

                            // set value object to member.
                            if ( valueObj == DBNull.Value || valueObj == null ) // if value is DBNull, set default value to target.
                                member.SetValue( target, default( T ) );
                            else if ( valueObj.GetType() == member.Type ) // if is same type, just set it.
                                member.SetValue( target, valueObj );

                            // if value is Enum and target is string and has EColumnRule.EnumDescription, Convert it by description
                            else if ( valueObj is string && ( member.Type.IsEnum || memberUnderlyingType.IsEnum )
                                && ( member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescription )
                                    || member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescriptionIgnoreCase ) ) ) {

                                var ignoreCase = member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescriptionIgnoreCase );
                                var enumType = memberUnderlyingType ?? member.Type;
                                member.SetValue( target, valueObj.ToString().ToEnumByDescription( enumType, ignoreCase ) );
                            } // else if
                            
                            // if value is Enum and target is string and has EColumnRule.EnumStringValue, Convert it by StringValue
                            else if ( valueObj is string && ( member.Type.IsEnum || memberUnderlyingType.IsEnum )
                                && ( member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumStringValue )
                                    || member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumStringValueIgnoreCase ) ) ) {

                                var ignoreCase = member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumStringValueIgnoreCase );
                                var enumType = memberUnderlyingType ?? member.Type;
                                member.SetValue( target, valueObj.ToString().ToEnumByStringValue( enumType, ignoreCase ) );
                            } // else if

                            else { // try convert value to target type.
                                if ( throwException ) {
                                    // convert first to throw exception if has.
                                    var value = valueObj.ToType( member.Type );
                                    
                                    // if convert success, make defaultValue and set to property
                                    var defaultValue = member.Type.IsValueType ? Activator.CreateInstance( member.Type ) : null;
                                    member.SetValue( target, value ?? defaultValue );
                                } // if
                                else {
                                    // make defaultValue first, and try convert it, if fail, use defaultValue.
                                    var defaultValue = member.Type.IsValueType ? Activator.CreateInstance( member.Type ) : null;
                                    member.SetValue( target, valueObj.ToType( member.Type, defaultValue ) );
                                } // else
                            } // else
                        } catch ( Exception ex ) {
                            if ( result == null ) result = new List<Exception>();
                            result.Add( new Exception( $"Error Occur at ColumnName:{column.ColumnName}, MemberName:{member.Name}", ex ) );
                            continue;
                        } // try-catch
                    } // foreach

                } catch ( Exception ex ) {
                    if ( result == null ) result = new List<Exception>();
                    result.Add( new Exception( $"Error Occur at ColumnName:{column.ColumnName}", ex ) );
                    continue;
                } // try-catch
            } // foreach

            // if need throw exception, throw it.
            if ( throwException && result?.Count > 0 )
                throw new MultipleException( "One or more exception when convert it.", result );

            return result;
        } // public static List<Exception> ToEntity<T>( this DataRow source, T target, bool throwException = true )
        


        /// <summary>
        /// Parse DataRow's field to a class of T ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have set method.
        /// *.T should have default constructor to create a new instance, otherwise try use another method TryParse( this DataRow source, ref T instance );
        /// *.if parse fail, will set the default value to [property/field].
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        public static T ToEntity<T>( this DataRow source, bool throwException = true )
            where T : class, IDatabaseTable, new() {
            // create instance.
            Type TType = typeof( T );
            T result = (T)Activator.CreateInstance( TType );

            source.ToEntity( result, throwException );

            // return.
            return result;
        } // public static T ToEntity<T>( this DataRow source, bool throwException = true )
        
        /// <summary>
        /// Parse every row in DataTable to a new list of T ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have set method.
        /// *.T should have default constructor to create a new instance, otherwise try use another method TryParse( this DataRow source, ref T instance );
        /// *.if parse fail, will set the default value to [property/field].
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="Errors"> save error's exception to list.( will clear origin item in list ) </param>
        /// <param name="throwException"> throw the exception when convert failed.  if not throw, will return all exception. </param>
        public static IEnumerable<T> ToEntity<T>( this DataTable source, ref List<Exception> Errors, bool throwException = true )
            where T : class, IDatabaseTable, new() {
            // create instance.
            Type TType = typeof( T );
            
            var result = new List<T>();
            foreach ( var row in source.AsEnumerable() ) {
                T target = (T)Activator.CreateInstance( TType );
                var exceptions = row.ToEntity<T>( target, throwException );
                if ( exceptions != null ){
                    if ( Errors == null ) Errors = new List<Exception>();
                    Errors.AddRange( exceptions );
                } // if
                result.Add( target );
            } // foreach

            return result;
        } // public static IEnumerable<T> ToEntity<T>( this DataTable source, ref List<Exception> Errors, bool throwException = true )
        
        /// <summary>
        /// Parse every row in DataTable to a new list of T ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have set method.
        /// *.T should have default constructor to create a new instance, otherwise try use another method TryParse( this DataRow source, ref T instance );
        /// *.if parse fail, will set the default value to [property/field].
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed.  if not throw, will return all exception. </param>
        public static IEnumerable<T> ToEntity<T>( this DataTable source, bool throwException = true )
            where T : class, IDatabaseTable, new() {
            // create instance.
            Type TType = typeof( T );

            var result = new List<T>();
            foreach ( var row in source.AsEnumerable() ) {
                T target = (T)Activator.CreateInstance( TType );
                row.ToEntity<T>( target, throwException );
                result.Add( target );
            } // foreach

            return result;
        } // public static IEnumerable<T> ToEntity<T>( this DataTable source, bool throwException = true )

#endregion
        
        #region FromEntity
        
        /// <summary>
        /// Set DataRow's field from a class of T by parse property/field ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have get method.
        /// *.if parse fail with value, will do nothing of field in DataRow.
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="From"> Object to copy value from. </param>
        /// <param name="Errors"> save error's exception to list.( will clear origin item in list ) </param>
        /// <param name="throwException"> throw the exception when convert failed.  if not throw, will return all exception. </param>
        public static DataRow FromEntity<T>( this DataRow source, T From, ref List<Exception> Errors, bool throwException = true )
            where T : class, IDatabaseTable {
            if ( From == null ) return null;

            // get mapping member from target type.
            var membersHasColumnAttribute = DBColumnAttribute.GetCorrespondMember( typeof( T ) );

            foreach ( DataColumn column in source.Table.Columns ) {
                try {
                    // try get member from column name, if not exist, skip this column.
                    // only get first member to get value from.
                    var member = membersHasColumnAttribute.GetValue( column.ColumnName )
                        ?.FirstOrDefault( item => item.ColumnAttribute.ConvertType.HasFlag( EConvertType.ToDB ) );
                    if ( member == null ) continue;
                    if ( member.GetValue == null ) continue;
                    
                    // get value object from member.
                    var valueObj = member.GetValue( From );

                    // get column datatype.
                    var columnDataType = column.DataType;
                    if ( column.AllowDBNull && column.DataType.IsValueType )
                        columnDataType = typeof( Nullable<> ).MakeGenericType( column.DataType );

                    // set value object to column.
                    if ( valueObj == null ) source[column] = DBNull.Value;
                    else if ( valueObj.GetType() == columnDataType ) source[column] = valueObj;

                    // if value is Enum and target is string and has EColumnRule.EnumDescription, Convert it by description
                    else if ( valueObj is Enum && columnDataType == typeof( string )
                        && ( member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescription )
                            || member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescriptionIgnoreCase ) ) ) {

                        source[column] = (object)valueObj.CastTo<Enum>().Description() ?? DBNull.Value;
                    } // else if
                    
                    // if value is Enum and target is string and has EColumnRule.EnumStringValue, Convert it by StringValue
                    else if ( valueObj is Enum && columnDataType == typeof( string )
                        && ( member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumStringValue )
                            || member.ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumStringValueIgnoreCase ) ) ) {

                        source[column] = (object)valueObj.CastTo<Enum>().StringValue() ?? DBNull.Value;
                    } // else if

                    else { // convert value object to destination type.
                        // whether successs or not, set result or default value.
                        if ( throwException ) {
                            // convert first to throw exception if has.
                            var value = valueObj.ToType( columnDataType );

                            // if convert success, make defaultValue and set to property
                            var defaultValue = columnDataType.IsValueType ? Activator.CreateInstance( columnDataType ) : null;
                            source[column] = value ?? defaultValue ?? DBNull.Value;
                        } // if
                        else {
                            // make defaultValue first, and try convert it, if fail, use defaultValue.
                            var defaultValue = columnDataType.IsValueType ? Activator.CreateInstance( columnDataType ) : null;
                            source[column] = valueObj.ToType( columnDataType, defaultValue ) ?? DBNull.Value;
                        } // else
                    } // else
                } catch ( Exception ex ){
                    if ( Errors == null ) Errors = new List<Exception>();

                    Errors.Add( new Exception( $"Error Occur at ColumnName:{column.ColumnName}", ex ) );
                    continue;
                } // try-catch
            } // foreach

            // if need throw exception, throw it.
            if ( throwException && Errors?.Count > 0 )
                throw new MultipleException( "One or more exception when convert it.", Errors );

            return source;
        } // public static DataRow FromEntity<T>( this DataRow source, T From, ref List<Exception> Errors, bool throwException = true )
        

        
        /// <summary>
        /// Set DataTable's Rows from list of class of T by parse [property/field] ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have set method.
        /// *.if parse fail with value, will do nothing of field in DataRow.
        /// *.each object in list will create a new DataRow and put value to it.
        /// *.to avoid the primary key conflict, clear all rows before calling this function.
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="From"> Object list to copy value from. </param>
        /// <param name="Errors"> save error's exception to list.( will clear origin item in list ) </param>
        /// <param name="throwException"> throw the exception when convert failed.  if not throw, will return all exception. </param>
        public static void FromEntity<T>( this DataTable source, IEnumerable<T> From, ref List<Exception> Errors, bool throwException = true )
            where T : class, IDatabaseTable {
            foreach ( var item in From ) {
                source.Rows.Add( source.NewRow().FromEntity( item, ref Errors, throwException ) );
            } // foreach
        } // public static void FromEntity<T>( this DataTable source, IEnumerable<T> From, ref List<Exception> Errors, bool throwException = true )
        
        /// <summary>
        /// Set DataRow's field from a class of T by parse property/field ( T must inherit from IDatabaseTable ).
        /// *.Parse all [property/field] of T which has DBColumnAttribute :
        ///     **.Match column name with name of DBColumnAttribute in [property/field].
        ///     **.Property should have get method.
        /// *.if parse fail with value, will do nothing of field in DataRow.
        /// </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="From"> Object to copy value from. </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        public static DataRow FromEntity<T>( this DataRow source, T From, bool throwException = true )
            where T : class, IDatabaseTable {
            List<Exception> Errors = null;
            return source.FromEntity( From, ref Errors, throwException );
        } // public static DataRow FromEntity<T>( this DataRow source, T From, bool throwException = true )

        #endregion

        #region ToDataRow, ToDataTable, ToEmptyDataTable

        /// <summary> Convert source to DataRow. ( without add to DataTable ) </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        /// <returns> DataRow with datas from source. </returns>
        public static DataRow ToDataRow<T>( this T source, bool throwException = true )
            where T : class, IDatabaseTable {

            // create datatable
            var result = IDatabaseTableExtension.CreateEmptyDataTable<T>( throwException );

            // create datarow by source and return it.
            return result?.NewRow().FromEntity( source, throwException );
        } // public static DataRow ToDataRow<T>( this T source, bool throwException = true )

        
        /// <summary> Convert source to DataRow. ( without add to DataTable ) </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        /// <returns> DataRow with datas from source. </returns>
        public static IEnumerable<DataRow> ToDataRow<T>( this IEnumerable<T> source, bool throwException = true )
            where T : class, IDatabaseTable {

            // create datatable
            var result = IDatabaseTableExtension.CreateEmptyDataTable<T>( throwException );
            if ( result == null ) yield break;

            // create datarow by source and return it.
            foreach ( var item in source )
                yield return result.NewRow().FromEntity( item, throwException );
        } // public static IEnumerable<DataRow> ToDataRow<T>( this IEnumerable<T> source, bool throwException = true )

        
        /// <summary> Convert source to DataTable. </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        /// <returns> DataTable with datas from source. </returns>
        public static DataTable ToDataTable<T>( this IEnumerable<T> source, bool throwException = true )
            where T : class, IDatabaseTable {

            // create datatable
            var result = IDatabaseTableExtension.CreateEmptyDataTable<T>( throwException );
            if ( result == null ) return null;

            // create datarow for each item in source.
            foreach ( var item in source )
                result.Rows.Add( result.NewRow().FromEntity( item, throwException ) );

            return result;
        } // public static DataTable ToDataTable<T>( this IEnumerable<T> source, bool throwException = true )
        
        
        /// <summary> Convert source to DataTable. </summary>
        /// <typeparam name="T"> type of entity to convert. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        /// <returns> DataTable with datas from source. </returns>
        public static DataTable ToDataTable<T>( this T source, bool throwException = true )
            where T : class, IDatabaseTable {

            // create datatable
            var result = IDatabaseTableExtension.CreateEmptyDataTable<T>( throwException );
            if ( result == null ) return null;

            // create datarow for each item in source.
            result.Rows.Add( result.NewRow().FromEntity( source, throwException ) );

            return result;
        } // public static DataTable ToDataTable<T>( this T source, bool throwException = true )


        /// <summary> Convert source to DataTable without data. ( only create schema ) </summary>
        /// <param name="source"> source </param>
        /// <param name="throwException"> throw the exception when convert failed. </param>
        /// <returns> DataTable without datas from source. </returns>
        public static DataTable ToEmptyDataTable( this IDatabaseTable source, bool throwException = true ) => IDatabaseTableExtension.CreateEmptyDataTable( source.GetType(), throwException );

        /// <summary> Create DataTable without data. ( only create schema ) </summary>
        /// <typeparam name="T"> type of DataTable schema. ( should be class and IDatabaseTable ) </typeparam>
        /// <param name="throwException"> throw the exception when failed. </param>
        /// <returns> Empty DataTable create from type. </returns>
        public static DataTable CreateEmptyDataTable<T>( bool throwException = true )
            where T : class, IDatabaseTable => CreateEmptyDataTable( typeof( T ), throwException );


        /// <summary> Create DataTable without data. ( only create schema ) </summary>
        /// <param name="Type"> type of DataTable schema. ( should be class and IDatabaseTable ) </param>
        /// <param name="throwException"> throw the exception when failed. </param>
        /// <returns> Empty DataTable create from type. </returns>
        public static DataTable CreateEmptyDataTable( Type Type, bool throwException = true ) {
            // Arguments check.
            if ( typeof( IDatabaseTable ).IsAssignableFrom( Type ) )
                throw new ArgumentException( $"{nameof( Type )} should implement {typeof( IDatabaseTable ).FullName}", nameof( Type ) );

            if ( !Type.IsClass )
                throw new ArgumentException( $"{nameof( Type )} should be a class", nameof( Type ) );


            // get mapping member from target type.
            var membersHasColumnAttribute = DBColumnAttribute.GetCorrespondMember( Type );

            // generate DataTable.
            var Columns = new List<DataColumn>();
            var primaryKeyColumns = new List<DataColumn>();
            foreach ( var column in membersHasColumnAttribute ) {
                var columnName = column.Key;
                var columnType = column.Value.First().Type;
                if ( columnType.IsEnum ) {
                    if ( column.Value.First().ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumDescription ) ||
                        column.Value.First().ColumnAttribute.ColumnRule.HasFlag( EColumnRule.EnumToString ) )
                        columnType = typeof( string );
                    else
                        columnType = typeof( int );
                } // if

                var dataColumn = new DataColumn( columnName, columnType );
                Columns.Add( dataColumn );

                if ( column.Value.First().ColumnAttribute.PrimaryKey )
                    primaryKeyColumns.Add( dataColumn );
            } // foreach

            var result = new DataTable();
            result.Columns.AddRange( Columns.ToArray() );
            result.PrimaryKey = primaryKeyColumns.ToArray();

            return result;
        } // public static DataTable CreateEmptyDataTable( Type Type, bool throwException = true )

        #endregion

        #region ToInsertCommand, ToUpdateCommand, ToDeleteCommand, ToCommand
        
        /// <summary>
        /// Parse <see cref="DataRow"/>'s field to <see cref="InsertCommand"/>.
        /// *.Use <see cref="DataRow.Table"/>'s TableName to set <see cref="InsertCommand"/>'s table name.
        /// </summary>
        /// <param name="source"> source </param>
        public static InsertCommand ToInsertCommand( this DataRow source ) {
            if ( source == null ) return null;
                
            // get value object from source, add to command
            var parameters = new List<DBParameter>();
            foreach ( DataColumn column in source.Table.Columns ) {
                var valueObj = source[column];
                parameters.Add( new DBParameter( column.ColumnName, valueObj ) );
            } // foreach
            
            // create command
            var result = new InsertCommand()
                .InsertInTo( new DBTable( source.Table.TableName ) )
                .Values( parameters );
                
            return result;
        } // public static InsertCommand ToInsertCommand( this DataRow source )
        
        /// <summary>
        /// Parse each row in <see cref="DataTable"/> to <see cref="InsertCommand"/>.
        /// *.Use <see cref="DataTable.TableName"/> to set <see cref="InsertCommand"/>'s table name.
        /// </summary>
        /// <param name="source"> source </param>
        public static IEnumerable<InsertCommand> ToInsertCommand( this DataTable source ) =>
            source.AsEnumerable().Select( row => row.ToInsertCommand() );

        
        
        /// <summary>
        /// Parse <see cref="DataRow"/>'s field to <see cref="UpdateCommand"/>.
        /// *.Use <see cref="DataRow.Table"/>'s TableName to set <see cref="UpdateCommand"/>'s table name.
        /// *.Use primary key to set where clause, if no primary key, use all column to set where clause.
        /// </summary>
        /// <param name="source"> source </param>
        public static UpdateCommand ToUpdateCommand( this DataRow source ) {
            if ( source == null ) return null;
                
            // get value object from source, add to command
            var setParameters = new List<DBParameter>();
            var whereParameters = new List<DBParameter>();
            var primaryKey = source.Table.PrimaryKey;
            foreach ( DataColumn column in source.Table.Columns ) {
                var valueObj = source[column];
                var dbParameter = new DBParameter( column.ColumnName, valueObj );
                setParameters.Add( dbParameter );

                // if has primary key, check is this column is primary key, if so, add it to where clause.
                if ( primaryKey.Length != 0 && primaryKey.Contains( column ) )
                    whereParameters.Add( dbParameter );
                else // if has no primary key, all column should set to where clause.
                    whereParameters.Add( dbParameter );
            } // foreach
            
            // create command
            var result = new UpdateCommand()
                .UpdateTo( new DBTable( source.Table.TableName ) )
                .Set( setParameters )
                .WhereAnd( whereParameters );
            
            return result;
        } // public static UpdateCommand ToUpdateCommand( this DataRow source )
        
        /// <summary>
        /// Parse each row in <see cref="DataTable"/> to <see cref="UpdateCommand"/>.
        /// *.Use <see cref="DataTable.TableName"/> to set <see cref="UpdateCommand"/>'s table name.
        /// </summary>
        /// <param name="source"> source </param>
        public static IEnumerable<UpdateCommand> ToUpdateCommand( this DataTable source ) =>
            source.AsEnumerable().Select( row => row.ToUpdateCommand() );

        
        
        /// <summary>
        /// Parse <see cref="DataRow"/>'s field to <see cref="DeleteCommand"/>.
        /// *.Use <see cref="DataRow.Table"/>'s TableName to set <see cref="DeleteCommand"/>'s table name.
        /// *.Use primary key to set where clause, if no primary key, use all column to set where clause.
        /// </summary>
        /// <param name="source"> source </param>
        public static DeleteCommand ToDeleteCommand( this DataRow source ) {
            if ( source == null ) return null;
                
            // get value object from source, add to command
            var whereParameters = new List<DBParameter>();
            var primaryKey = source.Table.PrimaryKey;
            foreach ( DataColumn column in source.Table.Columns ) {
                var valueObj = source[column];

                // if has primary key, check is this column is primary key, if so, add it to where clause.
                if ( primaryKey.Length != 0 && primaryKey.Contains( column ) )
                    whereParameters.Add( new DBParameter( column.ColumnName, valueObj ) );
                else // if has no primary key, all column should set to where clause.
                    whereParameters.Add( new DBParameter( column.ColumnName, valueObj ) );
            } // foreach
            
            // create command
            var result = new DeleteCommand()
                .DeleteFrom( new DBTable( source.Table.TableName ) )
                .WhereAnd( whereParameters );
            
            return result;
        } // public static DeleteCommand ToDeleteCommand( this DataRow source )
        
        /// <summary>
        /// Parse each row in <see cref="DataTable"/> to <see cref="DeleteCommand"/>.
        /// *.Use <see cref="DataTable.TableName"/> to set <see cref="DeleteCommand"/>'s table name.
        /// </summary>
        /// <param name="source"> source </param>
        public static IEnumerable<DeleteCommand> ToDeleteCommand( this DataTable source ) =>
            source.AsEnumerable().Select( row => row.ToDeleteCommand() );
        
        
        /// <summary>
        /// Parse <see cref="DataRow"/>'s field to <see cref="InsertCommand"/>, <see cref="UpdateCommand"/> or <see cref="DeleteCommand"/> by <see cref="DataRow.RowState"/>.
        /// *.Use <see cref="DataRow.Table"/>'s TableName to set Command's table name.
        /// *.Use primary key to set where clause, if no primary key, use all column to set where clause.
        /// </summary>
        /// <param name="source"> source </param>
        public static IDBCommand ToCommand( this DataRow source ) {
            if ( source == null ) return null;

            switch ( source.RowState ){
                case DataRowState.Added:
                    return source.ToInsertCommand();
                case DataRowState.Modified:
                    return source.ToUpdateCommand();
                case DataRowState.Deleted:
                    return source.ToDeleteCommand();
                default:
                    return null;
            } // switch
        } // public static IDBCommand ToCommand( this DataRow source )
        
        /// <summary>
        /// Parse <see cref="DataRow"/>'s field to <see cref="InsertCommand"/>, <see cref="UpdateCommand"/> or <see cref="DeleteCommand"/> by <see cref="DataRow.RowState"/>.
        /// *.Use <see cref="DataRow.Table"/>'s TableName to set Command's table name.
        /// *.Use primary key to set where clause, if no primary key, use all column to set where clause.
        /// </summary>
        /// <param name="source"> source </param>
        public static IEnumerable<IDBCommand> ToCommand( this DataTable source ) =>
            source.AsEnumerable().Select( row => row.ToCommand() ).Where( item => item != null );
        
        #endregion

        #region Extra to DBTable

        /// <summary> Get the DBTable name from DBTableAttribute from type. </summary>
        /// <param name="DBTableType">class type of table</param>
        /// <returns> Table name from DBTableAttribute or null. </returns>
        public static DBTable Table( Type DBTableType ) {
            /// Check arguments.
            if ( DBTableType.IsInterface )
                throw new ArgumentException( $"The type can't be an interface. {DBTableType.FullName}" );

            if ( !typeof( IDatabaseTable ).IsAssignableFrom( DBTableType ) )
                throw new ArgumentException( $"The type must implement IDatabaseTable. {DBTableType.FullName}" );

            return DBTableAttribute.GetDBTable( DBTableType );
        } // public static DBTable Table( Type DBTableType )

        /// <summary> Get the DBTable from DBTableAttribute from type. </summary>
        /// <returns> DBTable from DBTableAttribute or null. </returns>
        public static DBTable Table<T>()
            where T : IDatabaseTable => DBTableAttribute.GetDBTable( typeof( T ) );

        #endregion

        #region Extra to DBColumn

        /// <summary> Extra property <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="FilterType">Demetermine which property should be convert to <see cref="DBColumn"/>.</param>
        /// <param name="PropertyNames">Property name list. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        private static IEnumerable<DBColumn> ExtraColumnToDBColumn<T>( EFilterType FilterType = EFilterType.All,
            string[] PropertyNames = null, EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable {
            
            // extra member from type.
            var columnDictionary = DBColumnAttribute.GetCorrespondMember( typeof( T ) );
            var columnList = columnDictionary
                .SelectMany( column => column.Value.Select( item => new { Name = column.Key, Value = item } ) );

            // filter member by convert type.
            if ( ConvertType == EConvertType.NotDBColumn ) columnList = columnList.Empty();
            else if ( ConvertType != EConvertType.Twoway )
                columnList = columnList.Where( item => ( ConvertType & item.Value.ColumnAttribute.ConvertType ) != 0 );
            
            // filter by FilterType.
            if ( FilterType == EFilterType.ExcludeList )
                columnList = columnList.Where( item => !PropertyNames?.Contains( item.Value.Name ) ?? true );
            else if ( FilterType == EFilterType.IncludeList )
                columnList = columnList.Where( item => PropertyNames?.Contains( item.Value.Name ) ?? false );
            else if ( FilterType == EFilterType.NonPrimaryKey )
                columnList = columnList.Where( item => !item.Value.ColumnAttribute.PrimaryKey );
            else if ( FilterType == EFilterType.PrimaryKey )
                columnList = columnList.Where( item => item.Value.ColumnAttribute.PrimaryKey );

            // yield return result.
            foreach ( var item in columnList )
                yield return new DBColumn( item.Name, item.Value.ColumnAttribute.ColumnRule );
        } // private static IEnumerable<DBColumn> ExtraColumnToDBColumn<T>( EFilterType FilterType = EFilterType.All, ... )

        /// <summary> Extra primary key property <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> PrimaryKey<T>( EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable => ExtraColumnToDBColumn<T>( EFilterType.PrimaryKey, ConvertType: ConvertType );

        /// <summary> Extra non primary key property <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> NonPrimaryKey<T>( EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable => ExtraColumnToDBColumn<T>( EFilterType.NonPrimaryKey, ConvertType: ConvertType );

        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> Columns<T>( EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable => ExtraColumnToDBColumn<T>( EFilterType.ExcludeList, null, ConvertType: ConvertType );

        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="PropertyNames">Property name list to extra to DBColumn. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> Columns<T>( string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable {
            if ( ( PropertyNames?.Length ?? 0 ) == 0 )
                return ExtraColumnToDBColumn<T>( EFilterType.ExcludeList, null, ConvertType: ConvertType );
            else
                return ExtraColumnToDBColumn<T>( EFilterType.IncludeList, PropertyNames, ConvertType: ConvertType );
        } // public static IEnumerable<DBColumn> Columns<T>( string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway )
        
        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="PropertyNames">Property name list to extra to DBColumn. </param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> Columns<T>( params string[] PropertyNames )
            where T : IDatabaseTable {
            if ( ( PropertyNames?.Length ?? 0 ) == 0 )
                return ExtraColumnToDBColumn<T>( EFilterType.ExcludeList, null );
            else
                return ExtraColumnToDBColumn<T>( EFilterType.IncludeList, PropertyNames );
        } // public static IEnumerable<DBColumn> Columns<T>( params string[] PropertyNames )

        /// <summary> Extra propertys not in list <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="PropertyNames">Property name list to not extra to DBColumn. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> ColumnsExcept<T>( string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway )
            where T : IDatabaseTable => ExtraColumnToDBColumn<T>( EFilterType.ExcludeList, PropertyNames, ConvertType: ConvertType );

        /// <summary> Extra propertys not in list <see paramref="T"/> to <see cref="DBColumn"/>. </summary>
        /// <param name="PropertyNames">Property name list to not extra to DBColumn. </param>
        /// <returns>DBColumn list with specified condition.</returns>
        public static IEnumerable<DBColumn> ColumnsExcept<T>( params string[] PropertyNames )
            where T : IDatabaseTable => ExtraColumnToDBColumn<T>( EFilterType.ExcludeList, PropertyNames );

        #endregion

        #region Extra to DBParameter

        /// <summary> Extra property <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="FilterType">Demetermine which property should be convert to <see cref="DBParameter"/>.</param>
        /// <param name="PropertyNames">Property name list. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        private static IEnumerable<DBParameter> ExtraColumnToDBParameter( this IDatabaseTable source, EFilterType FilterType = EFilterType.All,
            string[] PropertyNames = null, EConvertType ConvertType = EConvertType.Twoway ) {
            
            // extra member from type.
            var columnDictionary = DBColumnAttribute.GetCorrespondMember( source.GetType() );
            var columnList = columnDictionary
                .SelectMany( column => column.Value.Select( item => new { Name = column.Key, Value = item } ) );

            // filter member by convert type.
            if ( ConvertType == EConvertType.NotDBColumn ) columnList = columnList.Empty();
            else if ( ConvertType != EConvertType.Twoway )
                columnList = columnList.Where( item => ( ConvertType & item.Value.ColumnAttribute.ConvertType ) != 0 );
            
            // filter by FilterType.
            if ( FilterType == EFilterType.ExcludeList )
                columnList = columnList.Where( item => !PropertyNames?.Contains( item.Value.Name ) ?? true );
            else if ( FilterType == EFilterType.IncludeList )
                columnList = columnList.Where( item => PropertyNames?.Contains( item.Value.Name ) ?? false );
            else if ( FilterType == EFilterType.NonPrimaryKey )
                columnList = columnList.Where( item => !item.Value.ColumnAttribute.PrimaryKey );
            else if ( FilterType == EFilterType.PrimaryKey )
                columnList = columnList.Where( item => item.Value.ColumnAttribute.PrimaryKey );

            // yield return result.
            foreach ( var item in columnList )
                yield return new DBParameter( item.Name, item.Value.GetValue( source ), item.Value.ColumnAttribute.ColumnRule );
        } // private static IEnumerable<DBParameter> ExtraColumnToDBParameter( this IDatabaseTable source, EFilterType FilterType = EFilterType.All, ... )

        /// <summary> Extra primary key property <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> PrimaryKey( this IDatabaseTable source, EConvertType ConvertType = EConvertType.Twoway ) => source.ExtraColumnToDBParameter( EFilterType.PrimaryKey, ConvertType: ConvertType );

        /// <summary> Extra non primary key property <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> NonPrimaryKey( this IDatabaseTable source, EConvertType ConvertType = EConvertType.Twoway ) => source.ExtraColumnToDBParameter( EFilterType.NonPrimaryKey, ConvertType: ConvertType );

        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> Columns( this IDatabaseTable source, EConvertType ConvertType = EConvertType.Twoway ) => source.ExtraColumnToDBParameter( EFilterType.ExcludeList, null, ConvertType: ConvertType );

        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="PropertyNames">Property name list to extra to DBParameter. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> Columns( this IDatabaseTable source, string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway ) {
            if ( ( PropertyNames?.Length ?? 0 ) == 0 )
                return source.ExtraColumnToDBParameter( EFilterType.ExcludeList, null, ConvertType: ConvertType );
            else
                return source.ExtraColumnToDBParameter( EFilterType.IncludeList, PropertyNames, ConvertType: ConvertType );
        } // public static IEnumerable<DBParameter> Columns( this IDatabaseTable source, string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway )
        
        /// <summary> Extra propertys in list <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="PropertyNames">Property name list to extra to DBParameter. </param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> Columns( this IDatabaseTable source, params string[] PropertyNames ) {
            if ( ( PropertyNames?.Length ?? 0 ) == 0 )
                return source.ExtraColumnToDBParameter( EFilterType.ExcludeList, null );
            else
                return source.ExtraColumnToDBParameter( EFilterType.IncludeList, PropertyNames );
        } // public static IEnumerable<DBParameter> Columns( this IDatabaseTable source, params string[] PropertyNames )

        /// <summary> Extra propertys not in list <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="PropertyNames">Property name list to not extra to DBParameter. </param>
        /// <param name="ConvertType">Filter property by it's <see cref="DBColumnAttribute.ConvertType"/>.</param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> ColumnsExcept( this IDatabaseTable source, string[] PropertyNames, EConvertType ConvertType = EConvertType.Twoway ) => source.ExtraColumnToDBParameter( EFilterType.ExcludeList, PropertyNames, ConvertType: ConvertType );

        /// <summary> Extra propertys not in list <see paramref="T"/> to <see cref="DBParameter"/>. </summary>
        /// <param name="source">source.</param>
        /// <param name="PropertyNames">Property name list to not extra to DBParameter. </param>
        /// <returns>DBParameter list with specified condition.</returns>
        public static IEnumerable<DBParameter> ColumnsExcept( this IDatabaseTable source, params string[] PropertyNames ) => source.ExtraColumnToDBParameter( EFilterType.ExcludeList, PropertyNames );

        #endregion

    } // public static class IDatabaseTableExtension

} // namespace PGLibrary.Database
