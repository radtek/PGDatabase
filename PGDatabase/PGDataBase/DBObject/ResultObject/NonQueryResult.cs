using System;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Result object when ExecuteNonQuery with DBSpool. </summary>
    public class NonQueryResult : EmptyResult {

        /// <summary> the real script to query from db. </summary>
        public string ExecuteScript { get; private set; }

        /// <summary> affected count when query from db. </summary>
        public int AffectedCount { get; private set; }

        /// <summary> quick to check has any affected data in db. </summary>
        public bool HasAffectedData => this.AffectedCount > 0;

        /// <summary> quick to check has any affected schema in db. ( check is <see cref="AffectedCount"/> eqaul to -1 ) </summary>
        public bool HasAffectedSchema => this.AffectedCount == -1;


        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="AffectedCount"> affected count when query from db. </param>
        internal NonQueryResult( string ExecuteScript, bool Success, int AffectedCount )
            : base ( Success, null ) {
            this.ExecuteScript = ExecuteScript;
            this.AffectedCount = AffectedCount;
        } // internal NonQueryResult( string ExecuteScript, bool Success, int AffectedCount )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal NonQueryResult( string ExecuteScript, Exception Exception )
            : base( false, Exception ) {
            this.ExecuteScript = ExecuteScript;
        } // internal NonQueryResult( string ExecuteScript, Exception Exception )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="AffectedCount"> affected count when query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal NonQueryResult( string ExecuteScript, bool Success, int AffectedCount, Exception Exception )
            : base( Success, Exception ) {
            this.ExecuteScript = ExecuteScript;
            this.AffectedCount = AffectedCount;
        } // internal NonQueryResult( string ExecuteScript, bool Success, int AffectedCount, Exception Exception )
        
        /// <summary> Convert this object to string, and show more detail. </summary>
        /// <returns> string for more detail. </returns>
        public override string ToString() {
            return $@"{this.GetType().Name} - 
Success:{this.Success.ToString()}
AffectedCount:{this.AffectedCount}
Script:{this.ExecuteScript}
Exception:{( this.Exception == null ? "null" : this.Exception.ToString())}";
        } // public override string ToString()

    } // public class NonQueryResult : EmptyResult
} // namespace PGLibrary.Database
