using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace AlbumService.Utils
{

    public class DBHelper
    {
        const string dbProviderName = "System.Data.SqlClient";
        private string _connectionString;
        private bool _stop = false;
        private int _retryCount = 1;
        private int _retryInterval = 1000;
        private int _commandTimeout = 600; // seconds

        /// <summary>
        /// Create a DBHelper instance
        /// </summary>
        /// <param name="connectionString">The connection string for the database.</param>
        /// <param name="retryCount">How many retries on sqlException before throw. <remarks>use -1 for inifinity retry.</remarks></param>
        /// <param name="retryInterval">The time to wait before the next retry (in milliseconds).</param>
        public DBHelper(string connectionString, int retryCount = 1, int retryInterval = 1000)
        {
            _connectionString = connectionString;
            _retryCount = retryCount;
            _retryInterval = retryInterval;
        }

        public void Stop()
        {
            _stop = true;
        }

        #region ExecuteSql
        public DataSet ExecuteDataSet(string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            return ExecuteWithRetry(InnerExecuteDataSet, commandText, dbParams, isPlainText);
        }

        private DataSet InnerExecuteDataSet(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            DbProviderFactory dbfactory = DbProviderFactories.GetFactory(dbProviderName);
            DbDataAdapter dbDataAdapter = dbfactory.CreateDataAdapter();

            using (var cmd = CreateCommand(commandText, dbParams, isPlainText))
            {
                try
                {
                    dbDataAdapter.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    dbDataAdapter.Fill(ds);
                    return ds;
                }
                finally
                {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                    cmd.Connection.Close();
                }
            }
        }

        public DataTable ExecuteDataTable(string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            return ExecuteWithRetry(InnerExecuteDataTable, commandText, dbParams, isPlainText);
        }

        private DataTable InnerExecuteDataTable(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            DbProviderFactory dbfactory = DbProviderFactories.GetFactory(dbProviderName);
            DbDataAdapter dbDataAdapter = dbfactory.CreateDataAdapter();
            using (var cmd = CreateCommand(commandText, dbParams, isPlainText))
            {
                try
                {
                    dbDataAdapter.SelectCommand = cmd;
                    DataTable dataTable = new DataTable();
                    dbDataAdapter.Fill(dataTable);
                    return dataTable;
                }
                finally
                {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                    cmd.Connection.Close();
                }
            }
        }

        public DataTable ExecuteDataTable(DbConnection connection, string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            DbProviderFactory dbfactory = DbProviderFactories.GetFactory(dbProviderName);
            DbDataAdapter dbDataAdapter = dbfactory.CreateDataAdapter();
            using (var cmd = CreateCommand(connection, commandText, dbParams, isPlainText)) {
                try {
                    dbDataAdapter.SelectCommand = cmd;
                    DataTable dataTable = new DataTable();
                    dbDataAdapter.Fill(dataTable);
                    return dataTable;
                }
                finally {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                }
            }
        }

        public DbDataReader ExecuteReader(string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            return ExecuteWithRetry(InnerExecuteReader, commandText, dbParams, isPlainText);
        }

        private DbDataReader InnerExecuteReader(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            DbDataReader reader;
            using (var cmd = CreateCommand(commandText, dbParams, isPlainText))
            {
                try
                {
                    cmd.Connection.Open();
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception)
                {
                    cmd.Connection.Close();
                    throw;
                }
                finally
                {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                }
            }
            return reader;
        }

        public int ExecuteNonQuery(string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            return ExecuteWithRetry(InnerExecuteNonQuery, commandText, dbParams, isPlainText);
        }

        private int InnerExecuteNonQuery(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            using (var cmd = CreateCommand(commandText, dbParams, isPlainText))
            {
                try
                {
                    cmd.Connection.Open();
                    return cmd.ExecuteNonQuery();
                }
                finally
                {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                }
            }
        }

        public int ExecuteNonQuery(DbConnection connection, string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            using (var cmd = CreateCommand(connection, commandText, dbParams, isPlainText)) {
                try {
                    return cmd.ExecuteNonQuery();
                }
                finally {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                }
            }
        }

        public object ExecuteScalar(string commandText, List<SqlParameter> dbParams = null, bool isPlainText = true)
        {
            return ExecuteWithRetry(InnerExecuteScalar, commandText, dbParams, isPlainText);
        }

        private object InnerExecuteScalar(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            using (var cmd = CreateCommand(commandText, dbParams, isPlainText))
            {
                try
                {
                    cmd.Connection.Open();
                    return cmd.ExecuteScalar();
                }
                finally
                {
                    // Have to clear the sql parameters in order to avoid the
                    // "the sql parameter has been assign to another collection"
                    // issue due to retry.
                    cmd.Parameters.Clear();
                    cmd.Connection.Close();
                }
            }
        }

        private T ExecuteWithRetry<T>(
            Func<string, List<SqlParameter>, bool, T> execDbFunc,
            string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    return execDbFunc(commandText, dbParams, isPlainText);
                }
                catch ( SqlException )
                {
                    if (_stop)
                        throw;

                    if (_retryCount != -1 && retryCount++ > _retryCount)
                        throw;

                    Thread.Sleep(_retryInterval);
                }
            }
        }
        #endregion

        private DbCommand CreateCommand(string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            var connection = new SqlConnection(_connectionString);
            return CreateCommand(connection, commandText, dbParams, isPlainText);
        }

        private DbCommand CreateCommand(DbConnection dbConnection, string commandText, List<SqlParameter> dbParams, bool isPlainText)
        {
            DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = isPlainText ? CommandType.Text : CommandType.StoredProcedure;
            dbCommand.CommandTimeout = _commandTimeout;

            if (dbParams != null)
            {
                foreach (var dbParam in dbParams)
                {
                    dbCommand.Parameters.Add(dbParam);
                }
            }

            return dbCommand;
        }

        public SqlParameter CreateSqlParameter(string parameterName, SqlDbType dbType, int? size, object value, ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            var parameter = new SqlParameter(parameterName, dbType);

            if (size != null)
                parameter.Size = size.Value;

            parameter.Direction = parameterDirection;
            if (parameterDirection != ParameterDirection.Output && parameterDirection != ParameterDirection.ReturnValue)
                parameter.Value = value;

            return parameter;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
