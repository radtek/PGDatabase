using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using PGCafe.Object;
using PGCafe;
using System.Collections;
using System.Collections.Generic;
using MayaFramework;

namespace PGLibrary.Database {

    /// <summary>
    /// Proviod method to query DB, need IPGDatabaseProvider with different type DB.
    /// * Not thread safe in multiple thread, each thread should create one object to use.
    /// * Use short connect when query.
    /// </summary>
    public abstract class PGDatabase {

        #region public property

        /// <summary> Connection string to connect to DB. </summary>
        public string ConnectionString { get; }

        /// <summary> Default timeout when query from DB. if not set, use default value of command object </summary>
        public int? DefaultTimeout { get; }

        /// <summary> Database's type. </summary>
        public EDatabaseType DatabaseType => this.Provider?.DatabaseType ?? EDatabaseType.None;

        /// <summary> Provider of DB. </summary>
        public IDatabaseProvider Provider { get; } // provider to get something that different by database type.
        
        /// <summary> Default value of throw exception when error in execute method or not. </summary>
        public bool DefaultThrowException { get; } = false;

        #endregion

        private DbConnection mConnection; // connection object to connect to db.
        private DbTransaction mTransaction; // transaction to use, one object only proviod one transaction.

        /// <summary> get is transaction has using. </summary>
        private bool mTransactionUsing => this.mTransaction != null;

        /// <summary> Initial PGDatabaseBase with ConnectionString, DefaultTimeout. </summary>
        /// <param name="DBProvider"> provider with different database. </param>
        /// <param name="ConnectionString"> Connection string to connect to DB. </param>
        /// <param name="DefaultTimeout"> default timeout when query from DB. set null to use default value of command object. </param>
        /// <param name="DefaultThrowException"> Default value of throw exception when error in execute method or not. </param>
        protected PGDatabase( IDatabaseProvider DBProvider, string ConnectionString, int? DefaultTimeout, bool DefaultThrowException = false ) {
            // check the provider is null or not.
            if ( DBProvider == null )
                throw new ArgumentNullException( nameof( DBProvider ) );

            // create provider.
            this.Provider = DBProvider;

            // Initial
            this.ConnectionString = ConnectionString;
            this.DefaultTimeout = DefaultTimeout;
            this.DefaultThrowException = DefaultThrowException;
        } // public PGDatabase( IDatabaseProvider DBProvider, string ConnectionString, int? DefaultTimeout, bool DefaultThrowException = false )

        /// <summary> Create Command object and set parameter to it. </summary>
        private DbCommand MyCreateCommandObject( string script, int? timeout ) {
            // create command object and set some parameter.
            var cmd = this.Provider.CreateCommandObject();
            cmd.CommandText = script;
            cmd.Connection = this.mConnection;
            cmd.Transaction = this.mTransaction;

            // set timeout, check custom timeout for this times.
            // and then check the default timeout of this PGDatabase Object.
            if ( timeout.HasValue )
                cmd.CommandTimeout = timeout.Value;
            else if ( this.DefaultTimeout.HasValue )
                cmd.CommandTimeout = this.DefaultTimeout.Value;

            // return command object.
            return cmd;
        } // private DbCommand MyCreateCommandObject( string script, int? timeout )

        #region Connection

        /// <summary> try open the connection and throw exception with any error occur. </summary>
        private void OpenConnection() {
            // create connection object first.
            if ( this.mConnection == null ) {
                //mLogger?.Info( "Create Connection Object.  ConnectionString = \"" + ConnectionString + "\"" );
                this.mConnection = this.Provider.CreateConnectionObject();
                this.mConnection.ConnectionString = this.ConnectionString;
            } // if

            // open connection.
            if ( this.mConnection.State == ConnectionState.Closed ) {
                //mLogger?.Info( "Try Open Connection." );
                this.mConnection.Open();
            } // if
        } // private void OpenConnection()

        /// <summary> try close the connection and throw exception with any error occur. </summary>
        private void CloseConnection() {
            // if no connection object, do nothing.
            if ( this.mConnection == null )
                return;

            // if connection is not closed, close it.
            if ( this.mConnection.State != ConnectionState.Closed ) {
                //mLogger?.Info( "Try Open Connection." );
                this.mConnection.Close();
            } // if
        } // private void CloseConnection()

