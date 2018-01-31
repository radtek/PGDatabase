using System;
using System.Data;
using System.Linq;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Result object when ExecuteDataTableOrNonQueryResult with DBSpool. </summary>
    public class DataTableOrNonQueryResult : EmptyResult {

        /// <summary> the real script to query from db. </summary>
        public string ExecuteScript { get; private set; }

        /// <summary> has query the table or not. </summary>
        public bool HasQueryTable => this.Table != null;

        /// <summary> result table of query </summary>
        public DataTable Table { get; private set; }

        /// <summary> quick to get Result's row count, if Result is null, reutrn -1. </summary>
        public int RowCount => this.Table?.Rows.Count ?? -1;

        /// <summary> quick to get Result's first row, if Result is null, return null. </summary>
        public DataRow FirstRow => this.Table?.AsEnumerable().FirstOrDefault();

        /// <summary> affected count when query from db. </summary>
        public int AffectedCount { get; private set; }

        /// <summary> quick to check has any affected data in db. </summary>
        public bool HasAffectedData => this.AffectedCount > 0;

        /// <summary> quick to check has any affected schema in db. ( check is <see cref="AffectedCount"/> eqaul to -1 ) </summary>
        public bool HasAffectedSchema => this.AffectedCount == -1;


        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="QueryTable"> result table of query. </param>
        /// <param name="AffectedCount"> affected count when query from db. </param>
        internal DataTableOrNonQueryResult( string ExecuteScript, bool Success, DataTable QueryTable, int AffectedCount )
            : base( Success, null ) {
            this.ExecuteScript = ExecuteScript;
            this.Table = QueryTable;
            this.AffectedCount = AffectedCount;
        } // internal DataTableOrNonQueryResult( string ExecuteScript, bool Success, DataTable QueryTable, int AffectedCount )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal DataTableOrNonQueryResult( string ExecuteScript, Exception Exception )
            : base( Exception ) {
            this.ExecuteScript = ExecuteScript;
        } // internal DataTableOrNonQueryResult( string ExecuteScript, Exception Exception )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="ExecuteScript"> the real script to query from db. </param>
        /// <param name="Success"> is query success. </param>
        /// <param name="QueryTable"> result table of query. </param>
        /// <param name="AffectedCount"> affected count when query from db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        internal DataTableOrNonQueryResult( string ExecuteScript, bool Success, DataTable QueryTable, int AffectedCount, Exception Exception )
            : base( Success, Exception ) {
            this.ExecuteScript = ExecuteScript;
            this.Table = QueryTable;
            this.AffectedCount = AffectedCount;
        } // internal DataTableOrNonQueryResult( string ExecuteScript, bool Success, DataTable QueryTable, int AffectedCount, Exception Exception )

        /// <summary> Convert this object to string, and show more detail. </summary>
        /// <returns> string for more detail. </returns>
        public override string ToString() {
            return $@"DataTableOrNonQueryResult - 
Success:{this.Success.ToString()}
RowCount:{( this.Table == null ? "null" : this.RowCount.ToString() )}
AffectedCount:{AffectedCount}
Script:{this.ExecuteScript}
Exception:{( this.Exception == null ? "null" : this.Exception.ToString() )}";
        } // public override string ToString()
    } // public class DataTableOrNonQueryResult : EmptyResult
} // namespace PGLibrary.Database
