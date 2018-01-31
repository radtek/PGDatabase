using System;
using System.Collections.Generic;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using PGCafe;
using PGCafe.Object;
using System.Linq;
using System.Data;

namespace PGLibrary.Database {
    /// <summary> proviod somethign to use in DBSpool by Oracle Database. </summary>
    public class OracleProvider : IDatabaseProvider {
        /// <summary> Database's type. </summary>
        public EDatabaseType DatabaseType => EDatabaseType.Oracle;
        
        /// <summary> Quote prefix of keyword. </summary>
        public string QuotePrefix => "\"";
        
        /// <summary> Quote suffix of keyword. </summary>
        public string QuoteSuffix => "\"";
        
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
                        new string[] { "ACCESS","ACCOUNT","ACTIVATE","ADD","ADMIN","ADVISE","AFTER","ALL","ALL_ROWS","ALLOCATE","ALTER","ANALYZE","AND",
                            "ANY","ARCHIVE","ARCHIVELOG","ARRAY","AS","ASC","AT","AUDIT","AUTHENTICATED","AUTHORIZATION","AUTOEXTEND","AUTOMATIC","BACKUP",
                            "BECOME","BEFORE","BEGIN","BETWEEN","BFILE","BITMAP","BLOB","BLOCK","BODY","BY","CACHE","CACHE_INSTANCES","CANCEL","CASCADE",
                            "CAST","CFILE","CHAINED","CHANGE","CHAR","CHAR_CS","CHARACTER","CHECK","CHECKPOINT","CHOOSE","CHUNK","CLEAR","CLOB","CLONE",
                            "CLOSE","CLOSE_CACHED_OPEN_CURSORS","CLUSTER","COALESCE","COLUMN","COLUMNS","COMMENT","COMMIT","COMMITTED","COMPATIBILITY",
                            "COMPILE","COMPLETE","COMPOSITE_LIMIT","COMPRESS","COMPUTE","CONNECT","CONNECT_TIME","CONSTRAINT","CONSTRAINTS","CONTENTS",
                            "CONTINUE","CONTROLFILE","CONVERT","COST","CPU_PER_CALL","CPU_PER_SESSION","CREATE","CURREN_USER","CURRENT","CURRENT_SCHEMA",
                            "CURSOR","CYCLE","DANGLING","DATABASE","DATAFILE","DATAFILES","DATAOBJNO","DATE","DBA","DBHIGH","DBLOW","DBMAC","DEALLOCATE",
                            "DEBUG","DEC","DECIMAL","DECLARE","DEFAULT","DEFERRABLE","DEFERRED","DEGREE","DELETE","DEREF","DESC","DIRECTORY","DISABLE",
                            "DISCONNECT","DISMOUNT","DISTINCT","DISTRIBUTED","DML","DOUBLE","DROP","DUMP","EACH","ELSE","ENABLE","END","ENFORCE","ENTRY",
                            "ESCAPE","EXCEPT","EXCEPTIONS","EXCHANGE","EXCLUDING","EXCLUSIVE","EXECUTE","EXISTS","EXPIRE","EXPLAIN","EXTENT","EXTENTS",
                            "EXTERNALLY","FAILED_LOGIN_ATTEMPTS","FAST","FILE","FIRST_ROWS","FLAGGER","FLOAT","FLOB","FLUSH","FOR","FORCE","FOREIGN","FREELIST",
                            "FREELISTS","FROM","FULL","FUNCTION","GLOBAL","GLOBAL_NAME","GLOBALLY","GRANT","GROUP","GROUPS","HASH","HASHKEYS","HAVING",
                            "HEADER","HEAP","IDENTIFIED","IDGENERATORS","IDLE_TIME","IF","IMMEDIATE","IN","INCLUDING","INCREMENT","IND_PARTITION","INDEX",
                            "INDEXED","INDEXES","INDICATOR","INITIAL","INITIALLY","INITRANS","INSERT","INSTANCE","INSTANCES","INSTEAD","INT","INTEGER",
                            "INTERMEDIATE","INTERSECT","INTO","IS","ISOLATION","ISOLATION_LEVEL","KEEP","KEY","KILL","LABEL","LAYER","LESS","LEVEL","LIBRARY",
                            "LIKE","LIMIT","LINK","LIST","LOB","LOCAL","LOCK","LOCKED","LOG","LOGFILE","LOGGING","LOGICAL_READS_PER_CALL",
                            "LOGICAL_READS_PER_SESSION","LONG","MANAGE","MASTER","MAX","MAXARCHLOGS","MAXDATAFILES","MAXEXTENTS","MAXINSTANCES","MAXLOGFILES",
                            "MAXLOGHISTORY","MAXLOGMEMBERS","MAXSIZE","MAXTRANS","MAXVALUE","MEMBER","MIN","MINEXTENTS","MINIMUM","MINUS","MINVALUE",
                            "MLS_LABEL_FORMAT","MLSLABEL","MODE","MODIFY","MOUNT","MOVE","MTS_DISPATCHERS","MULTISET","NATIONAL","NCHAR","NCHAR_CS","NCLOB",
                            "NEEDED","NESTED","NETWORK","NEW","NEXT","NOARCHIVELOG","NOAUDIT","NOCACHE","NOCOMPRESS","NOCYCLE","NOFORCE","NOLOGGING",
                            "NOMAXVALUE","NOMINVALUE","NONE","NOORDER","NOOVERRIDE","NOPARALLEL","NOPARALLEL","NOREVERSE","NORMAL","NOSORT","NOT","NOTHING",
                            "NOWAIT","NULL","NUMBER","NUMERIC","NVARCHAR2","OBJECT","OBJNO","OBJNO_REUSE","OF","OFF","OFFLINE","OID","OIDINDEX","OLD","ON",
                            "ONLINE","ONLY","OPCODE","OPEN","OPTIMAL","OPTIMIZER_GOAL","OPTION","OR","ORDER","ORGANIZATION","OSLABEL","OVERFLOW","OWN","PACKAGE",
                            "PARALLEL","PARTITION","PASSWORD","PASSWORD_GRACE_TIME","PASSWORD_LIFE_TIME","PASSWORD_LOCK_TIME","PASSWORD_REUSE_MAX",
                            "PASSWORD_REUSE_TIME","PASSWORD_VERIFY_FUNCTION","PCTFREE","PCTINCREASE","PCTTHRESHOLD","PCTUSED","PCTVERSION","PERCENT","PERMANENT",
                            "PLAN","PLSQL_DEBUG","POST_TRANSACTION","PRECISION","PRESERVE","PRIMARY","PRIOR","PRIVATE","PRIVATE_SGA","PRIVILEGE","PRIVILEGES",
                            "PROCEDURE","PROFILE","PUBLIC","PURGE","QUEUE","QUOTA","RANGE","RAW","RBA","READ","READUP","REAL","REBUILD","RECOVER","RECOVERABLE",
                            "RECOVERY","REF","REFERENCES","REFERENCING","REFRESH","RENAME","REPLACE","RESET","RESETLOGS","RESIZE","RESOURCE","RESTRICTED",
                            "RETURN","RETURNING","REUSE","REVERSE","REVOKE","ROLE","ROLES","ROLLBACK","ROW","ROWID","ROWNUM","ROWS","RULE","SAMPLE","SAVEPOINT",
                            "SB4","SCAN_INSTANCES","SCHEMA","SCN","SCOPE","SD_ALL","SD_INHIBIT","SD_SHOW","SEG_BLOCK","SEG_FILE","SEGMENT","SELECT","SEQUENCE",
                            "SERIALIZABLE","SESSION","SESSION_CACHED_CURSORS","SESSIONS_PER_USER","SET","SHARE","SHARED","SHARED_POOL","SHRINK","SIZE","SKIP",
                            "SKIP_UNUSABLE_INDEXES","SMALLINT","SNAPSHOT","SOME","SORT","SPECIFICATION","SPLIT","SQL_TRACE","STANDBY","START","STATEMENT_ID",
                            "STATISTICS","STOP","STORAGE","STORE","STRUCTURE","SUCCESSFUL","SWITCH","SYNONYM","SYS_OP_ENFORCE_NOT_NULL$","SYS_OP_NTCIMG$",
                            "SYSDATE","SYSDBA","SYSOPER","SYSTEM","TABLE","TABLES","TABLESPACE","TABLESPACE_NO","TABNO","TEMPORARY","THAN","THE","THEN","THREAD",
                            "TIME","TIMESTAMP","TO","TOPLEVEL","TRACE","TRACING","TRANSACTION","TRANSITIONAL","TRIGGER","TRIGGERS","TRUNCATE","TX","TYPE","UB2",
                            "UBA","UID","UNARCHIVED","UNDO","UNION","UNIQUE","UNLIMITED","UNLOCK","UNRECOVERABLE","UNTIL","UNUSABLE","UNUSED","UPDATABLE","UPDATE",
                            "USAGE","USE","USER","USING","VALIDATE","VALIDATION","VALUE","VALUES","VARCHAR","VARCHAR2","VARYING","VIEW","WHEN","WHENEVER","WHERE",
                            "WITH","WITHOUT","WORK","WRITE","WRITEDOWN","WRITEUP","XID","YEAR","ZONE","FALSE","TRUE",
                        },
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
            return new OracleConnection();
        } // public DbConnection CreateConnectionObject()

