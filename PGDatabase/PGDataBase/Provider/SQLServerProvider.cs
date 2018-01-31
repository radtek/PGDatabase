using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using PGCafe;
using PGCafe.Object;
using System.Linq;

namespace PGLibrary.Database {
    /// <summary> proviod somethign to use in DBSpool by SQLServer Database. </summary>
    public class SQLServerProvider : IDatabaseProvider {
        /// <summary> Database's type. </summary>
        public EDatabaseType DatabaseType => EDatabaseType.SQLServer;
        
        /// <summary> Quote prefix of keyword. </summary>
        public string QuotePrefix => "[";
        
        /// <summary> Quote suffix of keyword. </summary>
        public string QuoteSuffix => "]";
        
        /// <summary> Spliter of multiple command or end of command. </summary>
        public string CommandSpliter { get; } = ";";
        
        #region Reserved Words

        /// <summary> reserved words of version SQL Server 2012. </summary>
        private HashSet<string> mReservedWords = null;
        
        /// <summary> Reserved words of version SQL Server 2012. </summary>
        public HashSet<string> ReservedWords {
            get {
                if ( this.mReservedWords == null ) {
                    this.mReservedWords = new HashSet<string>(
                        new string[] { "ADD", "EXTERNAL", "PROCEDURE", "ALL", "FETCH", "PUBLIC", "ALTER", "FILE", "RAISERROR", "AND",
                            "FILLFACTOR", "READ", "ANY", "FOR", "READTEXT", "AS", "FOREIGN", "RECONFIGURE", "ASC", "FREETEXT", "REFERENCES",
                            "AUTHORIZATION", "FREETEXTTABLE", "REPLICATION", "BACKUP", "FROM", "RESTORE", "BEGIN", "FULL", "RESTRICT",
                            "BETWEEN", "FUNCTION", "RETURN", "BREAK", "GOTO", "REVERT", "BROWSE", "GRANT", "REVOKE", "BULK", "GROUP",
                            "RIGHT", "BY", "HAVING", "ROLLBACK", "CASCADE", "HOLDLOCK", "ROWCOUNT", "CASE", "IDENTITY", "ROWGUIDCOL",
                            "CHECK", "IDENTITY_INSERT", "RULE", "CHECKPOINT", "IDENTITYCOL", "SAVE", "CLOSE", "IF", "SCHEMA", "CLUSTERED",
                            "IN", "SECURITYAUDIT", "COALESCE", "INDEX", "SELECT", "COLLATE", "INNER", "SEMANTICKEYPHRASETABLE", "COLUMN",
                            "INSERT", "SEMANTICSIMILARITYDETAILSTABLE", "COMMIT", "INTERSECT", "SEMANTICSIMILARITYTABLE", "COMPUTE", "INTO",
                            "SESSION_USER", "CONSTRAINT", "IS", "SET", "CONTAINS", "JOIN", "SETUSER", "CONTAINSTABLE", "KEY", "SHUTDOWN",
                            "CONTINUE", "KILL", "SOME", "CONVERT", "LEFT", "STATISTICS", "CREATE", "LIKE", "SYSTEM_USER", "CROSS", "LINENO",
                            "TABLE", "CURRENT", "LOAD", "TABLESAMPLE", "CURRENT_DATE", "MERGE", "TEXTSIZE", "CURRENT_TIME", "NATIONAL", "THEN",
                            "CURRENT_TIMESTAMP", "NOCHECK", "TO", "CURRENT_USER", "NONCLUSTERED", "TOP", "CURSOR", "NOT", "TRAN", "DATABASE",
                            "NULL", "TRANSACTION", "DBCC", "NULLIF", "TRIGGER", "DEALLOCATE", "OF", "TRUNCATE", "DECLARE", "OFF", "TRY_CONVERT",
                            "DEFAULT", "OFFSETS", "TSEQUAL", "DELETE", "ON", "UNION", "DENY", "OPEN", "UNIQUE", "DESC", "OPENDATASOURCE", "UNPIVOT",
                            "DISK", "OPENQUERY", "UPDATE", "DISTINCT", "OPENROWSET", "UPDATETEXT", "DISTRIBUTED", "OPENXML", "USE", "DOUBLE",
                            "OPTION", "USER", "DROP", "OR", "VALUES", "DUMP", "ORDER", "VARYING", "ELSE", "OUTER", "VIEW", "END", "OVER", "WAITFOR",
                            "ERRLVL", "PERCENT", "WHEN", "ESCAPE", "PIVOT", "WHERE", "EXCEPT", "PLAN", "WHILE", "EXEC", "PRECISION", "WITH", "EXECUTE",
                            "PRIMARY", "WITHIN GROUP", "EXISTS", "PRINT", "WRITETEXT", "EXIT", "PROC", },
                        StringComparer.OrdinalIgnoreCase );
                } // if

                return this.mReservedWords;
            } // get
        } // public HashSet<string> ReservedWords

