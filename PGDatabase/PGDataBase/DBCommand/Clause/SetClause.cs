using System.Collections.Generic;
using System.Linq;
using PGCafe;

namespace PGLibrary.Database {

    /// <summary> Generate Set clause with DBParameters and And condition. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class SetParameters : IClause {

        #region Property & Constructor

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;

        /// <summary> Initializes a new instance of the <see cref="SetParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public SetParameters( IEnumerable<DBParameter> Parameters ) {
            this.mParameters = Parameters?.ToList();
        } // public SetParameters( IEnumerable<DBParameter> Parameters )

        /// <summary> Initializes a new instance of the <see cref="SetParameters"/> class with <see cref="DBParameter"/> list. </summary>
        /// <param name="Parameters">The parameter list.</param>
        public SetParameters( params DBParameter[] Parameters ) : this( (IEnumerable<DBParameter>)Parameters ) { }

        #endregion

        #region ISetClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mParameters.IsNullOrEmpty();

        /// <summary> Generate instance to Set script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> Set script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            // if no any clause, return null.
            if ( !this.AnyClause() ) return null;

            // else generate each parameter to [Assign-Expression] and use comma to join them.
            var result = string.Join( ", ", this.mParameters.Select( item => {
                return $"{Provider.Quote( item.Name, QuoteType )} = {Provider.ToDBValueString( item.Value, item.ColumnRule )}";
            } ) );
            
            return " Set " + result;
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )

        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new SetParameters( this.mParameters );

        #endregion

    } // internal class SetParameters : ISetClause
    
    /// <summary> Generate Set clause with Set script and DBParameters. </summary>
    /// <seealso cref="PGLibrary.Database.IClause" />
    internal class SetScript : IClause {

        #region Property & Constructor

        /// <summary> Set script </summary>
        private string mScript = null;

        /// <summary> DBParameters list </summary>
        private List<DBParameter> mParameters = null;
        
        /// <summary> throw exception if parameter is not exact match between mParameters and mScript </summary>
        private bool mThrowExceptionIfParameterNotMatch = true;
        
        /// <summary> Initializes a new instance of the <see cref="SetParameters"/> class with Set script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The Set script.</param>
        /// <param name="Parameters">The parameter list.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public SetScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true ) {
            this.mScript = Script;
            this.mParameters = Parameters.ToList();
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public SetScript( string Script, IEnumerable<DBParameter> Parameters, bool throwExceptionIfParameterNotMatch = true )
        
        /// <summary> Initializes a new instance of the <see cref="SetParameters"/> class with Set script and <see cref="DBParameter"/> list. </summary>
        /// <param name="Script">The Set script.</param>
        /// <param name="Parameters">The parameter list.</param>
        public SetScript( string Script, params DBParameter[] Parameters ) : this( Script, (IEnumerable<DBParameter>)Parameters ) { }

        #endregion
        
        #region ISetClause

        /// <summary> Has any clause in this instance. </summary>
        /// <returns> true if any clause in this instance, otherwise false. </returns>
        public bool AnyClause() => !this.mScript.IsNullOrWhiteSpace();
        
        /// <summary> Generate instance to Set script </summary>
        /// <param name="Provider">The database provider.</param>
        /// <param name="QuoteType">Determin how to quote the word.</param>
        /// <returns> Set script </returns>
        public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType ) {
            return " Set " + DBParameter.ReplaceParameters( this.mScript, this.mParameters, Provider, this.mThrowExceptionIfParameterNotMatch );
        } // public string ToScript( IDatabaseProvider Provider, EQuoteType QuoteType )
        
        /// <summary> Clones this instance. </summary>
        /// <returns> New <see cref="IClause" /> Clone from this instance. </returns>
        public IClause Clone() => new SetScript( this.mScript, this.mParameters );

        #endregion

    } // internal class SetScript : IClause

} // namespace PGLibrary.Database
