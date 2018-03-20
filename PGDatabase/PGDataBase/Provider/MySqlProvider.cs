using System;
using System.Collections.Generic;
using System.Data.Common;
using PGCafe;
using MySql.Data.MySqlClient;
using PGLibrary.Database;
using PGCafe.Object;

namespace MayaFramework {
    /// <summary> proviod somethign to use in DBSpool by MySql DataBase. </summary>
    public class MySqlProvider : IDatabaseProvider {
        /// <summary> DataBase's type. </summary>
        public EDatabaseType DatabaseType => EDatabaseType.MySql;

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
                        new[] { "ACCESSIBLE", "ADD", "AGGREGATE", "ALTER", "ANALYZE", "AS", "ASENSITIVE", "AUTO_INCREMENT",
                            "BACKUP", "BETWEEN", "BINLOG", "BLOCK", "BOTH", "BYTE", "CASCADE", "CATALOG_NAME", "CHANGED",
                            "CHARACTER", "CHECKSUM", "CLIENT", "CODE", "COLUMN", "COLUMN_NAME", "COMMITTED", "COMPRESSED",
                            "CONDITION", "CONSTRAINT", "CONSTRAINT_SCHEMA", "CONTINUE", "CREATE", "CURRENT", "CURRENT_TIMESTAMP",
                            "CURSOR_NAME", "DATABASES", "DATETIME", "DAY_MICROSECOND", "DEALLOCATE", "DECLARE", "DEFINER",
                            "DELETE", "DES_KEY_FILE", "DIRECTORY", "DISK", "DIV", "DROP", "DUPLICATE", "ELSE", "ENCLOSED",
                            "ENDS", "ENUM", "ESCAPE", "EVENTS", "EXECUTE", "EXPANSION", "EXPORT", "FALSE", "FETCH",
                            "FILE_BLOCK_SIZE", "FIXED", "FLOAT8", "FOR", "FORMAT", "FULL", "GENERAL", "GEOMETRYCOLLECTION",
                            "GLOBAL", "GROUP", "HASH", "HIGH_PRIORITY", "HOUR", "HOUR_SECOND", "IGNORE", "IN",
                            "INFILE", "INOUT", "INSERT_METHOD", "INT", "INT3", "INTEGER", "INVOKER", "IO_BEFORE_GTIDS",
                            "IS", "ITERATE", "KEY", "KILL", "LEADING", "LEFT", "LIKE", "LINES", "LOAD", "LOCALTIMESTAMP",
                            "LOGFILE", "LONGBLOB", "LOW_PRIORITY", "MASTER_BIND", "MASTER_HEARTBEAT_PERIOD", "MASTER_LOG_POS",
                            "MASTER_RETRY_COUNT", "MASTER_SSL_CA", "MASTER_SSL_CIPHER", "MASTER_SSL_KEY", "MASTER_USER", "MAX_CONNECTIONS_PER_HOUR",
                            "MAX_SIZE", "MAX_USER_CONNECTIONS", "MEDIUMINT", "MERGE", "MIDDLEINT", "MINUTE_MICROSECOND", "MOD",
                            "MODIFY", "MULTIPOINT", "MYSQL_ERRNO", "NATIONAL", "NDB", "NEW", "NODEGROUP", "NOT", "NULL", "NVARCHAR",
                            "ON", "OPEN", "OPTION", "OR", "OUTER", "PACK_KEYS", "PARSE_GCOL_EXPR", "PARTITIONING", "PHASE",
                            "PLUGIN_DIR", "PORT", "PREPARE", "PRIMARY", "PROCESSLIST", "PROXY", "QUERY", "READ", "READ_WRITE",
                            "RECOVER", "REDUNDANT", "RELAY", "RELAY_LOG_POS", "RELOAD", "REORGANIZE", "REPEATABLE", "REPLICATE_DO_TABLE",
                            "REPLICATE_REWRITE_DB", "REPLICATION", "RESIGNAL", "RESUME", "RETURNS", "RIGHT", "ROLLUP", "ROW", "ROW_FORMAT",
                            "SCHEDULE", "SCHEMA_NAME", "SECURITY", "SEPARATOR", "SERVER", "SHARE", "SIGNAL", "SLAVE", "SNAPSHOT", "SONAME",
                            "SPATIAL", "SQLEXCEPTION", "SQL_AFTER_GTIDS", "SQL_BIG_RESULT", "SQL_CALC_FOUND_ROWS", "SQL_THREAD",
                            "SQL_TSI_MINUTE", "SQL_TSI_SECOND", "SSL", "STARTING", "STATS_PERSISTENT", "STOP", "STRAIGHT_JOIN", "SUBJECT",
                            "SUPER", "SWITCHES", "TABLESPACE", "TEMPORARY", "TEXT", "TIME", "TIMESTAMPDIFF", "TINYTEXT", "TRANSACTION", "TRUE",
                            "TYPES", "UNDO", "UNICODE", "UNIQUE", "UNSIGNED", "UPGRADE", "USER", "USING", "UTC_TIMESTAMP", "VALUES",
                            "VARCHARACTER", "VIEW", "WARNINGS", "WHEN", "WITH", "WRAPPER", "XA", "XOR", "ZEROFILL", "ACCOUNT", "AFTER",
                            "ALGORITHM", "ALWAYS", "AND", "ASC", "AT", "AVG", "BEFORE", "BIGINT", "BIT", "BOOL", "BTREE", "CACHE", "CASCADED",
                            "CHAIN", "CHANNEL", "CHARSET", "CIPHER", "CLOSE", "COLLATE", "COLUMNS", "COMMENT", "COMPACT", "COMPRESSION",
                            "CONNECTION", "CONSTRAINT_CATALOG", "CONTAINS", "CONVERT", "CROSS", "CURRENT_DATE", "CURRENT_USER", "DATA",
                            "DATAFILE", "DAY", "DAY_MINUTE", "DEC", "DEFAULT", "DELAYED", "DESC", "DETERMINISTIC", "DISABLE", "DISTINCT",
                            "DO", "DUAL", "DYNAMIC", "ELSEIF", "ENCRYPTION", "ENGINE", "ERROR", "ESCAPED", "EVERY", "EXISTS", "EXPIRE",
                            "EXTENDED", "FAST", "FIELDS", "FILTER", "FLOAT", "FLUSH", "FORCE", "FOUND", "FULLTEXT", "GENERATED", "GET",
                            "GRANT", "GROUP_REPLICATION", "HAVING", "HOST", "HOUR_MICROSECOND", "IDENTIFIED", "IGNORE_SERVER_IDS", "INDEX",
                            "INITIAL_SIZE", "INSENSITIVE", "INSTALL", "INT1", "INT4", "INTERVAL", "IO", "IO_THREAD", "ISOLATION", "JOIN",
                            "KEYS", "LANGUAGE", "LEAVE", "LESS", "LIMIT", "LINESTRING", "LOCAL", "LOCK", "LOGS", "LONGTEXT", "MASTER",
                            "MASTER_CONNECT_RETRY", "MASTER_HOST", "MASTER_PASSWORD", "MASTER_SERVER_ID", "MASTER_SSL_CAPATH", "MASTER_SSL_CRL",
                            "MASTER_SSL_VERIFY_SERVER_CERT", "MATCH", "MAX_QUERIES_PER_HOUR", "MAX_STATEMENT_TIME", "MEDIUM", "MEDIUMTEXT",
                            "MESSAGE_TEXT", "MIGRATE", "MINUTE_SECOND", "MODE", "MONTH", "MULTIPOLYGON", "NAME", "NATURAL", "NDBCLUSTER", "NEXT",
                            "NONBLOCKING", "NO_WAIT", "NUMBER", "OFFSET", "ONE", "OPTIMIZE", "OPTIONALLY", "ORDER", "OUTFILE", "PAGE", "PARTIAL",
                            "PARTITIONS", "PLUGIN", "POINT", "PRECEDES", "PRESERVE", "PRIVILEGES", "PROFILE", "PURGE", "QUICK", "READS", "REAL",
                            "REDOFILE", "REFERENCES", "RELAYLOG", "RELAY_THREAD", "REMOVE", "REPAIR", "REPLACE", "REPLICATE_IGNORE_DB",
                            "REPLICATE_WILD_DO_TABLE", "REQUIRE", "RESTORE", "RETURN", "REVERSE", "RLIKE", "ROTATE", "ROWS", "RTREE", "SCHEMA",
                            "SECOND", "SELECT", "SERIAL", "SESSION", "SHOW", "SIGNED", "SLOW", "SOCKET", "SOUNDS", "SPECIFIC", "SQLSTATE",
                            "SQL_AFTER_MTS_GAPS", "SQL_BUFFER_RESULT", "SQL_NO_CACHE", "SQL_TSI_DAY", "SQL_TSI_MONTH", "SQL_TSI_WEEK", "STACKED",
                            "STARTS", "STATS_SAMPLE_PAGES", "STORAGE", "STRING", "SUBPARTITION", "SUSPEND", "TABLE", "TABLE_CHECKSUM", "TEMPTABLE",
                            "THAN", "TIMESTAMP", "TINYBLOB", "TO", "TRIGGER", "TRUNCATE", "UNCOMMITTED", "UNDOFILE", "UNINSTALL", "UNKNOWN",
                            "UNTIL", "USAGE", "USER_RESOURCES", "UTC_DATE", "VALIDATION", "VARBINARY", "VARIABLES", "VIRTUAL", "WEEK", "WHERE",
                            "WITHOUT", "WRITE", "XID", "YEAR", "ACTION", "AGAINST", "ALL", "ANALYSE", "ANY", "ASCII", "AUTOEXTEND_SIZE",
                            "AVG_ROW_LENGTH", "BEGIN", "BINARY", "BLOB", "BOOLEAN", "BY", "CALL", "CASE", "CHANGE", "CHAR", "CHECK",
                            "CLASS_ORIGIN", "COALESCE", "COLLATION", "COLUMN_FORMAT", "COMMIT", "COMPLETION", "CONCURRENT", "CONSISTENT",
                            "CONSTRAINT_NAME", "CONTEXT", "CPU", "CUBE", "CURRENT_TIME", "CURSOR", "DATABASE", "DATE", "DAY_HOUR", "DAY_SECOND",
                            "DECIMAL", "DEFAULT_AUTH", "DELAY_KEY_WRITE", "DESCRIBE", "DIAGNOSTICS", "DISCARD", "DISTINCTROW", "DOUBLE",
                            "DUMPFILE", "EACH", "ENABLE", "END", "ENGINES", "ERRORS", "EVENT", "EXCHANGE", "EXIT", "EXPLAIN", "EXTENT_SIZE",
                            "FAULTS", "FILE", "FIRST", "FLOAT4", "FOLLOWS", "FOREIGN", "FROM", "FUNCTION", "GEOMETRY", "GET_FORMAT", "GRANTS",
                            "HANDLER", "HELP", "HOSTS", "HOUR_MINUTE", "IF", "IMPORT", "INDEXES", "INNER", "INSERT", "INSTANCE", "INT2", "INT8",
                            "INTO", "IO_AFTER_GTIDS", "IPC", "ISSUER", "JSON", "KEY_BLOCK_SIZE", "LAST", "LEAVES", "LEVEL", "LINEAR", "LIST",
                            "LOCALTIME", "LOCKS", "LONG", "LOOP", "MASTER_AUTO_POSITION", "MASTER_DELAY", "MASTER_LOG_FILE", "MASTER_PORT",
                            "MASTER_SSL", "MASTER_SSL_CERT", "MASTER_SSL_CRLPATH", "MASTER_TLS_VERSION", "MAXVALUE", "MAX_ROWS", "MAX_UPDATES_PER_HOUR",
                            "MEDIUMBLOB", "MEMORY", "MICROSECOND", "MINUTE", "MIN_ROWS", "MODIFIES", "MULTILINESTRING", "MUTEX", "NAMES", "NCHAR",
                            "NEVER", "NO", "NONE", "NO_WRITE_TO_BINLOG", "NUMERIC", "OLD_PASSWORD", "ONLY", "OPTIMIZER_COSTS", "OPTIONS", "OUT",
                            "OWNER", "PARSER", "PARTITION", "PASSWORD", "PLUGINS", "POLYGON", "PRECISION", "PREV", "PROCEDURE", "PROFILES",
                            "QUARTER", "RANGE", "READ_ONLY", "REBUILD", "REDO_BUFFER_SIZE", "REGEXP", "RELAY_LOG_FILE", "RELEASE", "RENAME",
                            "REPEAT", "REPLICATE_DO_DB", "REPLICATE_IGNORE_TABLE", "REPLICATE_WILD_IGNORE_TABLE", "RESET", "RESTRICT", "RETURNED_SQLSTATE",
                            "REVOKE", "ROLLBACK", "ROUTINE", "ROW_COUNT", "SAVEPOINT", "SCHEMAS", "SECOND_MICROSECOND", "SENSITIVE", "SERIALIZABLE",
                            "SET", "SHUTDOWN", "SIMPLE", "SMALLINT", "SOME", "SOURCE", "SQL", "SQLWARNING", "SQL_BEFORE_GTIDS", "SQL_CACHE",
                            "SQL_SMALL_RESULT", "SQL_TSI_HOUR", "SQL_TSI_QUARTER", "SQL_TSI_YEAR", "START", "STATS_AUTO_RECALC", "STATUS", "STORED",
                            "SUBCLASS_ORIGIN", "SUBPARTITIONS", "SWAPS", "TABLES", "TABLE_NAME", "TERMINATED", "THEN", "TIMESTAMPADD", "TINYINT",
                            "TRAILING", "TRIGGERS", "TYPE", "UNDEFINED", "UNDO_BUFFER_SIZE", "UNION", "UNLOCK", "UPDATE", "USE", "USE_FRM",
                            "UTC_TIME", "VALUE", "VARCHAR", "VARYING", "WAIT", "WEIGHT_STRING", "WHILE", "WORK", "X509", "XML", "YEAR_MONTH",
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
            return new MySqlConnection();
        } // public DbConnection CreateConnectionObject()