        #endregion

        #region Create Object method

        /// <summary> Just create connection object and wihout any setting of property. </summary>
        /// <returns> Connection object of specific DB type. </returns>
        public DbConnection CreateConnectionObject() {
            return new SqlConnection();
        } // public DbConnection CreateConnectionObject()

        /// <summary> Just create command object and wihout any setting of property. </summary>
        /// <returns> Command object of specific DB type. </returns>
        public DbCommand CreateCommandObject() {
            return new SqlCommand();
        } // public DbCommand CreateCommandObject()

        /// <summary> Just create CommandBuilder object and need to set QuotePrefix and QuoteSuffix property. </summary>
        /// <returns> CommandBuilder object of specific DB type. </returns>
        public DbCommandBuilder CreateCommandBuilder() {
            return new SqlCommandBuilder();
        } // public DbCommandBuilder CreateCommandBuilder()

        /// <summary> Just create DataAdapter object and wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specific DB type. </returns>
        public DbDataAdapter CreateDataAdapter() {
            return new SqlDataAdapter();
        } // public DbDataAdapter CreateDataAdapter()

        #endregion
        
        #region General method

        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="ColumnRule"> Column rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        public string ToDBValueString( object value, EColumnRule ColumnRule = EColumnRule.None ) {
            if ( value == null ) return "NULL";

            else if ( value is DBNull )
                return "NULL";

            else if ( value is string || value is char ) {
                if ( ColumnRule.HasFlag( EColumnRule.UseVarCharType ) ) {
                    return $"'{value.ToString().Replace( "'", "''" )}'";
                } // if
                else {
                    return $"N'{value.ToString().Replace( "'", "''" )}'";
                } // else
            } // else if
            
            else if ( value is Enum ) {
                if ( ColumnRule.HasFlag( EColumnRule.EnumToString ) ) {
                    var valueString = value.ToString();
                    if ( ColumnRule.HasFlag( EColumnRule.UseVarCharType ) ) {
                        return $"'{valueString.Replace( "'", "''" )}'";
                    } // if
                    else {
                        return $"N'{valueString.Replace( "'", "''" )}'";
                    } // else
                } // else if
            
                else if ( value is Enum && ColumnRule.HasFlag( EColumnRule.EnumDescription ) ) {
                    var valueString = value.CastTo<Enum>().Description();
                    if ( ColumnRule.HasFlag( EColumnRule.UseVarCharType ) ) {
                        return $"'{valueString.Replace( "'", "''" )}'";
                    } // if
                    else {
                        return $"N'{valueString.Replace( "'", "''" )}'";
                    } // else
                } // else if
            
                else if ( value is Enum && ColumnRule.HasFlag( EColumnRule.EnumStringValue ) ) {
                    var valueString = value.CastTo<Enum>().StringValue();
                    if ( ColumnRule.HasFlag( EColumnRule.UseVarCharType ) ) {
                        return $"'{valueString.Replace( "'", "''" )}'";
                    } // if
                    else {
                        return $"N'{valueString.Replace( "'", "''" )}'";
                    } // else
                } // else if

                else
                    return value.ToType<int>().ToString();

            } // else if

            else if ( value is bool )
                return value.CastTo<bool>() ? "1" : "0";

            else if ( value is DateTime )
                return $"'{value.CastTo<DateTime>():yyyy/MM/dd HH:mm:ss.fff}'";
            
            else return value.ToString();
        } // public string ValueToDBString( object value, EColumnRule SpecialRule = EColumnRule.None )

