using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using PGCafe.Object;

namespace PGLibrary.Database {
    
    /// <summary>
    /// Proviod method to query DB.
    /// * Not thread safe in multiple thread, each thread should create one object to use.
    /// * Use short connect when query.
    /// </summary>
    public class SQLServerDatabase : PGDatabase {

        /// <summary> Initial SQLServerPGDatabase with ConnectionString, DefaultTimeout. </summary>
        /// <param name="ConnectionString"> Connection string to connect to DB. </param>
        /// <param name="DefaultTimeout"> default timeout when query from DB. set null to use default value of command object. </param>
        /// <param name="DefaultThrowException"> Default value of throw exception when error in execute method or not. </param>
        public SQLServerDatabase( string ConnectionString, int? DefaultTimeout = null, bool DefaultThrowException = false )
            : base( new SQLServerProvider(), ConnectionString, DefaultTimeout, DefaultThrowException ) { }

    } // public class SQLServerDatabase : PGDatabase

} // namespace PGLibrary.Database