        #endregion

        #region Transaction

        /// <summary> Begin a transaction. </summary>
        /// <param name="IsolationLevel"> set transaction's level, pass null to use default value. </param>
        /// <param name="throwException"> throw exception if transaction failed and has any exception occur, without return Transaction Result. </param>
        /// <returns> TransactionResult to record success or exception if error. </returns>
        public EmptyResult TransactionBegin( IsolationLevel? IsolationLevel = null, bool? throwException = null ) {
            try {
                if ( this.mTransaction != null )
                    throw new NotSupportedException( "Transaction 正在使用中" );

                this.OpenConnection();
                //mLogger?.Info( "Try Open Transaction with IsolationLevel = " + ( IsolationLevel?.ToString() ?? "default" ) );
                if ( IsolationLevel.HasValue ) // 如果有指定 IsolationLevel 則傳給 BeginTransaction
                    this.mTransaction = this.mConnection.BeginTransaction( IsolationLevel.Value );
                else // 否則使用預設值
                    this.mTransaction = this.mConnection.BeginTransaction();

                return true;
            } catch ( Exception ex ) {

                try { // try close connection.
                    this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with begin transaction and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when Open Transaction", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException ) throw exception;
                    else return exception;
                } // try-catch

                //mLogger?.Error( "Error occur when Open Transaction", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException ) throw;
                else return ex;
            } // try-catch
        } // public EmptyResult TransactionBegin( IsolationLevel? IsolationLevel = null, bool? throwException = null )

        /// <summary> Commit the transaction, will auto rollback if commit failed. </summary>
        /// <param name="throwException"> throw exception if transaction failed and has any exception occur, without return Transaction Result. </param>
        /// <returns> TransactionResult to record success and exception if error. </returns>
        public EmptyResult TransactionCommit( bool? throwException = null ) {
            try {
                if ( this.mTransaction == null )
                    throw new NotSupportedException( "尚未開啟 Transaction" );

                // try commit and close connection if commit success.
                //mLogger?.Info( "Try Commit Transaction" );
                this.mTransaction.Commit();

                try { // try close connection.
                    this.CloseConnection();
                } catch ( Exception ex ) {
                    //mLogger?.Error( "Error occur when Commit Transaction", ex );

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new EmptyResult( true, ex );
                } // try-catch

                return true;
            } catch ( Exception ex ) {

                // when commit error, auot rollback
                var result = this.TransactionRollback();
                if ( !result.Success ) { // if roolback error, return multiple exception.
                    var exception = new MultipleException( "Commit 與自動 Rollback 失敗", new[] { ex, result.Exception } );
                    //mLogger?.Error( "Error occur when Commit Transaction", exception );

                    // if need throw exception, throw it.  else throw it.
                    if ( throwException ?? this.DefaultThrowException ) throw exception;
                    else return exception;
                } // if

                // if rollback success, return commit exception.
                //mLogger?.Error( "Error occur when Commit Transaction", ex );

                // if need throw exception, throw it.  else throw it.
                if ( throwException ?? this.DefaultThrowException ) throw;
                else return ex;
            } finally {
                this.mTransaction = null; // set variable to null to use when next TransactionBegin.
            } // try-catch-finally
        } // public EmptyResult TransactionCommit( bool? throwException = null )

        /// <summary> Rollback the transaction. </summary>
        /// <param name="throwException"> throw exception if transaction failed and has any exception occur, without return Transaction Result. </param>
        /// <returns> TransactionResult to record success and exception if error. </returns>
        public EmptyResult TransactionRollback( bool? throwException = null ) {
            try { // try rollback if has transaction.
                if ( !this.mTransactionUsing )
                    throw new NotSupportedException( "尚未開啟 Transaction" );

                //mLogger?.Info( "Try Rollback Transaction" );
                this.mTransaction.Rollback();

                try { // try close connection.
                    this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when Rollback Transaction", ex );
                    return new EmptyResult( true, ex );
                } // try-catch

                // if close connection success, just return success result.
                return true;
            } catch ( Exception ex ) {

                try { // try close connection.
                    this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with rollback and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when Rollback Transaction", exception );

                    // if need throw exception, throw it.  else throw it.
                    if ( throwException ?? this.DefaultThrowException ) throw exception;
                    else return exception;
                } // try-catch

                // if close connection success, just return rollback exception result.
                //mLogger?.Error( "Error occur when Rollback Transaction", ex );

                // if need throw exception, throw it.  else throw it.
                if ( throwException ?? this.DefaultThrowException ) throw;
                else return ex;
            } finally { // wheather success or fail, set transaction variable to null.
                this.mTransaction = null; // set variable to null to use when next TransactionBegin.
            } // try-catch-finally
        } // public EmptyResult TransactionRollback( bool? throwException = null )
        