        /// <summary> Quote the word by QuoteType. </summary>
        /// <param name="Word"> the word to quote. </param>
        /// <param name="QuoteType"> how to quote the word. </param>
        /// <returns> the word after quote. </returns>
        public string Quote( string Word, EQuoteType QuoteType = EQuoteType.Always ) {
            if ( QuoteType == EQuoteType.Always ) return $"{this.QuotePrefix}{Word}{this.QuoteSuffix}";
            else if ( QuoteType == EQuoteType.Auto && this.ReservedWords.Contains( Word ) ) return $"{this.QuotePrefix}{Word}{this.QuoteSuffix}";
            else return Word;
        } // public string Quote( string Word, EQuoteType QuoteType = EQuoteType.Always )
        
        #endregion
        
        #region Select Schema

        #region implement
        
        /// <summary> Select all table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException ) {
            var result = this.SelectSchema( DBObject, timeout, throwException );
            return new SingleResult<IEnumerable<ITableSchema>>( result.Success, result.Exception, result.Value );
        } // SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException )
        
        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="TableNames"> specific table names to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string[] TableNames, int? timeout, bool? throwException ) {
            var result = this.SelectSchema( DBObject, TableNames, timeout, throwException );
            return new SingleResult<IEnumerable<ITableSchema>>( result.Success, result.Exception, result.Value );
        } // SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException )
        
        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="TableName"> specific table name to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string TableName, int? timeout, bool? throwException ) {
            var result = this.SelectSchema( DBObject, TableName, timeout, throwException );
            return new SingleResult<IEnumerable<ITableSchema>>( result.Success, result.Exception, result.Value );
        } // SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException )

        #endregion

        
        /// <summary> Select all table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, int? timeout = null, bool? throwException = null ) {
            // select table and column schema from db
            DBCommand cmd = new DBCommand( @"
                  Select A.TABLE_SCHEMA, A.TABLE_NAME, A.COLUMN_NAME, A.IS_NULLABLE, A.DATA_TYPE, 
	                     A.CHARACTER_MAXIMUM_LENGTH, A.NUMERIC_PRECISION, A.NUMERIC_SCALE,
                         B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                    From INFORMATION_SCHEMA.COLUMNS A
                    Left Join INFORMATION_SCHEMA.KEY_COLUMN_USAGE B
                      On A.TABLE_CATALOG = B.TABLE_CATALOG
                     And A.TABLE_SCHEMA = B.TABLE_SCHEMA
                     And A.TABLE_NAME = B.TABLE_NAME
                     And A.COLUMN_NAME = B.COLUMN_NAME" );
            
            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, int? timeout = null, bool? throwException = null )
        

        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="Table"> specific tables to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, string[] Table, int? timeout = null, bool? throwException = null ) {
            // select table and column schema from db
            var tablesInScrip = string.Join( ", ", Table.Select( item => $"'{item}'" ) );
            DBCommand cmd = new DBCommand( $@"
                  Select A.TABLE_SCHEMA, A.TABLE_NAME, A.COLUMN_NAME, A.IS_NULLABLE, A.DATA_TYPE, 
	                     A.CHARACTER_MAXIMUM_LENGTH, A.NUMERIC_PRECISION, A.NUMERIC_SCALE,
                         B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                    From INFORMATION_SCHEMA.COLUMNS A
                    Left Join INFORMATION_SCHEMA.KEY_COLUMN_USAGE B
                      On A.TABLE_CATALOG = B.TABLE_CATALOG
                     And A.TABLE_SCHEMA = B.TABLE_SCHEMA
                     And A.TABLE_NAME = B.TABLE_NAME
                     And A.COLUMN_NAME = B.COLUMN_NAME
                   Where A.TABLE_NAME In ( {tablesInScrip} )" );
            
            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, string[] Table, bool? throwException = null )
        

        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="Table"> specific table to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, string Table, int? timeout = null, bool? throwException = null ) {
            // select specific table and column schema from db
            DBCommand cmd = new DBCommand( @"
                  Select A.TABLE_SCHEMA, A.TABLE_NAME, A.COLUMN_NAME, A.IS_NULLABLE, A.DATA_TYPE, 
	                     A.CHARACTER_MAXIMUM_LENGTH, A.NUMERIC_PRECISION, A.NUMERIC_SCALE,
                         B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                    From INFORMATION_SCHEMA.COLUMNS A
                    Left Join INFORMATION_SCHEMA.KEY_COLUMN_USAGE B
                      On A.TABLE_CATALOG = B.TABLE_CATALOG
                     And A.TABLE_SCHEMA = B.TABLE_SCHEMA
                     And A.TABLE_NAME = B.TABLE_NAME
                     And A.COLUMN_NAME = B.COLUMN_NAME
                   Where A.TABLE_NAME = :Table" )
                    .AddParameter( "Table", Table );

            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<SQLServerTableSchema>> SelectSchema( PGDatabase DBObject, string Table, int? timeout = null, bool? throwException = null )
        
        
        private SingleResult<IEnumerable<SQLServerTableSchema>> QueryTableSchema( PGDatabase DBObject, IDBCommand command, int? timeout, bool? throwException ) {
            
            // if query failed, return exception.
            var queryResult = DBObject.ExecuteDataTable( command, timeout, throwException );
            if ( !queryResult.Success ) return queryResult.Exception;

            var result = new List<SQLServerTableSchema>();
            var groupByTableName = queryResult.Value.AsEnumerable()
                .GroupBy( item => new { Schema = item["TABLE_SCHEMA"].ToType<string>(), Table = item["TABLE_NAME"].ToType<string>() } );

            foreach ( var table in groupByTableName ) {
                // foreach table, create columns and table object.
                var newTable = new SQLServerTableSchema( table.Key.Schema, table.Key.Table );
                newTable.Columns.AddRange( table.Select( row => ColumnSchemaFromDataRow( row ) ) );

                var primaryKeyName = table.FirstOrDefault( row => !row.IsNull( "PRIMARYKEY_NAME" ) )?["PRIMARYKEY_NAME"].ToString();
                newTable.PrimaryKeyName = primaryKeyName;

                result.Add( newTable );
            } // foreach
            
            // convert DataTable to Schema object and return.
            return new SingleResult<IEnumerable<SQLServerTableSchema>>( result );
        } // private SingleResult<IEnumerable<SQLServerTableSchema>> QueryTableSchema( PGDatabase DBObject, IDBCommand command, int? timeout, bool? throwException )


        /// <summary> get <see cref="SQLServerColumnSchema"/> from datarow contains schema data. </summary>
        /// <param name="columnData">The column data.</param>
        private SQLServerColumnSchema ColumnSchemaFromDataRow( DataRow columnData ) {
            // create column schema
            var name = columnData["COLUMN_NAME"].ToType<string>();
            var isNullable = columnData["IS_NULLABLE"].ToType<string>().EqualsIgnoreCase( "YES" );
            var datatype = columnData["DATA_TYPE"].ToType<string>();
            var characterLength = columnData["CHARACTER_MAXIMUM_LENGTH"].ToType<int?>();
            var numericPrecision = columnData["NUMERIC_PRECISION"].ToType<int?>();
            var numericScale = columnData["NUMERIC_SCALE"].ToType<int?>();
            var isPrimaryKey = !columnData.IsNull( "PRIMARYKEY_NAME" );

            var newColumn = SQLServerColumnSchema.PowerCreator( name, isNullable, datatype,
                characterLength, numericPrecision, numericScale );

            newColumn.IsPrimaryKey = isPrimaryKey;

            return newColumn;
        } // private SQLServerColumnSchema ColumnSchemaFromDataRow( DataTable columnData )

        #endregion

    } // public class SQLServerProvider : IDatabaseProvider
} // namespace PGLibrary.Database
