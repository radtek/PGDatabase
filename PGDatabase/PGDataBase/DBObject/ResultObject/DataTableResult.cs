using System;
using System.Data;
using System.Linq;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Result object when ExecuteDataTable with DBSpool. </summary>
    public class DataTableResult : SingleResult<DataTable> {

        /// <summary> the real script to query from db. </summary>
        public string ExecuteScript { get; private set; }

        /// <summary> quick to get Result's row count, if Result is null, reutrn -1. </summary>
        public int RowCount => this.Value?.Rows.Count ?? -1;

        /// <summary> quick to get Result's first row, if Result is null, return null. </summary>
        public DataRow FirstRow => this.Value?.AsEnumerable().FirstOrDefault();


        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="Result"> result of query. </param>
        internal DataTableResult( string ExecuteScript, bool Success, DataTable Result )
            : base( Success, null, Result ) {
            this.ExecuteScript = ExecuteScript;
        } // internal DataTableResult( string ExecuteScript, bool Success, DataTable Result )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal DataTableResult( string ExecuteScript, Exception Exception )
            : base( Exception ) {
            this.ExecuteScript = ExecuteScript;
        } // internal DataTableResult( string ExecuteScript, Exception Exception )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="Result"> result of query. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal DataTableResult( string ExecuteScript, bool Success, DataTable Result, Exception Exception )
            : base( Success, Exception, Result ) {
            this.ExecuteScript = ExecuteScript;
        } // internal DataTableResult( string ExecuteScript, bool Success, DataTable Result, Exception Exception )

        /// <summary> Convert this object to string, and show more detail. </summary>
        /// <returns> string for more detail. </returns>
        public override string ToString() {
            return $@"DataTableResult - 
Success:{this.Success.ToString()}
RowCount:{( this.Value == null ? "null" : this.RowCount.ToString())}
Script:{this.ExecuteScript}
Exception:{( this.Exception == null ? "null" : this.Exception.ToString())}";
        } // public override string ToString()
    } // public class DataTableResult
} // namespace PGLibrary.Database