        /// <summary> Execute expression when transaction, auto commit when all is success, rollback if has any error. </summary>
        /// <param name="ExecuteFunction"> any expression to execute. </param>
        /// <param name="IsolationLevel"> set transaction's level, pass null to use default value. </param>
        /// <param name="throwException"> throw exception if transaction failed and has any exception occur, without return Transaction Result. </param>
        /// <returns> true if no any error occur. </returns>
        public EmptyResult TransactionExecute( Action ExecuteFunction, IsolationLevel? IsolationLevel = null, bool? throwException = null ) {
            return TransactionExecute( () => {
                ExecuteFunction();
                return true;
            }, IsolationLevel, throwException );
        } // public EmptyResult TransactionExecute( Action ExecuteFunction, IsolationLevel? IsolationLevel = null, bool? throwException = null )

        /// <summary> Execute expression when transaction, auto commit when all is success, rollback if has any error. </summary>
        /// <param name="ExecuteFunction"> any expression to execute. </param>
        /// <param name="IsolationLevel"> set transaction's level, pass null to use default value. </param>
        /// <param name="throwException"> throw exception if transaction failed and has any exception occur, without return Transaction Result. </param>
        /// <returns> TResult of ExecuteFunction's result. </returns>
        public SingleResult<TResult> TransactionExecute<TResult>( Func<TResult> ExecuteFunction, IsolationLevel? IsolationLevel = null, bool? throwException = null ) {
            try {
                this.TransactionBegin( IsolationLevel, true );
                var result = ExecuteFunction();
                
                try { // if commit fail, will auto rollback in commit funcion.
                    this.TransactionCommit( true );
                } catch ( Exception ex ) {
                    // if need throw exception, throw it.  else throw it.
                    if ( throwException ?? this.DefaultThrowException ) throw;
                    else return ex;
                } // try-catch

                return result;
            } catch ( Exception ex ) {

                try { // try rollback transaction.
                    this.TransactionRollback( true );
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and rollback.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when Rollback Transaction", exception );

                    // if need throw exception, throw it.  else throw it.
                    if ( throwException ?? this.DefaultThrowException ) throw exception;
                    else return exception;
                } // try-catch
                
                // if need throw exception, throw it.  else throw it.
                if ( throwException ?? this.DefaultThrowException ) throw;
                else return ex;
            } // try-catch

        } // public SingleResult<TResult> TransactionExecute<TResult>( Func<TResult> ExecuteFunction, IsolationLevel? IsolationLevel = null, bool? throwException = null )




