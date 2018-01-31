using System;
using System.Collections.Generic;
using System.Data.Common;
using PGCafe;
using IBM.Data.DB2;
using PGLibrary.Database;
using PGCafe.Object;

namespace MayaFramework {
    /// <summary> proviod somethign to use in DBSpool by DB2 DataBase. </summary>
    public class DB2Provider : IDatabaseProvider {
        /// <summary> DataBase's type. </summary>
        public EDatabaseType DatabaseType => EDatabaseType.DB2;

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
                        new[] { "ABS", "ACTIVATE", "ADD", "AFTER", "ALIAS", "ALL", "ALLOCATE", "ALLOW", "ALTER", "AND", "ANY",
                            "ARE", "ARRAY", "AS", "ASENSITIVE", "ASSOCIATE", "ASUTIME", "ASYMMETRIC", "AT", "ATOMIC", "ATTRIBUTES",
                            "AUDIT", "AUTHORIZATION", "AUX", "AUXILIARY", "AVG", "BEFORE", "BEGIN", "BETWEEN", "BIGINT", "BINARY",
                            "BLOB", "BOOLEAN", "BOTH", "BUFFERPOOL", "BY", "CACHE", "CALL", "CALLED", "CAPTURE", "CARDINALITY",
                            "CASCADED", "CASE", "CAST", "CCSID", "CEIL", "CEILING", "CHAR", "CHAR_LENGTH", "CHARACTER",
                            "CHARACTER_LENGTH", "CHECK", "CLOB", "CLONE", "CLOSE", "CLUSTER", "COALESCE", "COLLATE", "COLLECT",
                            "COLLECTION", "COLLID", "COLUMN", "COMMENT", "COMMIT", "CONCAT", "CONDITION", "CONNECT", "CONNECTION",
                            "CONSTRAINT", "CONTAINS", "CONTINUE", "CONVERT", "CORR", "CORRESPONDING", "COUNT", "COUNT_BIG",
                            "COVAR_POP", "COVAR_SAMP", "CREATE", "CROSS", "CUBE", "CUME_DIST", "CURRENT", "CURRENT_DATE",
                            "CURRENT_DEFAULT_TRANSFORM_GROUP", "CURRENT_LC_CTYPE", "CURRENT_PATH", "CURRENT_ROLE", "CURRENT_SCHEMA",
                            "CURRENT_SERVER", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_TIMEZONE",
                            "CURRENT_TRANSFORM_GROUP_FOR_TYPE", "CURRENT_USER", "CURSOR", "CYCLE", "DATA", "DATABASE",
                            "DATAPARTITIONNAME", "DATAPARTITIONNUM", "DATE", "DAY", "DAYS", "DB2GENERAL", "DB2GENRL", "DB2SQL",
                            "DBINFO", "DBPARTITIONNAME", "DBPARTITIONNUM", "DEALLOCATE", "DEC", "DECIMAL", "DECLARE", "DEFAULT",
                            "DEFAULTS", "DEFINITION", "DELETE", "DENSE_RANK", "DENSERANK", "DEREF", "DESCRIBE", "DESCRIPTOR",
                            "DETERMINISTIC", "DIAGNOSTICS", "DISABLE", "DISALLOW", "DISCONNECT", "DISTINCT", "DO", "DOCUMENT",
                            "DOUBLE", "DROP", "DSSIZE", "DYNAMIC", "EACH", "EDITPROC", "ELEMENT", "ELSE", "ELSEIF", "ENABLE",
                            "ENCODING", "ENCRYPTION", "END", "END-EXEC", "ENDING", "ERASE", "ESCAPE", "EVERY", "EXCEPT",
                            "EXCEPTION", "EXCLUDING", "EXCLUSIVE", "EXEC", "EXECUTE", "EXISTS", "EXIT", "EXP", "EXPLAIN", "EXTENDED",
                            "EXTERNAL", "EXTRACT", "FENCED", "FETCH", "FIELDPROC", "FILE", "FILTER", "FINAL", "FLOAT", "FLOOR",
                            "FOR", "FOREIGN", "FREE", "FROM", "FULL", "FUNCTION", "FUSION", "GENERAL", "GENERATED", "GET", "GLOBAL",
                            "GO", "GOTO", "GRANT", "GRAPHIC", "GROUP", "GROUPING", "HANDLER", "HASH", "HASHED_VALUE", "HAVING",
                            "HINT", "HOLD", "HOUR", "HOURS", "IDENTITY", "IF", "IMMEDIATE", "IN", "INCLUDING", "INCLUSIVE",
                            "INCREMENT", "INDEX", "INDICATOR", "INDICATORS", "INF", "INFINITY", "INHERIT", "INNER", "INOUT",
                            "INSENSITIVE", "INSERT", "INT", "INTEGER", "INTEGRITY", "INTERSECT", "INTERSECTION", "INTERVAL", "INTO",
                            "IS", "ISOBID", "ISOLATION", "ITERATE", "JAR", "JAVA", "JOIN", "KEEP", "KEY", "LABEL", "LANGUAGE",
                            "LARGE", "LATERAL", "LC_CTYPE", "LEADING", "LEAVE", "LEFT", "LIKE", "LIMIT", "LINKTYPE", "LN", "LOCAL",
                            "LOCALDATE", "LOCALE", "LOCALTIME", "LOCALTIMESTAMP", "LOCATOR", "LOCATORS", "LOCK", "LOCKMAX",
                            "LOCKSIZE", "LONG", "LOOP", "LOWER", "MAINTAINED", "MATCH", "MATERIALIZED", "MAX", "MAXVALUE", "MEMBER",
                            "MERGE", "METHOD", "MICROSECOND", "MICROSECONDS", "MIN", "MINUTE", "MINUTES", "MINVALUE", "MOD", "MODE",
                            "MODIFIES", "MODULE", "MONTH", "MONTHS", "MULTISET", "NAN", "NATIONAL", "NATURAL", "NCHAR", "NCLOB",
                            "NEW", "NEW_TABLE", "NEXTVAL", "NO", "NOCACHE", "NOCYCLE", "NODENAME", "NODENUMBER", "NOMAXVALUE",
                            "NOMINVALUE", "NONE", "NOORDER", "NORMALIZE", "NORMALIZED", "NOT", "NULL", "NULLIF", "NULLS", "NUMERIC",
                            "NUMPARTS", "OBID", "OCTET_LENGTH", "OF", "OFFSET", "OLD", "OLD_TABLE", "ON", "ONLY", "OPEN",
                            "OPTIMIZATION", "OPTIMIZE", "OPTION", "OR", "ORDER", "OUT", "OUTER", "OVER", "OVERLAPS", "OVERLAY",
                            "OVERRIDING", "PACKAGE", "PADDED", "PAGESIZE", "PARAMETER", "PART", "PARTITION", "PARTITIONED",
                            "PARTITIONING", "PARTITIONS", "PASSWORD", "PATH", "PERCENT_RANK", "PERCENTILE_CONT", "PERCENTILE_DISC",
                            "PIECESIZE", "PLAN", "POSITION", "POWER", "PRECISION", "PREPARE", "PREVVAL", "PRIMARY", "PRIQTY",
                            "PRIVILEGES", "PROCEDURE", "PROGRAM", "PSID", "PUBLIC", "QUERY", "QUERYNO", "RANGE", "RANK", "READ",
                            "READS", "REAL", "RECOVERY", "RECURSIVE", "REF", "REFERENCES", "REFERENCING", "REFRESH", "REGR_AVGX",
                            "REGR_AVGY", "REGR_COUNT", "REGR_INTERCEPT", "REGR_R2", "REGR_SLOPE", "REGR_SXX", "REGR_SXY", "REGR_SYY",
                            "RELEASE", "RENAME", "REPEAT", "RESET", "RESIGNAL", "RESTART", "RESTRICT", "RESULT", "RESULT_SET_LOCATOR",
                            "RETURN", "RETURNS", "REVOKE", "RIGHT", "ROLE", "ROLLBACK", "ROLLUP", "ROUND_CEILING", "ROUND_DOWN",
                            "ROUND_FLOOR", "ROUND_HALF_DOWN", "ROUND_HALF_EVEN", "ROUND_HALF_UP", "ROUND_UP", "ROUTINE", "ROW",
                            "ROW_NUMBER", "ROWNUMBER", "ROWS", "ROWSET", "RRN", "RUN", "SAVEPOINT", "SCHEMA", "SCOPE", "SCRATCHPAD",
                            "SCROLL", "SEARCH", "SECOND", "SECONDS", "SECQTY", "SECURITY", "SELECT", "SENSITIVE", "SEQUENCE",
                            "SESSION", "SESSION_USER", "SET", "SIGNAL", "SIMILAR", "SIMPLE", "SMALLINT", "SNAN", "SOME", "SOURCE",
                            "SPECIFIC", "SPECIFICTYPE", "SQL", "SQLEXCEPTION", "SQLID", "SQLSTATE", "SQLWARNING", "SQRT", "STACKED",
                            "STANDARD", "START", "STARTING", "STATEMENT", "STATIC", "STATMENT", "STAY", "STDDEV_POP", "STDDEV_SAMP",
                            "STOGROUP", "STORES", "STYLE", "SUBMULTISET", "SUBSTRING", "SUM", "SUMMARY", "SYMMETRIC", "SYNONYM",
                            "SYSFUN", "SYSIBM", "SYSPROC", "SYSTEM", "SYSTEM_USER", "TABLE", "TABLESAMPLE", "TABLESPACE", "THEN",
                            "TIME", "TIMESTAMP", "TIMEZONE_HOUR", "TIMEZONE_MINUTE", "TO", "TRAILING", "TRANSACTION", "TRANSLATE",
                            "TRANSLATION", "TREAT", "TRIGGER", "TRIM", "TRUNCATE", "TYPE", "UESCAPE", "UNDO", "UNION", "UNIQUE",
                            "UNKNOWN", "UNNEST", "UNTIL", "UPDATE", "UPPER", "USAGE", "USER", "USING", "VALIDPROC", "VALUE", "VALUES",
                            "VAR_POP", "VAR_SAMP", "VARCHAR", "VARIABLE", "VARIANT", "VARYING", "VCAT", "VERSION", "VIEW", "VOLATILE",
                            "VOLUMES", "WHEN", "WHENEVER", "WHERE", "WHILE", "WIDTH_BUCKET", "WINDOW", "WITH", "WITHIN", "WITHOUT",
                            "WLM", "WRITE", "XMLELEMENT", "XMLEXISTS", "XMLNAMESPACES", "YEAR", "YEARS", "FALSE", "TRUE",
                        },
                        StringComparer.OrdinalIgnoreCase );
                } // if

                return this.mReservedWords;
            } // get
        } // public HashSet<string> ReservedWords

        #endregion

        #region Implement protected method

        /// <summary> Just create connection object and wihout any setting of property. </summary>
        /// <returns> Connection object of specify DB type. </returns>
        public DbConnection CreateConnectionObject() {
            return new DB2Connection();
        } // public DbConnection CreateConnectionObject()

        /// <summary> Just create command object and wihout any setting of property. </summary>
        /// <returns> Command object of specify DB type. </returns>
        public DbCommand CreateCommandObject() {
            return new DB2Command();
        } // public DbCommand CreateCommandObject()

        /// <summary> Just create CommandBuilder object and need to set QuotePrefix and QuoteSuffix property. </summary>
        /// <returns> CommandBuilder object of specify DB type. </returns>
        public DbCommandBuilder CreateCommandBuilder() {
            return new DB2CommandBuilder();
        } // public DbCommandBuilder CreateCommandBuilder()

        /// <summary> Just create DataAdapter object and wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specify DB type. </returns>
        public DbDataAdapter CreateDataAdapter() {
            return new DB2DataAdapter();
        } // public DbDataAdapter CreateDataAdapter()
        
        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="ColumnRule"> Special rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        public string ToDBValueString( object value, EColumnRule ColumnRule ) {
            if ( value == null )
                return "NULL";

            else if ( value is DBNull )
                return "NULL";

            else if ( value is string || value is char )
                return $"'{value.CastTo<string>().Replace( "'", "''" )}'";
            
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

            else
                return value.ToString();
        } // public string ValueToDBString( object value, EColumnRule ColumnRule )

        /// <summary> Quote the word by QuoteType. </summary>
        /// <param name="Word"> the word to quote. </param>
        /// <param name="QuoteType"> how to quote the word. </param>
        /// <returns> the word after quote. </returns>
        public string Quote( string Word, EQuoteType QuoteType = EQuoteType.Always ) {
            if ( QuoteType == EQuoteType.Always ) return $"{this.QuotePrefix}{Word}{this.QuoteSuffix}";

            if ( QuoteType == EQuoteType.Auto && this.ReservedWords.Contains( Word ) ) return $"{this.QuotePrefix}{Word}{this.QuoteSuffix}";

            return Word;
        } // public string Quote( string Word, EQuoteType QuoteType = EQuoteType.Always )

        #endregion
        
        #region Select Schema

        #region Implement

        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException ) => throw new NotSupportedException();
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string[] TableNames, int? timeout, bool? throwException ) => throw new NotSupportedException();
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string TableName, int? timeout, bool? throwException ) => throw new NotSupportedException();

        #endregion
        
        #endregion

    } // public class DB2Provider : IDatabaseProvider

} // namespace MayaFramework
