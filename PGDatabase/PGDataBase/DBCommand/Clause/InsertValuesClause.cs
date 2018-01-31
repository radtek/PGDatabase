using System.Collections.Generic;
using System.Linq;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Generate InsertValues clause with DBParameters and And condition. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class InsertValuesParameters : IClause {

        #region Property & Constructor

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;

        /// <summary> Initializes a new instance of the <see cref="InsertValuesParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public InsertValuesParameters( IEnumerable<DBParameter> Parameters ) {
            this.mParameters = Parameters?.ToList();
        } // public InsertValuesParameters( IEnumerable<DBParameter> Parameters )

        /// <summary> Initializes a new instance of the <see cref="InsertValuesParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public InsertValuesParameters( params DBParameter[] Parameters ) : this( (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region IInsertValuesClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mParameters.IsNullOrEmpty();

        /// <summary> Generate instance to InsertValues script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> InsertValues script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each parameter to [Column part] and [Value part].
            var columnScript = string.Join( ", ", this.mParameters.Select( item => Provider.Quote( item.Name, QuoteType ) ) );
            var InsertValuescript = string.Join( ", ", this.mParameters.Select( item => Provider.ToDBValueString( item.Value, item.ColumnRule ) ) );

            // combine column part and value part script.
            return $" ( {columnScript} ) Values ( {InsertValuescript} )";
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new InsertValuesParameters( this.mParameters );

        #endregion

    } // internal class InsertValuesParameters : IInsertValuesClause
    
    /// <summary> Generate InsertValues clause with InsertValues script and DBParameters. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class InsertValuesScript : IClause {

        #region Property & Constructor

        /// <summary> InsertValues script </summary>
        private string mColumnScript = null;

        /// <summary> InsertValues script </summary>
        private string mInsertValuescript = null;

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;
        
        /// <summary> throw exception if parameter is not exact match between mParameters and mScript </summary>
        private bool mThrowExceptionIfParameterNotMatch = true;
        
        /// <summary> Initializes a new instance of the <see cref="InsertValuesParameters"/> class with InsertValues script and <see cref="DBParameter"/> list. </summary>
        /// <param name="ColumnScript">The InsertValues's column script.</param>
        /// <param name="InsertValuescript">The InsertValues's value script.</param>
        /// <param name="Parameters">The parameter list.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public InsertValuesScript( string ColumnScript, string InsertValuescript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.mColumnScript = ColumnScript;
            this.mInsertValuescript = InsertValuescript;
            this.mParameters = Parameters.ToList();
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public InsertValuesScript( string ColumnScript, string InsertValuescript, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Initializes a new instance of the <see cref="InsertValuesParameters"/> class with InsertValues script and <see cref="DBParameter"/> list. </summary>
        /// <param name="ColumnScript">The InsertValues's column script.</param>
        /// <param name="InsertValuescript">The InsertValues's value script.</param>
        /// <param name="Parameters">The parameter list.</param>
        public InsertValuesScript( string ColumnScript, string InsertValuescript, params DBParameter[] Parameters )
            : this( ColumnScript, InsertValuescript, (IEnumerable<DBParameter>)Parameters ) { }

        #endregion
        
        #region IInsertValuesClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mColumnScript.IsNullOrWhiteSpace();
        
        /// <summary> Generate instance to InsertValues script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> InsertValues script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            var result = $" ( {DBParameter.ReplaceParameters( this.mColumnScript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch )} )";
            result += $" Values ( {DBParameter.ReplaceParameters( this.mInsertValuescript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch )} )";
            return result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )
        
        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new InsertValuesScript( this.mColumnScript, this.mInsertValuescript, this.mParameters );

        #endregion

    } // internal class InsertValuesScript : IClause

} // namespace PGLibrary.Database
