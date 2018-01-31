using PGCafe.Object;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace PGLibrary.Database {
    /// <summary> proviod somethign to use in DBSpool by different type of Database. </summary>
    public interface IDatabaseProvider {

        #region Property

        /// <summary> Database's type. </summary>
        EDatabaseType DatabaseType { get; }

        /// <summary> Quote prefix of keyword. </summary>
        string QuotePrefix { get; }
        
        /// <summary> Quote suffix of keyword. </summary>
        string QuoteSuffix { get; }
        
        /// <summary> Reserved word of provider's Database type. </summary>
        HashSet<string> ReservedWords { get; }
        
        /// <summary> Spliter of multiple command or end of command. </summary>
        string CommandSpliter { get; }

        #endregion

        #region Create Object method

        /// <summary> Just create connection object wihout any setting of property. </summary>
        /// <returns> Connection object of specific DB type. </returns>
        DbConnection CreateConnectionObject();

        /// <summary> Just create command object wihout any setting of property. </summary>
        /// <returns> Command object of specific DB type. </returns>
        DbCommand CreateCommandObject();

        /// <summary> Just create CommandBuilder object wihout any setting of property. </summary>
        /// <returns> CommandBuilder object of specific DB type. </returns>
        DbCommandBuilder CreateCommandBuilder();

        /// <summary> Just create DataAdapter object wihout any setting of property. </summary>
        /// <returns> DataAdapter object of specific DB type. </returns>
        DbDataAdapter CreateDataAdapter();

        #endregion

        #region General method

        /// <summary> Convert value to script string for auto generate script. </summary>
        /// <param name="value"> value to convert. </param>
        /// <param name="SpecialRule"> Special rule to use when convert. </param>
        /// <returns> script string of value. </returns>
        string ToDBValueString( object value, EColumnRule SpecialRule );

        /// <summary> Quote the word and return it. </summary>
        /// <param name="Word"> Word to quote. </param>
        /// <param name="QuoteType"> how to quote the word. </param>
        /// <returns> script string of value. </returns>
        string Quote( string Word, EQuoteType QuoteType = EQuoteType.Always );

        #endregion

        #region Select Schema

        /// <summary> Select all table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> SelectSchema( PGDatabase DBObject, int? timeout = null, bool? throwException = null );
        
        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="TableNames"> specific table names to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> SelectSchema( PGDatabase DBObject, string[] TableNames, int? timeout = null, bool? throwException = null );
        
        /// <summary> Select specific table's schema </summary>
        /// <param name="DBObject"> Database object to query.</param>
        /// <param name="TableName"> specific table name to select.</param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        SingleResult<IEnumerable<ITableSchema>> SelectSchema( PGDatabase DBObject, string TableName, int? timeout = null, bool? throwException = null );
        
        #endregion

    } // public interface IDatabaseProvider
} // namespace PGLibrary.Database
