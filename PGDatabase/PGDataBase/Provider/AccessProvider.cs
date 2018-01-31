using System;
using System.Collections.Generic;
using System.Data.Common;
using PGCafe;
using System.Data.OleDb;
using PGLibrary.Database;
using PGCafe.Object;

namespace MayaFramework {
    /// <summary> proviod somethign to use in DBSpool by Access DataBase. </summary>
    public class AccessProvider : IDatabaseProvider {
        /// <summary> DataBase's type. </summary>
        public EDatabaseType DatabaseType => EDatabaseType.Access;

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
                        new[] { "ABSOLUTE", "ACTION", "ADD", "ADMINDB", "ALL", "ALLOCATE", "ALPHANUMERIC", "ALTER", "AND",
                            "ANY", "APPLICATION", "ARE", "AS", "ASC", "ASSERTION", "ASSISTANT", "AT", "AUTHORIZATION",
                            "AUTOINCREMENT", "AVG", "BAND", "BEGIN", "BETWEEN", "BINARY", "BIT", "BIT_LENGTH", "BNOT",
                            "BOOLEAN", "BOR", "BOTH", "BXOR", "BY", "BYTE", "CASCADE", "CASCADED", "CASE", "CAST", "CATALOG",
                            "CHAR", "CHAR, CHARACTER", "CHAR_LENGTH", "CHARACTER", "CHARACTER_LENGTH", "CHECK", "CLOSE",
                            "COALESCE", "COLLATE", "COLLATION", "COLUMN", "COMMIT", "COMP", "COMPACTDATABASE", "COMPRESSION",
                            "CONNECT", "CONNECTION", "CONSTRAINT", "CONSTRAINTS", "CONTAINER", "CONTINUE", "CONVERT",
                            "CORRESPONDING", "COUNT", "COUNTER", "CREATE", "CREATEDATABASE", "CREATEDB", "CREATEFIELD",
                            "CREATEGROUP", "CREATEINDEX", "CREATEOBJECT", "CREATEPROPERTY", "CREATERELATION", "CREATETABLEDEF",
                            "CREATEUSER", "CREATEWORKSPACE", "CROSS", "CURRENCY", "CURRENT", "CURRENT_DATE", "CURRENT_TIME",
                            "CURRENT_TIMESTAMP", "CURRENT_USER", "CURRENTUSER", "CURSOR", "DATABASE", "DATE", "DATETIME", "DAY",
                            "DEALLOCATE", "DEC", "DECIMAL", "DECLARE", "DEFAULT", "DEFERRABLE", "DEFERRED", "DELETE", "DESC",
                            "DESCRIBE", "DESCRIPTION", "DESCRIPTOR", "DIAGNOSTICS", "DISALLOW", "DISCONNECT", "DISTINCT",
                            "DISTINCTROW", "DOCUMENT", "DOMAIN", "DOUBLE", "DROP", "ECHO", "ELSE", "END", "END-EXEC", "EQV",
                            "ERROR", "ESCAPE", "EXCEPT", "EXCEPTION", "EXCLUSIVECONNECT", "EXEC", "EXECUTE", "EXISTS", "EXIT",
                            "EXTERNAL", "EXTRACT", "FALSE", "FETCH", "FIELD, FIELDS", "FILLCACHE", "FIRST", "FLOAT",
                            "FLOAT4", "FLOAT8", "FOR", "FOREIGN", "FORM, FORMS", "FOUND", "FROM", "FULL", "FUNCTION",
                            "GENERAL", "GET", "GETOBJECT", "GETOPTION", "GLOBAL", "GO", "GOTO", "GOTOPAGE", "GRANT", "GROUP",
                            "GROUP BY", "GUID", "HAVING", "HOUR", "IDENTITY", "IDLE", "IEEEDOUBLE", "IEEEDOUBLE, IEEESINGLE",
                            "IEEESINGLE", "IF", "IGNORE", "IMAGE", "IMMEDIATE", "IMP", "IN", "INDEX", "INDEX, INDEXES",
                            "INDICATOR", "INHERITABLE", "ININDEX", "INITIALLY", "INNER", "INPUT", "INSENSITIVE", "INSERT",
                            "INSERTTEXT", "INT", "INTEGER", "INTEGER1", "INTEGER2", "INTEGER4", "INTERSECT", "INTERVAL",
                            "INTO", "IS", "ISOLATION", "JOIN", "KEY", "LANGUAGE", "LAST", "LASTMODIFIED", "LEADING", "LEFT",
                            "LEVEL", "LIKE", "LOCAL", "LOGICAL", "LOGICAL, LOGICAL1", "LOGICAL1", "LONG", "LONGBINARY",
                            "LONGCHAR", "LONGTEXT", "LOWER", "MACRO", "MATCH", "MAX", "MEMO", "MIN", "MINUTE", "MODULE",
                            "MONEY", "MONTH", "MOVE", "NAME", "NAMES", "NATIONAL", "NATURAL", "NCHAR", "NEWPASSWORD", "NEXT",
                            "NO", "NOT", "NOTE", "NULL", "NULLIF", "NUMBER", "NUMERIC", "OBJECT", "OCTET_LENGTH", "OFF",
                            "OFOLEOBJECT", "OLEOBJECT", "ON", "ONONLY", "OPEN", "OPENRECORDSET", "OPTION", "OR", "ORDER",
                            "ORIENTATION", "ORORDER", "OUTER", "OUTPUT", "OVERLAPS", "OWNERACCESS", "PAD", "PARAMETER",
                            "PARAMETERS", "PARTIAL", "PASSWORD", "PERCENT", "PIVOT", "POSITION", "PRECISION", "PREPARE",
                            "PRESERVE", "PRIMARY", "PRIOR", "PRIVILEGES", "PROC", "PROCEDURE", "PROPERTY", "PUBLIC", "QUERIES",
                            "QUERY", "QUIT", "READ", "REAL", "RECALC", "RECORDSET", "REFERENCES", "REFRESH", "REFRESHLINK",
                            "REGISTERDATABASE", "RELATION", "RELATIVE", "REPAINT", "REPAIRDATABASE", "REPORT", "REPORTS",
                            "REQUERY", "RESTRICT", "REVOKE", "RIGHT", "ROLLBACK", "ROWS", "SCHEMA", "SCREEN", "SCROLL", "SECOND",
                            "SECTION", "SELECT", "SELECTSCHEMA", "SELECTSECURITY", "SESSION", "SESSION_USER", "SET", "SETFOCUS",
                            "SETOPTION", "SHORT", "SINGLE", "SIZE", "SMALLINT", "SOME", "SPACE", "SQL", "SQLCODE", "SQLERROR",
                            "SQLSTATE", "STDEV", "STDEVP", "STRING", "SUBSTRING", "SUM", "SYSTEM_USER", "TABLE", "TABLEDEF",
                            "TABLEDEFS", "TABLEID", "TEMPORARY", "TEXT", "THEN", "TIME", "TIMESTAMP", "TIMEZONE_HOUR",
                            "TIMEZONE_MINUTE", "TO", "TOP", "TRAILING", "TRANSACTION", "TRANSFORM", "TRANSLATE", "TRANSLATION",
                            "TRIM", "TRUE", "TYPE", "UNION", "UNIQUE", "UNIQUEIDENTIFIER", "UNKNOWN", "UPDATE", "UPDATEIDENTITY",
                            "UPDATEOWNER", "UPDATESECURITY", "UPPER", "USAGE", "USER", "USING", "VALUE", "VALUES", "VAR", "VARP",
                            "VARBINARY", "VARBINARY, VARCHAR", "VARCHAR", "VARYING", "VIEW", "WHEN", "WHENEVER", "WHERE", "WITH",
                            "WORK", "WORKSPACE", "WRITE", "XOR", "YEAR", "YES", "YESNO", "ZONE", },
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
            return new OleDbConnection();
        } // public DbConnection CreateConnectionObject()

