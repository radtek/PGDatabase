using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PGCafe.Object;

namespace PGLibrary.Database {
    /// <summary> Result object when UpdateTable with DBSpool. </summary>
    public class UpdateTableResult : EmptyResult {
        
        /// <summary> Table name to update. </summary>
        public string TableName { get; private set; }
        
        /// <summary> affected count when query update to. </summary>
        public int AffectedCount { get; private set; }

        /// <summary> quick to check has any affected count. </summary>
        public bool HasAnyAffected => this.AffectedCount > 0;


        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="Success"> is query success. </param>
        /// <param name="TableName"> Table name to update. </param>
        /// <param name="AffectedCount"> affected count when update to db. </param>
        public UpdateTableResult( bool Success, string TableName, int AffectedCount )
            : base( Success ) {
            this.TableName = TableName;
            this.AffectedCount = AffectedCount;
        } // public UpdateTableResult( bool Success, string TableName, int AffectedCount )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="TableName"> Table name to update. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        public UpdateTableResult( string TableName, Exception Exception )
            : base( false, Exception ) {
            this.TableName = TableName;
        } // public UpdateTableResult( Exception Exception )
        
        /// <summary> Create object with some initail parameter. </summary>
        /// <param name="Success"> is query success. </param>
        /// <param name="TableName"> Table name to update. </param>
        /// <param name="AffectedCount"> affected count when update to db. </param>
        /// <param name="Exception"> exception object if has any error. </param>
        public UpdateTableResult( bool Success, string TableName, int AffectedCount, Exception Exception )
            : base( Success, Exception ) {
            this.TableName = TableName;
            this.AffectedCount = AffectedCount;
        } // public UpdateTableResult( bool Success, string TableName, int AffectedCount, Exception Exception )
        
        /// <summary> Convert this object to string, and show more detail. </summary>
        /// <returns> string for more detail. </returns>
        public override string ToString() {
            return $@"{this.GetType().Name} - 
Success:{this.Success.ToString()}
TableName:{this.TableName}
AffectedCount:{this.AffectedCount}
Exception:{( this.Exception == null ? "null" : this.Exception.ToString())}";
        } // public override string ToString()

    } // public class UpdateTableResult : EmptyResult
} // namespace PGLibrary.Database
