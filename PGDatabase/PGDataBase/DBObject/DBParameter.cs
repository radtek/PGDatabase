using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PGCafe;

namespace PGLibrary.Database {
    /// <summary> Save parameter's Name and Value to use in DBCommand. </summary>
    public class DBParameter : DBColumn {

        #region Property & Constructor
        
        /// <summary> Parameter's value </summary>
        public object Value { get; private set; }
        
        /// <summary> Create <see cref="DBParameter"/> with DBColumn and Value to use in DBCommand. </summary>
        /// <param name="DBColumn"> Column of parameter </param>
        /// <param name="Value"> Parameter's value </param>
        public DBParameter( DBColumn DBColumn, object Value ) 
            : base( DBColumn.Name, DBColumn.ColumnRule ) {
            this.Value = Value;
        } // public DBParameter( DBColumn DBColumn, object Value )
        
        /// <summary> Create <see cref="DBParameter"/> with Name and Value to use in DBCommand. </summary>
        /// <param name="Name"> Parameter's name </param>
        /// <param name="Value"> Parameter's value </param>
        /// <param name="SpecialRule"> Special rule to use at this parameter. </param>
        public DBParameter( string Name, object Value, EColumnRule SpecialRule = EColumnRule.None ) 
            : base( Name, SpecialRule ) {
            this.Value = Value;
        } // public DBParameter( string Name, object Value, EColumnRule SpecialRule = EColumnRule.None )

        #endregion

        #region ReplaceParameters

        // pattern to match Parameter in script, for one application, it only need one and compile it to speed up when first use.
        private static Regex mMatchParameterPattern = null;

        /// <summary> Generate to sql script in type of <see cref="IDatabaseProvider.DatabaseType"/>. </summary>
        /// <param name="Script">The script to replace by Parameters.</param>
        /// <param name="Parameters"><see cref="DBParameter" /> list to search and replace in Script.</param>
        /// <param name="Provider"><see cref="IDatabaseProvider" /> to support convert.</param>
        /// <param name="throwExceptionIfParameterNotMatch">
        /// if any parameter in parameter list hasn't be used
        /// or any parameter in CommandText hasn't be replace, throw exception. </param>
        /// <returns> script that replace parameter by value. </returns>
        public static string ReplaceParameters( string Script, IEnumerable<DBParameter> Parameters, IDatabaseProvider Provider, bool throwExceptionIfParameterNotMatch ) {

            // if first use, initial it.
            if ( mMatchParameterPattern == null ) {
                mMatchParameterPattern =
                    new Regex( @"(?<Para>:\w*)|(--[^\r\n]*)|((?<stringopen>\').*?(?<stringclose-stringopen>(\'|$)))|((?<commentopen>/\*).*?(?<commentclose-commentopen>(\*/|$)))", RegexOptions.Compiled );
            } // if
            
            // prepare to replace parameter to value.
            var result = new StringBuilder();
            var nextCutStartIndex = 0; // next time need cut string's index, start from zero.
            var usedValueList = new Dictionary<string,string>(); // record the parameter-value has be used in this times.
            var notReplacedParameters = new List<string>();

            Parameters = Parameters.Evaluate();

            // match CommandText.
            var matches = mMatchParameterPattern.Matches( Script );

            // foreach match, replace "Para" to value.
            foreach ( Match match in matches ) {
                // if no match with Para, do nothing.
                var matchToken = match.Groups["Para"];
                if ( !matchToken.Success ) continue ;

                // get parameter name, parameter name should be :Name.  ignore first char ':', just get name after ':'.
                var parameterName = matchToken.Value.Substring( 1 );

                // get db script type value string from parameter list.
                // checked is parameter in used list, if no exist, get it from mParameters and save it to used list.
                if ( !usedValueList.ContainsKey( parameterName ) ) {

                    // if origin parameter list also no this parameter, do not replace, just continue.
                    var find = Parameters.FirstOrDefault( item => item.Name == parameterName );
                    if ( find == null ) {
                        notReplacedParameters.Add( matchToken.Value );
                        continue ;
                    } // if

                    // if has parameter in origin list, convert the value to db script type.
                    string dbValueString = Provider.ToDBValueString( find.Value, find.ColumnRule );

                    // add value to used value list.
                    usedValueList.Add( parameterName, dbValueString );
                } // if

                // cut string from last matchToken's end index to cur matchToken's start index.
                result.Append( Script.Substring( nextCutStartIndex, matchToken.Index - nextCutStartIndex ) );

                // use value to replace Parameter.
                result.Append( usedValueList[parameterName] );

                // set next cut start index is => matchToken's end index.
                nextCutStartIndex = matchToken.Index + matchToken.Length;
            } // foreach

            // end of replace, cur string from last matchToken's end index to the end of CommandText.
            result.Append( Script.Substring( nextCutStartIndex ) );


            // check is any parameter hasn't used in this times convert.
            if ( throwExceptionIfParameterNotMatch && 
                 ( Parameters.Count() != usedValueList.Count || notReplacedParameters.Count > 0 ) ) {
                var notUsedParameters = Parameters.Select( item => item.Name ).Except( usedValueList.Keys );
                throw new DecoderFallbackException( 
$@"Parameters are not match when decoding.
Not used parameters:{string.Join( ",", notUsedParameters )}
Not replaced parameters:{string.Join( ",", notReplacedParameters )}" );
            } // if

            // return result.
            return result.ToString();
        } // public string ToScript( IDatabaseProvider Provider )

        #endregion

        /// <summary> Returns a <see cref="System.String" /> that represents this instance. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance. </returns>
        public override string ToString() {
            return $"{Name}:{Value}";
        } // public override string ToString()

    } // public class DBParameter : DBColumn

} // namespace PGLibrary.Database
