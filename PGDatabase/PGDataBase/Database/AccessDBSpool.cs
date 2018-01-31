using PGLibrary.Database;

namespace MayaFramework {
    
    /// <summary>
    /// Proviod method to query DB. ( if no need the log, use this to avoid reference error of log4net.dll )
    /// * Not thread safe in multiple thread, each thread should create one object to use.
    /// * Use short connect when query.
    /// </summary>
    public class AccessDatabase : PGDatabase {

        /// <summary> Initial AccessDBSpool with ConnectionString, DefaultTimeout. </summary>
        /// <param name="ConnectionString"> Connection string to connect to DB. </param>
        /// <param name="DefaultTimeout"> default timeout when query from DB. set null to use default value of command object. </param>
        /// <param name="DefaultThrowException"> Default value of throw exception argument in method. </param>
        public AccessDatabase( string ConnectionString, int? DefaultTimeout = null, bool DefaultThrowException = false )
            : base( new AccessProvider(), ConnectionString, DefaultTimeout, DefaultThrowException ) { }

    } // public class AccessDatabase : PGDatabase

} // namespace MayaFramework