        /// <summary> Just create command object and wihout any setting of property. </summary>
        /// <returns> Command object of specify DB type. </returns>
        public DbCommand CreateCommandObject() {
            return new OleDbCommand();
        } // public DbCommand CreateCommandObject()

        /// <summary> Just create CommandBuilder object and need to set QuotePrefix and QuoteSuffix property. </summary>
        /// <returns> CommandBuilder object of specify DB type. </returns>
        public DbCommandBuilder CreateCommandBuilder() {
            return new OleDbCommandBuilder();
        } // public DbCommandBuilder CreateCommandBuilder()

        /// <summary> Just create DataAdapter object and wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specify DB type. </returns>
        public DbDataAdapter CreateDataAdapter() {
            return new OleDbDataAdapter();
        } // public DbDataAdapter CreateDataAdapter()
        
        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="ColumnRule"> Special rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        public string ToDBValueString( object value, EColumnRule ColumnRule ) {
            if ( value == null ) return "NULL";

            else if ( value is DBNull )
                return "NULL";

            else if ( value is string || value is char ) {
                return $"'{value.ToString().Replace( "'", "''" )}'";
            } // else if
            
            else if ( value is Enum ) {
                if ( ColumnRule.HasFlag( EColumnRule.EnumToString ) ) {
                    var valueString = value.ToString();
                    return $"'{valueString.Replace( "'", "''" )}'";
                } // else if
            
                else if ( value is Enum && ColumnRule.HasFlag( EColumnRule.EnumDescription ) ) {
                    var valueString = value.CastTo<Enum>().Description();
                    return $"'{valueString.Replace( "'", "''" )}'";
                } // else if
            
                else if ( value is Enum && ColumnRule.HasFlag( EColumnRule.EnumStringValue ) ) {
                    var valueString = value.CastTo<Enum>().StringValue();
                    return $"'{valueString.Replace( "'", "''" )}'";
                } // else if

                else
                    return value.ToType<int>().ToString();

            } // else if

            else if ( value is bool )
                return value.CastTo<bool>() ? "1" : "0";

            else if ( value is DateTime )
                return $"CDate( '{value.CastTo<DateTime>():yyyy/MM/dd HH:mm:ss.fff}' )";
            
            else return value.ToString();
        } // public string ValueToDBString( object value, EColumnRule ColumnRule )

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

        #region Implement

        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, int? timeout, bool? throwException ) => throw new NotSupportedException();
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string[] TableNames, int? timeout, bool? throwException ) => throw new NotSupportedException();
        SingleResult<IEnumerable<ITableSchema>> IDatabaseProvider.SelectSchema( PGDatabase DBObject, string TableName, int? timeout, bool? throwException ) => throw new NotSupportedException();

        #endregion
        
        #endregion



    } // public class AccessProvider : IDatabaseProvider
} // namespace MayaFramework
