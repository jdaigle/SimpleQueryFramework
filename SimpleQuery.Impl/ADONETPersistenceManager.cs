using System;
using System.Data;
using System.Data.SqlClient;

namespace SimpleQuery.Impl {
    public class ADONETPersistenceManager : IPersistenceManager {

        private readonly SqlConnection connection;
        private IDbTransaction transaction;

        public ADONETPersistenceManager(string connectionString) {
            connection = new SqlConnection(connectionString);
        }

        public void Open() {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            if (transaction == null)
                transaction = connection.BeginTransaction();
        }

        public bool IsOpened() {
            return connection.State == ConnectionState.Open && transaction != null;
        }

        public void Rollback() {
            if (transaction != null)
                transaction.Rollback();
            transaction = null;
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }

        public void Commit() {
            if (transaction != null)
                transaction.Commit();
            transaction = null;
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }

        public IDbCommand CreateCommand() {
            Open();
            var command = connection.CreateCommand();
            command.Transaction = (SqlTransaction)transaction;
            return command;
        }
    }
}
