using System;
using System.Collections.Generic;
using System.Data;
using Piipan.Metrics.Models;

#nullable enable

namespace Piipan.Metrics.Api.DataAccessObjects
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
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandText = ParticipantUploadCountQueryString(state);
            cmd.CommandType = CommandType.Text;

            return (Int64)cmd.ExecuteScalar();
        }

        public IEnumerable<ParticipantUpload> GetParticipantUploadsForState(string? state, int limit, int offset)
        {
            var results = new List<ParticipantUpload>();
            var cmd = _dbConnection.CreateCommand();
            cmd.CommandText = ParticipantUploadQueryString(state, limit, offset);
            cmd.CommandType = CommandType.Text;

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                results.Add(new ParticipantUpload
                {
                    state = reader[0].ToString(),
                    uploaded_at = Convert.ToDateTime(reader[1])
                });
            }

            return results;
        }

        private string ParticipantUploadCountQueryString(string? state)
        {
            var text = "SELECT COUNT(*) from participant_uploads";
            if (!String.IsNullOrEmpty(state))
                text += $" WHERE lower(state) LIKE '%{state}%'";
            return text;
        }

        private string ParticipantUploadQueryString(string? state, int limit, int offset)
        {
            var statement = "SELECT state, uploaded_at FROM participant_uploads";
            if (!String.IsNullOrEmpty(state))
                statement += $" WHERE lower(state) LIKE '%{state.ToLower()}%'";
            statement += " ORDER BY uploaded_at DESC";
            statement += $" LIMIT {limit}";
            statement += $" OFFSET {offset}";
            return statement;
        }
    }
}