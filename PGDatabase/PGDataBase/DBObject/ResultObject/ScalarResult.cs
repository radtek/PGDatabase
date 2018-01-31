using System;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Result object when ExecuteScalar with DBSpool. </summary>
    public class ScalarResult<T> : SingleResult<T> {

        /// <summary> the real script to query from db. </summary>
        public string ExecuteScript { get; private set; }
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="Result"> result of query. </param>
        internal ScalarResult( string ExecuteScript, bool Success, T Result )
            : base( Success, null, Result ) {
            this.ExecuteScript = ExecuteScript;
        } // internal ScalarResult( string ExecuteScript, bool Success, T Result )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal ScalarResult( string ExecuteScript, Exception Exception )
            : base( Exception ) {
            this.ExecuteScript = ExecuteScript;
        } // internal ScalarResult( string ExecuteScript, Exception Exception )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="Value"> result of query. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal ScalarResult( string ExecuteScript, bool Success, T Value, Exception Exception )
            : base( Success, Exception, Value ) {
            this.ExecuteScript = ExecuteScript;
        } // internal ScalarResult( string ExecuteScript, bool Success, T Value, Exception Exception )
        
        /// <summary> Convert this object to string, and show more detail. </summary>
        /// <returns> string for more detail. </returns>
        public override string ToString() {
            return $@"{this.GetType().Name} - 
Success:{this.Success.ToString()}
Result Type:{typeof( T ).FullName}
Result:{this.Value?.ToString()}
Script:{this.ExecuteScript}
Exception:{( this.Exception == null ? "null" : this.Exception.ToString())}";
        } // public override string ToString()

    } // public class ScalarResult<T>
} // namespace PGLibrary.Database