        /// <summary> Just create command object and wihout any setting of property. </summary>
        /// <returns> Command object of specify DB type. </returns>
        public DbCommand CreateCommandObject() {
            return new MySqlCommand();
        } // public DbCommand CreateCommandObject()

        /// <summary> Just create CommandBuilder object and need to set QuotePrefix and QuoteSuffix property. </summary>
        /// <returns> CommandBuilder object of specify DB type. </returns>
        public DbCommandBuilder CreateCommandBuilder() {
            return new MySqlCommandBuilder();
        } // public DbCommandBuilder CreateCommandBuilder()

        /// <summary> Just create DataAdapter object and wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specify DB type. </returns>
        public DbDataAdapter CreateDataAdapter() {
            return new MySqlDataAdapter();
        } // public DbDataAdapter CreateDataAdapter()
        
        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="ColumnRule"> Special rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        public string ToDBValueString( object value, EColumnRule ColumnRule ) {
            if ( value == null ) return "NULL";

            if ( value is DBNull )
                return "NULL";

            if ( value is string || value is char )
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

            if ( value is DateTime )
                return $"'{value.CastTo<DateTime>():yyyy/MM/dd HH:mm:ss.fff}'";

            if ( value is bool )
                return value.CastTo<bool>() ? "True" : "False";

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

    } // public class MySqlProvider : IDatabaseProvider

} // namespace MayaFramework
