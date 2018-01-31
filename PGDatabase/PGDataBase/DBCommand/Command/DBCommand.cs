using System.Collections.Generic;

namespace PGLibrary.Database {

    /// <summary>
    /// Create <see cref="DBCommand"/> with command text and format parameter.
    /// * Parameter format is :ParameterName, EX: Select * From Table Where Key = :Key,
    /// => and add Parameter with Name = "Key", Value = ( value of key )
    /// => Parameter not limited should be string or any type, even the sql function of string type. EX: "getdate()"
    /// => Parameters will inject into CommandText and generate complete script to run.
    /// </summary>
    public class DBCommand : IDBCommand {

        #region Property & Constructor
        
        /// <summary> Parameter list. </summary>
        private Dictionary<string,DBParameter> mParameters = new Dictionary<string, DBParameter>(); // record all parameters.

        /// <summary> throw exception if parameter is not exact match between CommandText and mParameters </summary>
        private bool mThrowExceptionIfParameterNotMatch = true;

        /// <summary> Command text. </summary>
        public string CommandText { get; private set; }

        /// <summary> Create command with command text. </summary>
        /// <param name="CommandText"> command text. </param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        public DBCommand( string CommandText, bool throwExceptionIfParameterNotMatch = true ) {
            this.CommandText = CommandText;
            this.mThrowExceptionIfParameterNotMatch = throwExceptionIfParameterNotMatch;
        } // public DBCommand( string CommandText, bool throwExceptionIfParameterNotMatch = true )

        #endregion

        #region AddParameter

        /// <summary> add the parameter to command. </summary>
        /// <param name="Parameter"> Parameter to add. </param>
        public virtual DBCommand AddParameter( DBParameter Parameter ) {
            this.mParameters[Parameter.Name] = Parameter;
            return this;
        } // public override DBCommand AddParameter( DBParameter Parameter )
        

        /// <summary> add the parameter to command with parameter name and parameter value. </summary>
        /// <param name="Name"> Parameter name. </param>
        /// <param name="Value"> Parameter value </param>
        /// <param name="SpecialRule"> Special rule to use at this parameter </param>
        public virtual DBCommand AddParameter( string Name, object Value, EColumnRule SpecialRule = EColumnRule.None ) {
            this.mParameters[Name] = new DBParameter( Name, Value, SpecialRule );
            return this;
        } // public override DBCommand AddParameter( string Name, object Value, EColumnRule SpecialRule = EColumnRule.None )
        
        
        /// <summary> add the parameters to command. </summary>
        /// <param name="Parameters"> Parameters. </param>
        public virtual DBCommand AddParameter( IEnumerable<DBParameter> Parameters ) {
            foreach ( var item in Parameters ) this.AddParameter( item );
            return this;
        } // public override DBCommand AddParameter( IEnumerable<DBParameter> Parameters )

        #endregion

        #region ToScript
        
        /// <summary> Generate to sql script in type of <see cref="IDatabaseProvider.DatabaseType"/>. </summary>
        /// <param name="Provider">Database provider to get personal setting.</param>
        /// <param name="AddCommandSpliter">No effect with this method.</param>
        /// <returns> script that replace parameter by value. </returns>
        public string ToScript( IDatabaseProvider Provider, bool AddCommandSpliter = false ) 
            => DBParameter.ReplaceParameters( this.CommandText, this.mParameters.Values, Provider, this.mThrowExceptionIfParameterNotMatch );

        #endregion

    } // public class DBCommand : IDBCommand

} // namespace PGLibrary.Database
