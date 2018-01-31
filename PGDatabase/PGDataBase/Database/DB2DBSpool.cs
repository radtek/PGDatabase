using PGLibrary.Database;

namespace MayaFramework {
    
    /// <summary>
    /// Proviod method to query DB. ( if no need the log, use this to avoid reference error of log4net.dll )
    /// * Not thread safe in multiple thread, each thread should create one object to use.
    /// * Use short connect when query.
    /// </summary>
    public class DB2Database : PGDatabase {

        /// <summary> Initial DB2DBSpool with ConnectionString, DefaultTimeout. </summary>
        /// <param name="ConnectionString"> Connection string to connect to DB. </param>
        /// <param name="DefaultTimeout"> default timeout when query from DB. set null to use default value of command object. </param>
        /// <param name="DefaultThrowException"> Default value of throw exception argument in method. </param>
        public DB2Database( string ConnectionString, int? DefaultTimeout = null, bool DefaultThrowException = false )
            : base( new DB2Provider(), ConnectionString, DefaultTimeout, DefaultThrowException ) { }

    } // public class DB2Database : PGDatabase

} // namespace MayaFramework
