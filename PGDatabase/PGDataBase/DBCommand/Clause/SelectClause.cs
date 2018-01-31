using System.Collections.Generic;
using System.Linq;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Generate Select clause with DBColumns. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class SelectColumns : IClause {

        #region Property & Constructor

        /// <summary> DBColumns list </summary>
        private List<DBColumn> mColumns = null;

        /// <summary> Initializes a new instance of the <see cref="SelectColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public SelectColumns( IEnumerable<DBColumn> Columns ) {
            this.mColumns = Columns?.ToList();
        } // public SelectColumns( IEnumerable<DBColumn> Columns )

        /// <summary> Initializes a new instance of the <see cref="SelectColumns"/> class with <see cref="DBColumn"/> list. </summary>
        /// <param name="Columns">The Column list.</param>
        public SelectColumns( params DBColumn[] Columns ) : this( (IEnumerable<DBColumn>)Columns ) { }

        #endregion

        #region ISelectClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mColumns.IsNullOrEmpty();

        /// <summary> Generate instance to Select script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> Select script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each Column to [Column-Name] and use comma to join them.
            var result = string.Join( ", ", this.mColumns.Select( item => $"{Provider.Quote( item.Name, QuoteType )}" ) );
            
            return "Select " + result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new SelectColumns( this.mColumns );

        #endregion

    } // internal class SelectColumns : ISelectClause
    
    /// <summary> Generate Select clause with Select script and DBParameters. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class SelectScript : IClause {

        #region Property & Constructor

        /// <summary> Select script </summary>
        private string mScript = null;

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;

        /// <summary> throw exception if parameter is not exact match between mParameters and mScript </summary>
        private bool mThrowExceptionIfParameterNotMatch = true;
        
        /// <summary> Initializes a new instance of the <see cref="SelectScript"/> class with Select script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The Select script.</param>
        /// <param name="Parameters">The parameter list.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SelectScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.mScript = Script;
            this.mParameters = Parameters.ToList();
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public SelectScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Initializes a new instance of the <see cref="SelectScript"/> class with Select script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The Select script.</param>
        /// <param name="Parameters">The parameter list.</param>
        public SelectScript( string Script, params DBParameter[] Parameters ) : this( Script, (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region ISelectClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mScript.IsNullOrWhiteSpace();
        
        /// <summary> Generate instance to Select script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> Select script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            return "Select " + DBParameter.ReplaceParameters( this.mScript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch );
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )
        
        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new SelectScript( this.mScript, this.mParameters );

        #endregion

    } // internal class SelectScript : IClause
    
} // namespace PGLibrary.Database