        /// <summary> Just create command object and wihout any setting of property. </summary>
        /// <returns> Command object of specific DB type. </returns>
        public DbCommand CreateCommandObject() {
            return new OracleCommand();
        } // public DbCommand CreateCommandObject()

        /// <summary> Just create CommandBuilder object and need to set QuotePrefix and QuoteSuffix property. </summary>
        /// <returns> CommandBuilder object of specific DB type. </returns>
        public DbCommandBuilder CreateCommandBuilder() {
            return new OracleCommandBuilder();
        } // public DbCommandBuilder CreateCommandBuilder()

        /// <summary> Just create DataAdapter object and wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specific DB type. </returns>
        public DbDataAdapter CreateDataAdapter() {
            return new OracleDataAdapter();
        } // public DbDataAdapter CreateDataAdapter()
        
        #endregion

        #region General method

        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="ColumnRule"> Column rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        public string ToDBValueString( object value, EColumnRule ColumnRule ) {
            if ( value == null ) return "NULL";

            else if ( value is DBNull )
                return "NULL";
            
            else if ( value is string || value is char )
                return $"'{value.ToString().Replace( "'", "''" )}'";
            
            else if ( value is Enum ) {
                if ( ColumnRule.HasFlag( EColumnRule.EnumToString ) )
                    return $"'{value.ToString().Replace( "'", "''" )}'";
            
                else if ( ColumnRule.HasFlag( EColumnRule.EnumDescription ) )
                    return $"'{value.CastTo<Enum>().Description().Replace( "'", "''" )}'";
            
                else if ( ColumnRule.HasFlag( EColumnRule.EnumStringValue ) )
                    return $"'{value.CastTo<Enum>().StringValue().Replace( "'", "''" )}'";

                else
                    return value.ToType<int>().ToString();

            } // else if

            else if ( value is DateTime )
                return $"TO_DATE('{value.CastTo<DateTime>():yyyy/MM/dd HH:mm:ss}', 'YYYY/MM/DD HH24:MI:SS')";

            else if ( value is bool )
                return value.CastTo<bool>() ? "1" : "0";
            
            else return value.ToString();
        } // public string ValueToDBString( object value, EColumnRule SpecialRule )

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
        public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, int? timeout = null, bool? throwException = null ) {
            // select table and column schema from db
            DBCommand cmd = new DBCommand( @"
                With Constraint_Tmp As (
                    Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_NAME
                      From all_cons_columns A
                     Inner Join all_constraints B  On B.CONSTRAINT_TYPE = 'P' And B.OWNER = A.OWNER And B.CONSTRAINT_NAME = A.CONSTRAINT_NAME
                )
                Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, A.DATA_TYPE, A.NULLABLE, A.CHAR_LENGTH, A.DATA_PRECISION, A.DATA_SCALE,
                       B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                  From all_tab_columns A
                  Left Join Constraint_Tmp B On A.OWNER = B.OWNER And A.TABLE_NAME = B.TABLE_NAME And A.COLUMN_NAME = B.COLUMN_NAME
                 Order By A.TABLE_NAME, A.COLUMN_ID" );
            
            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, int? timeout = null, bool? throwException = null )
        

        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="Table"> specific tables to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, string[] Table, int? timeout = null, bool? throwException = null ) {
            // select table and column schema from db
            var tablesInScrip = string.Join( ", ", Table.Select( item => $"'{item}'" ) );

            DBCommand cmd = new DBCommand( $@"
                With Constraint_Tmp As (
                    Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_NAME
                      From all_cons_columns A
                     Inner Join all_constraints B  On B.CONSTRAINT_TYPE = 'P' And B.OWNER = A.OWNER And B.CONSTRAINT_NAME = A.CONSTRAINT_NAME
                )
                Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, A.DATA_TYPE, A.NULLABLE, A.CHAR_LENGTH, A.DATA_PRECISION, A.DATA_SCALE,
                       B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                  From all_tab_columns A
                  Left Join Constraint_Tmp B On A.OWNER = B.OWNER And A.TABLE_NAME = B.TABLE_NAME And A.COLUMN_NAME = B.COLUMN_NAME
                 Where A.TABLE_NAME In ( {tablesInScrip} )
                 Order By A.TABLE_NAME, A.COLUMN_ID" );
            
            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, string[] Table, int? timeout = null, bool? throwException = null )
        

        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="Table"> specific table to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, string Table, int? timeout = null, bool? throwException = null ) {
            // select specific table and column schema from db
            DBCommand cmd = new DBCommand( @"
                With Constraint_Tmp As (
                    Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, B.CONSTRAINT_NAME
                      From all_cons_columns A
                     Inner Join all_constraints B  On B.CONSTRAINT_TYPE = 'P' And B.OWNER = A.OWNER And B.CONSTRAINT_NAME = A.CONSTRAINT_NAME
                )
                Select A.OWNER, A.TABLE_NAME, A.COLUMN_NAME, A.DATA_TYPE, A.NULLABLE, A.CHAR_LENGTH, A.DATA_PRECISION, A.DATA_SCALE,
                       B.CONSTRAINT_NAME As PRIMARYKEY_NAME
                  From all_tab_columns A
                  Left Join Constraint_Tmp B On A.OWNER = B.OWNER And A.TABLE_NAME = B.TABLE_NAME And A.COLUMN_NAME = B.COLUMN_NAME
                 Where A.TABLE_NAME = :TableName
                 Order By A.COLUMN_ID" )
                 .AddParameter( "TableName", Table );
            
            return QueryTableSchema( DBObject, cmd, timeout, throwException );
        } // public SingleResult<IEnumerable<OracleTableSchema>> SelectSchema( PGDatabase DBObject, string Table, int? timeout = null, bool? throwException = null )
        
        
        private SingleResult<IEnumerable<OracleTableSchema>> QueryTableSchema( PGDatabase DBObject, IDBCommand command, int? timeout, bool? throwException ) {
            
            // if query failed, return exception.
            var queryResult = DBObject.ExecuteDataTable( command, timeout, throwException );
            if ( !queryResult.Success ) return queryResult.Exception;

            var groupByTableName = queryResult.Value.AsEnumerable()
                .GroupBy( item => new { Schema = item["OWNER"].ToType<string>(), Table = item["TABLE_NAME"].ToType<string>() } );

            var result = new List<OracleTableSchema>();
            foreach ( var table in groupByTableName ) {
                // foreach table, create columns and table object.
                var newTable = new OracleTableSchema( table.Key.Schema, table.Key.Table );
                newTable.Columns.AddRange( table.Select( row => ColumnSchemaFromDataRow( row ) ) );
                
                var primaryKeyName = table.FirstOrDefault( row => !row.IsNull( "PRIMARYKEY_NAME" ) )?["PRIMARYKEY_NAME"].ToString();
                newTable.PrimaryKeyName = primaryKeyName;

                result.Add( newTable );
            } // foreach
            
            // convert DataTable to Schema object and return.
            return new SingleResult<IEnumerable<OracleTableSchema>>( result );
        } // private SingleResult<IEnumerable<OracleTableSchema>> QueryTableSchema( PGDatabase DBObject, IDBCommand command, int? timeout, bool? throwException )


        /// <summary> get <see cref="OracleColumnSchema"/> from datarow contains schema data. </summary>
        /// <param name="columnData">The column data.</param>
        private OracleColumnSchema ColumnSchemaFromDataRow( DataRow columnData ){
            // create column schema
            var name = columnData["COLUMN_NAME"].ToType<string>();
            var isNullable = columnData["NULLABLE"].ToType<string>().EqualsIgnoreCase( "Y" );
            var datatype = columnData["DATA_TYPE"].ToType<string>();
            var charLength = columnData["CHAR_LENGTH"].ToType<int?>();
            var dataPrecision = columnData["DATA_PRECISION"].ToType<int?>();
            var dataScale = columnData["DATA_SCALE"].ToType<int?>();
            var isPrimaryKey = !columnData.IsNull( "PRIMARYKEY_NAME" );

            var newColumn = OracleColumnSchema.PowerCreator( name, isNullable, datatype,
                charLength, dataPrecision, dataScale );
            
            newColumn.IsPrimaryKey = isPrimaryKey;

            return newColumn;
        } // private OracleColumnSchema ColumnSchemaFromDataRow( DataTable columnData )

        #endregion

    } // public class OracleProvider : IDatabaseProvider
} // namespace PGLibrary.Database
