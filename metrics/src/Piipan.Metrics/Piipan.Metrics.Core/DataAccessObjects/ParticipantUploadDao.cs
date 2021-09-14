using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Core.Extensions;
using Piipan.Metrics.Api;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public class ParticipantUploadDao : IParticipantUploadDao
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<ParticipantUploadDao> _logger;

        public ParticipantUploadDao(
            IDbConnection dbConnection, 
            ILogger<ParticipantUploadDao> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public Int64 GetUploadCount(string? state)
        {
            var cmd = ParticipantUploadCountQueryCommand(state);

            return (Int64)cmd.ExecuteScalar();
        }

        public IEnumerable<ParticipantUpload> GetUploads(string? state, int limit, int offset = 0)
        {
            var results = new List<ParticipantUpload>();
            var cmd = ParticipantUploadQueryCommand(state, limit, offset);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var record = new ParticipantUpload
                {
                    State = reader[0].ToString(),
                    UploadedAt = Convert.ToDateTime(reader[1])
                };
                results.Add(record);
            }

            return results;
        }

        public IEnumerable<ParticipantUpload> GetLatestUploadsByState()
        {
            var results = new List<ParticipantUpload>();
            var cmd = LatestParticipantUploadByStateQueryCommand();

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var record = new ParticipantUpload
                {
                    State = reader[0].ToString(),
                    UploadedAt = Convert.ToDateTime(reader[1])
                };
                results.Add(record);
            }

            return results;
        }

        public int AddUpload(string state, DateTime uploadedAt)
        {
            var tx = _dbConnection.BeginTransaction();
            var cmd = AddUploadCommand(state, uploadedAt);
            int nRows = cmd.ExecuteNonQuery();
            tx.Commit();
            return nRows;
        }

        private IDbCommand ParticipantUploadCountQueryCommand(string? state)
        {
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            var statement = "SELECT COUNT(*) from participant_uploads";
            if (!String.IsNullOrEmpty(state))
            {
                statement += $" WHERE lower(state) LIKE @state";
            }

            cmd.CommandText = statement;
            if (!String.IsNullOrEmpty(state))
            {
                cmd.AddParameter(DbType.String, "state", state);
            }

            return cmd;
        }

        private IDbCommand ParticipantUploadQueryCommand(string? state, int limit, int offset)
        {
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            var statement = "SELECT state, uploaded_at FROM participant_uploads";
            if (!String.IsNullOrEmpty(state))
                statement += $" WHERE lower(state) LIKE @state";
            statement += " ORDER BY uploaded_at DESC";
            statement += $" LIMIT @limit";
            statement += $" OFFSET @offset";
            
            cmd.CommandText = statement;

            if (!String.IsNullOrEmpty(state))
            {
                cmd.AddParameter(DbType.String, "state", state.ToLower());
            }

            cmd.AddParameter(DbType.Int64, "limit", limit);
            cmd.AddParameter(DbType.Int64, "offset", offset);

            return cmd;
        }

        private IDbCommand LatestParticipantUploadByStateQueryCommand()
        {
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            var statement = @"
                SELECT state, max(uploaded_at) as uploaded_at
                FROM participant_uploads
                GROUP BY state
                ORDER BY uploaded_at ASC
            ;";
            
            cmd.CommandText = statement;

            return cmd;
        }

        private IDbCommand AddUploadCommand(string state, DateTime uploadedAt)
        {
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
                INSERT INTO participant_uploads (state, uploaded_at) 
                VALUES (@state, @uploaded_at)
            ;";

            cmd.AddParameter(DbType.String, "state", state);
            cmd.AddParameter(DbType.DateTime, "uploaded_at", uploadedAt);

            return cmd;
        }
    }
}