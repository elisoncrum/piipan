using System;
using System.Collections.Generic;
using System.Data;
using Piipan.Metrics.Core.Extensions;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.DataAccess
{
    public class ParticipantUploadDao : IParticipantUploadDao
    {
        private readonly IDbConnection _dbConnection;

        public ParticipantUploadDao(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public Int64 GetParticipantUploadCount(string? state)
        {
            var cmd = ParticipantUploadCountQueryCommand(state);

            return (Int64)cmd.ExecuteScalar();
        }

        public IEnumerable<ParticipantUpload> GetParticipantUploads(string? state, int limit, int offset = 0)
        {
            var results = new List<ParticipantUpload>();
            var cmd = ParticipantUploadQueryCommand(state, limit, offset);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var record = new ParticipantUpload
                {
                    state = reader[0].ToString(),
                    uploaded_at = Convert.ToDateTime(reader[1])
                };
                results.Add(record);
            }

            return results;
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
    }
}