        /// <summary> Begin a transaction. </summary>
        /// <param name="IsolationLevel"> set transaction's level, pass null to use default value. </param>
        /// <returns> TransactionResult to record success or exception if error. </returns>
        public EmptyResult TransactionBeginShowMsgIfError( IsolationLevel? IsolationLevel = null ) {
            var result = this.TransactionBegin( IsolationLevel, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "TransactionBegin failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public EmptyResult TransactionBeginShowMsgIfError( IsolationLevel? IsolationLevel = null )

        /// <summary> Commit the transaction, will auto rollback if commit failed. </summary>
        /// <returns> TransactionResult to record success and exception if error. </returns>
        public EmptyResult TransactionCommitShowMsgIfError() {
            var result = this.TransactionCommit( false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "TransactionCommit failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public EmptyResult TransactionCommitShowMsgIfError()

        /// <summary> Rollback the transaction. </summary>
        /// <returns> TransactionResult to record success and exception if error. </returns>
        public EmptyResult TransactionRollbackShowMsgIfError() {
            var result = this.TransactionRollback( false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "TransactionRollback failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public EmptyResult TransactionRollbackShowMsgIfError()
        
        /// <summary> Execute expression when transaction, auto commit when all is success, rollback if has any error. </summary>
        /// <param name="ExecuteFunction"> any expression to execute. </param>
        /// <param name="IsolationLevel"> set transaction's level, pass null to use default value. </param>
        /// <returns> TResult of ExecuteFunction's result. </returns>
        public SingleResult<TResult> TransactionExecuteShowMsgIfError<TResult>( Func<TResult> ExecuteFunction, IsolationLevel? IsolationLevel = null ) {
            var result = this.TransactionExecute( ExecuteFunction, IsolationLevel, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "TransactionExecute failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public SingleResult<TResult> TransactionExecuteShowMsgIfError<TResult>( Func<TResult> ExecuteFunction, IsolationLevel? IsolationLevel = null )

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Execute non query script.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> NonQueryResult to record success and exception. </returns>
        public NonQueryResult ExecuteNonQuery( string script, int? timeout = null, bool? throwException = null ) {
            try {
                //mLogger?.Info( $"ExecuteNonQuery with timeout = {timeout}, script = {script}" );
                this.OpenConnection();

                // create and execute command.
                var cmd = this.MyCreateCommandObject( script, timeout );
                var affectedCount = cmd.ExecuteNonQuery();

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when ExecuteNonQuery", ex );

                    var success = affectedCount != -1;

                    // if not success and need throw exception, throw it.
                    if ( !success && ( throwException ?? this.DefaultThrowException ) )
                        throw;

                    return new NonQueryResult( script, success, affectedCount, ex );
                } // try-catch

                // if close connection success, return the query result.
                // if affectedCount == -1, means has error occur when ExecuteNonQuery.
                return new NonQueryResult( script, affectedCount != -1, affectedCount );
            } catch ( Exception ex ) {

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when ExecuteNonQuery", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException )
                        throw exception;

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new NonQueryResult( script, exception );
                } // try-catch

                // if close connection success, just return execute exception result.
                //mLogger?.Error( "Error occur when ExecuteNonQuery", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException )
                    throw;

                return new NonQueryResult( script, ex );
            } // try-catch
        } // public NonQueryResult ExecuteNonQuery( string script, int? timeout = null, bool? throwException = null )



        /// <summary>
        /// Execute non query command.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> NonQueryResult to record success and exception. </returns>
        public NonQueryResult ExecuteNonQuery( IDBCommand command, int? timeout = null, bool? throwException = null ) {
            return this.ExecuteNonQuery( command.ToScript( this.Provider ), timeout, throwException );
        } // public NonQueryResult ExecuteNonQuery( IDBCommand command, int? timeout = null, bool? throwException = null )

        /// <summary>
        /// Execute non query script.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> NonQueryResult to record success and exception. </returns>
        public NonQueryResult ExecuteNonQueryShowMsgIfError( string script, int? timeout = null ) {
            var result = this.ExecuteNonQuery( script, timeout, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "ExecuteNonQuery failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public NonQueryResult ExecuteNonQueryShowMsgIfError( string script, int? timeout = null )

        /// <summary>
        /// Execute non query command.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> NonQueryResult to record success and exception. </returns>
        public NonQueryResult ExecuteNonQueryShowMsgIfError( IDBCommand command, int? timeout = null ) {
            return this.ExecuteNonQueryShowMsgIfError( command.ToScript( this.Provider ), timeout );
        } // public NonQueryResult ExecuteNonQueryShowMsgIfError( IDBCommand command, int? timeout = null )

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Execute script to get first row, first column's value.
        /// * Use ConvertExtension.ToType&lt;T&gt; to convert result to type T.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> ScalarResult to record success and exception. </returns>
        public ScalarResult<T> ExecuteScalar<T>( string script, int? timeout = null, bool? throwException = null ) {
            try {
                //mLogger?.Info( $"ExecuteScalar with T = {typeof( T ).Name}, timeout = {timeout}, script = {script}" );
                this.OpenConnection();

                // create and execute command.
                var cmd = this.MyCreateCommandObject( script, timeout );
                var resultObj = cmd.ExecuteScalar();
                var resultValue = resultObj.ToType<T>();

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when ExecuteScalar", ex );
                    return new ScalarResult<T>( script, true, resultValue, ex );
                } // try-catch

                // if close connection success, return the query result.
                // * resultObj == null, means no query result with script, doesn't means has error of script.
                // => so always return success if no exception bean throw.
                return new ScalarResult<T>( script, true, resultValue );
            } catch ( Exception ex ) {

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when ExecuteScalar", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException )
                        throw exception;

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new ScalarResult<T>( script, exception );
                } // try-catch

                // if close connection success, just return execute exception result.
                //mLogger?.Error( "Error occur when ExecuteScalar", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException )
                    throw;

                return new ScalarResult<T>( script, ex );
            } // try-catch
        } // public ScalarResult<T> ExecuteScalar<T>( string script, int? timeout = null, bool? throwException = null )



        /// <summary>
        /// Execute script to get first row, first column's value.
        /// * Use ConvertExtension.ToType&lt;T&gt; to convert result to type T.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> ScalarResult to record success and exception. </returns>
        public ScalarResult<T> ExecuteScalar<T>( IDBCommand command, int? timeout = null, bool? throwException = null ) {
            return this.ExecuteScalar<T>( command.ToScript( this.Provider ), timeout, throwException );
        } // public ScalarResult<T> ExecuteScalar<T>( IDBCommand command, int? timeout = null, bool? throwException = null )

        /// <summary>
        /// Execute script to get first row, first column's value.
        /// * Use ConvertExtension.ToType&lt;T&gt; to convert result to type T.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> ScalarResult to record success and exception. </returns>
        public ScalarResult<T> ExecuteScalarShowMsgIfError<T>( string script, int? timeout = null ) {
            var result = this.ExecuteScalar<T>( script, timeout, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "ExecuteScalar failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public ScalarResult<T> ExecuteScalarShowMsgIfError<T>( string script, int? timeout = null )

        /// <summary>
        /// Execute script to get first row, first column's value.
        /// * Use ConvertExtension.ToType&lt;T&gt; to convert result to type T.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> ScalarResult to record success and exception. </returns>
        public ScalarResult<T> ExecuteScalarShowMsgIfError<T>( IDBCommand command, int? timeout = null ) {
            return this.ExecuteScalarShowMsgIfError<T>( command.ToScript( this.Provider ), timeout );
        } // public ScalarResult<T> ExecuteScalarShowMsgIfError<T>( IDBCommand command, int? timeout = null )

        #endregion

        #region ExecuteDataTable

        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> DataTableResult to record success and exception. </returns>
        public DataTableResult ExecuteDataTable( string script, int? timeout = null, bool? throwException = null ) {
            try {
                //mLogger?.Info( $"ExecuteDataTable timeout = {timeout}, script = {script}" );
                this.OpenConnection();

                // should execute reader befor new DataTable.( use the DataTable has created to checked is execute reader failed.
                var cmd = this.MyCreateCommandObject( script, timeout );
                var reader = cmd.ExecuteReader();

                // create DataTable and load data from reader.
                DataTable result = new DataTable();
                result.Load( reader );

                try { // try close reader and connection if not using transaction.
                    reader.Close();
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when ExecuteDataTable", ex );
                    
                    // if execute success and close connection error, return both true and exception.
                    return new DataTableResult( script, true, result, ex );
                } // try-catch

                // if close connection success, return the query result.
                return new DataTableResult( script, true, result );
            } catch ( Exception ex ) {

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when ExecuteDataTable", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException )
                        throw exception;

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new DataTableResult( script, exception );
                } // try-catch

                // if close connection success, just return execute exception result.
                //mLogger?.Error( "Error occur when ExecuteDataTable", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException )
                    throw;

                return new DataTableResult( script, ex );
            } // try-catch
        } // public DataTableResult ExecuteDataTable( string script, int? timeout = null, bool? throwException = null )



        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> DataTableResult to record success and exception. </returns>
        public DataTableResult ExecuteDataTable( IDBCommand command, int? timeout = null, bool? throwException = null ) {
            return this.ExecuteDataTable( command.ToScript( this.Provider ), timeout, throwException );
        } // public DataTableResult ExecuteDataTable( IDBCommand command, int? timeout = null, bool? throwException = null )


        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> DataTableResult to record success and exception. </returns>
        public DataTableResult ExecuteDataTableShowMsgIfError( string script, int? timeout = null ) {
            var result = this.ExecuteDataTable( script, timeout, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "ExecuteDataTable failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public DataTableResult ExecuteDataTableShowMsgIfError( string script, int? timeout = null )

        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> DataTableResult to record success and exception. </returns>
        public DataTableResult ExecuteDataTableShowMsgIfError( DBCommand command, int? timeout = null ) {
            return this.ExecuteDataTableShowMsgIfError( command.ToScript( this.Provider ), timeout );
        } // public DataTableResult ExecuteDataTableShowMsgIfError( PGDatabaseCommand command, int? timeout = null )

        #endregion

        #region UpdateTable
        
        /// <summary>
        /// Update data rows to DB.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="TableName"> assign DB's Table to update to it. </param>
        /// <param name="DataRows"> data rows to update to DB. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> UpdateTableResult to record success and exception. </returns>
        public UpdateTableResult UpdateTable( string TableName, DataRow[] DataRows, int? timeout = null, bool? throwException = null ) {
            // vaild argument.
            if ( DataRows == null )
                throw new ArgumentException( $"{nameof( DataRows )} can't not be null." );

            try {
                //mLogger?.Info( $"UpdateTable with TableName = {TableName}, timeout = {timeout}, DataRowCount = {DataRows.Length}" );
                this.OpenConnection();

                // create object.
                var adapter = this.Provider.CreateDataAdapter();
                adapter.SelectCommand = this.MyCreateCommandObject( "Select * From " + TableName, timeout );
                var builder = this.Provider.CreateCommandBuilder();
                builder.QuotePrefix = this.Provider.QuotePrefix;
                builder.QuoteSuffix = this.Provider.QuoteSuffix;
                builder.DataAdapter = adapter;

                // create command.
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();

                // set transaction.
                adapter.InsertCommand.Transaction = this.mTransaction;
                adapter.UpdateCommand.Transaction = this.mTransaction;
                adapter.DeleteCommand.Transaction = this.mTransaction;

                var affectedCount = adapter.Update( DataRows );

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when UpdateTable", ex );

                    var success = affectedCount != -1;

                    // if not success and need throw exception, throw it.
                    if ( !success && ( throwException ?? this.DefaultThrowException ) )
                        throw;

                    return new UpdateTableResult( success, TableName, affectedCount, ex );
                } // try-catch

                // if close connection success, return the query result.
                // if affectedCount == -1, means has error occur when ExecuteNonQuery.
                return new UpdateTableResult( affectedCount != -1, TableName, affectedCount );
            } catch ( Exception ex ) {

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when ExecuteDataTable", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException )
                        throw exception;

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new UpdateTableResult( TableName, exception );
                } // try-catch

                // if close connection success, just return execute exception result.
                //mLogger?.Error( "Error occur when ExecuteDataTable", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException )
                    throw;

                return new UpdateTableResult( TableName, ex );
            } // try-catch
        } // public UpdateTableResult UpdateTable( string TableName, DataRow[] DataRows, int? timeout = null, bool? throwException = null )


        /// <summary>
        /// Update datatable to DB.
        /// * set DataTable.TableName to assign DB's Table.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="Table"> DataTable to update to DB. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> UpdateTableResult to record success and exception. </returns>
        public UpdateTableResult UpdateTable( DataTable Table, int? timeout = null, bool? throwException = null ) {
            // vaild argument.
            if ( Table == null )
                throw new ArgumentException( $"{nameof( Table )} can't not be null." );

            return this.UpdateTable( Table.TableName, Table.AsEnumerable().ToArray(), timeout, throwException );
        } // public UpdateTableResult UpdateTable( DataTable Table, int? timeout = null, bool? throwException = null )


        /// <summary>
        /// Update data rows to DB.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="TableName"> assign DB's Table to update to it. </param>
        /// <param name="DataRows"> data rows to update to DB. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> UpdateTableResult to record success and exception. </returns>
        public UpdateTableResult UpdateTableShowMsgIfError( string TableName, DataRow[] DataRows, int? timeout = null ) {
            var result = this.UpdateTable( TableName, DataRows, timeout, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "UpdateTable failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public UpdateTableResult UpdateTableShowMsgIfError( string TableName, DataRow[] DataRows, int? timeout = null )

        /// <summary>
        /// Update datatable to DB.
        /// * set DataTable.TableName to assign DB's Table.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="Table"> DataTable to update to DB. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> UpdateTableResult to record success and exception. </returns>
        public UpdateTableResult UpdateTableShowMsgIfError( DataTable Table, int? timeout = null ) {
            return this.UpdateTableShowMsgIfError( Table.TableName, Table.AsEnumerable().ToArray(), timeout );
        } // public UpdateTableResult UpdateTableShowMsgIfError( DataTable Table, int? timeout = null )

        #endregion
        
        #region ExecuteDataTableOrNonQueryResult

        /// <summary>
        /// Execute script to get data table or non query.( ex: both select or insert command can be execute )
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> DataTableOrNonQueryResult to record success and exception. </returns>
        public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResult( string script, int? timeout = null, bool? throwException = null ) {
            try {
                //mLogger?.Info( $"ExecuteDataTableOrNonQueryResult timeout = {timeout}, script = {script}" );
                this.OpenConnection();

                // should execute reader befor new DataTable.( use the DataTable has created to checked is execute reader failed.
                var cmd = this.MyCreateCommandObject( script, timeout );
                var reader = cmd.ExecuteReader();
                
                // create DataTable and load data from reader.
                DataTable queryTable = null;
                if ( reader.FieldCount > 0 ){
                    queryTable = new DataTable();
                    queryTable.Load( reader );
                } // if

                var affectedCount = reader.RecordsAffected;

                try { // try close reader and connection if not using transaction.
                    reader.Close();
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex ) {
                    // if no exception when query, but exception when close connection, return both success and exception.
                    //mLogger?.Error( "Error occur when ExecuteDataTableOrNonQueryResult", ex );
                    
                    // if execute success and close connection error, return both true and exception.
                    return new DataTableOrNonQueryResult( script, true, queryTable, affectedCount, ex );
                } // try-catch

                // if close connection success, return the query result.
                return new DataTableOrNonQueryResult( script, true, queryTable, affectedCount );
            } catch ( Exception ex ) {

                try { // try close connection if not using transaction.
                    if ( !this.mTransactionUsing )
                        this.CloseConnection();
                } catch ( Exception ex2 ) {
                    var exception = new MultipleException( "error occur with execute and close connection.", new[] { ex, ex2 } );
                    //mLogger?.Error( "Error occur when ExecuteDataTableOrNonQueryResult", exception );

                    // if need throw exception, throw it.
                    if ( throwException ?? this.DefaultThrowException )
                        throw exception;

                    // if no exception when query, but exception when close connection, return both success and exception.
                    return new DataTableOrNonQueryResult( script, exception );
                } // try-catch

                // if close connection success, just return execute exception result.
                //mLogger?.Error( "Error occur when ExecuteDataTableOrNonQueryResult", ex );

                // if need throw exception, throw it.
                if ( throwException ?? this.DefaultThrowException )
                    throw;

                return new DataTableOrNonQueryResult( script, ex );
            } // try-catch
        } // public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResult( string script, int? timeout = null, bool? throwException = null )



        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        /// <returns> DataTableOrNonQueryResult to record success and exception. </returns>
        public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResult( IDBCommand command, int? timeout = null, bool? throwException = null ) {
            return this.ExecuteDataTableOrNonQueryResult( command.ToScript( this.Provider ), timeout, throwException );
        } // public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResult( IDBCommand command, int? timeout = null, bool? throwException = null )


        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="script"> script to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> DataTableOrNonQueryResult to record success and exception. </returns>
        public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResultShowMsgIfError( string script, int? timeout = null ) {
            var result = this.ExecuteDataTableOrNonQueryResult( script, timeout, false );

            // if failed, show error.
            if ( !result.Success )
                PGMessageBox.Show( "ExecuteDataTableOrNonQueryResult failed:\n" + result.ToString(), MsgType.Err );

            return result;
        } // public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResultShowMsgIfError( string script, int? timeout = null )

        /// <summary>
        /// Execute script to get data table.
        /// * Use transaction if has call TransactionBegin() before.
        /// * Show MessageBox only when execute failed.
        /// </summary>
        /// <param name="command"> command to execute. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <returns> DataTableOrNonQueryResult to record success and exception. </returns>
        public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResultShowMsgIfError( DBCommand command, int? timeout = null ) {
            return this.ExecuteDataTableOrNonQueryResultShowMsgIfError( command.ToScript( this.Provider ), timeout );
        } // public DataTableOrNonQueryResult ExecuteDataTableOrNonQueryResultShowMsgIfError( PGDatabaseCommand command, int? timeout = null )

        #endregion

        #region Select Schema
        
        /// <summary>
        /// Select all table's schema in database
        /// * Only support database type which PGDatabase has implements,
        ///   if the database type is custom, won't support in this method,
        ///   if so, please create call it from custom TableSchema class which inherit from BaseTableSchema.
        /// </summary>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<ITableSchema>> SelectSchema( int? timeout = null, bool? throwException = null )
            => this.Provider.SelectSchema( this, timeout, throwException );
        
        
        /// <summary>
        /// Select specific table's schema in database
        /// * Only support database type which PGDatabase has implements,
        ///   if the database type is custom, won't support in this method,
        ///   if so, please create call it from custom TableSchema class which inherit from BaseTableSchema.
        /// </summary>
        /// <param name="TableNames"> table names to get schema. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<ITableSchema>> SelectSchema( string[] TableNames, int? timeout = null, bool? throwException = null )
            => this.Provider.SelectSchema( this, TableNames, timeout, throwException );
        
        
        /// <summary>
        /// Select specific table's schema in database
        /// * Only support database type which PGDatabase has implements,
        ///   if the database type is custom, won't support in this method,
        ///   if so, please create call it from custom TableSchema class which inherit from BaseTableSchema.
        /// </summary>
        /// <param name="TableName"> table name to get schema. </param>
        /// <param name="timeout"> set custom time out of this times execute, set null to use default timeout. </param>
        /// <param name="throwException"> throw exception if query failed and has any exception occur, without return Query Result. </param>
        public SingleResult<IEnumerable<ITableSchema>> SelectSchema( string TableName, int? timeout = null, bool? throwException = null )
            => this.Provider.SelectSchema( this, TableName, timeout, throwException );

        #endregion

        #region Static Method

        /// <summary>
        /// Initial PGDatabase by DatabaseType with ConnectionString, DefaultTimeout.
        /// * Only support database type which PGDatabase has implements,
        ///   if the database type is custom, won't support in this method,
        ///   if so, please create it self.
        /// </summary>
        /// <param name="DatabaseType"> Database type to create. </param>
        /// <param name="ConnectionString"> Connection string to connect to DB. </param>
        /// <param name="DefaultTimeout"> default timeout when query from DB. set null to use default value of command object. </param>
        /// <param name="DefaultThrowException"> Default value of throw exception when error in execute method or not. </param>
        public static PGDatabase Create( EDatabaseType DatabaseType, string ConnectionString, int? DefaultTimeout = null, bool DefaultThrowException = false ) {
            if ( DatabaseType == EDatabaseType.SQLServer )
                return new SQLServerDatabase( ConnectionString, DefaultTimeout, DefaultThrowException );
            else if ( DatabaseType == EDatabaseType.Oracle )
                return new OracleDatabase( ConnectionString, DefaultTimeout, DefaultThrowException );
            else if ( DatabaseType == EDatabaseType.MySql )
                return new MySqlDatabase( ConnectionString, DefaultTimeout, DefaultThrowException );
            else if ( DatabaseType == EDatabaseType.Access )
                return new AccessDatabase( ConnectionString, DefaultTimeout, DefaultThrowException );
            //else if ( DatabaseType == EDatabaseType.DB2 )
            //    return new DB2Database( ConnectionString, DefaultTimeout, DefaultThrowException );
            else return null;
        } // public static PGDatabase Create( EDatabaseType DatabaseType, string ConnectionString, int? DefaultTimeout = null, bool DefaultThrowException = false )

        #endregion

    } // public abstract class PGDatabase

} // namespace PGLibrary.Database
