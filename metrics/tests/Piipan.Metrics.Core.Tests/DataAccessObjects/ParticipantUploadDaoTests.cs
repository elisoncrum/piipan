using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Core.DataAccessObjects;
using Moq;
using Xunit;

namespace Piipan.Metrics.Core.Tests.DataAccessObjects
{
    public class ParticipantUploadDaoTests
    {
        private Mock<IDbCommand> _command;
        private string _commandText;
        private Mock<IDbConnection> _connection;
        private Mock<ILogger<ParticipantUploadDao>> _logger;

        [Fact]
        public void GetUploadCount_WithState()
        {
            // Arrange
            var dao = Setup();
            
            _command
                .Setup(m => m.ExecuteScalar())
                .Returns((Int64)5);

            // Act
            var count = dao.GetUploadCount("somestate");

            // Assert
            Assert.Equal(5, count);
            Assert.Contains("SELECT COUNT(*) from participant_uploads", _commandText);
            Assert.Contains("WHERE lower(state) LIKE @state", _commandText);
        }

        [Fact]
        public void GetUploadCount_WithoutState()
        {
            // Arrange
            var dao = Setup();
            
            _command
                .Setup(m => m.ExecuteScalar())
                .Returns((Int64)5);

            // Act
            var count = dao.GetUploadCount(null);

            // Assert
            Assert.Equal(5, count);
            Assert.Contains("SELECT COUNT(*) from participant_uploads", _commandText);
            Assert.DoesNotContain("WHERE", _commandText);
        }

        [Fact]
        public void GetUploads_Single_WithState()
        {
            // Arrange
            var dao = Setup();

            var uploadedAt = DateTime.Now;
            var reader = new Mock<IDataReader>();
            reader
                .SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(false);
            
            reader
                .Setup(m => m[0])
                .Returns("somestate");
            reader
                .Setup(m => m[1])
                .Returns(uploadedAt);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetUploads("somestate", 1, 0);

            // Assert
            Assert.Single(uploads);
            Assert.Single(uploads, (u) => u.state == "somestate" && u.uploaded_at == uploadedAt);
            Assert.Contains("SELECT state, uploaded_at FROM participant_uploads", _commandText);
            Assert.Contains("LIMIT", _commandText);
            Assert.Contains("OFFSET", _commandText);
            Assert.Contains("WHERE lower(state) LIKE @state", _commandText);
        }

        [Fact]
        public void GetUploads_Single_WithoutState()
        {
            // Arrange
            var dao = Setup();

            var uploadedAt = DateTime.Now;
            var reader = new Mock<IDataReader>();
            reader
                .SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(false);
            
            reader
                .Setup(m => m[0])
                .Returns("somestate");
            reader
                .Setup(m => m[1])
                .Returns(uploadedAt);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetUploads(null, 1, 1);

            // Assert
            Assert.Single(uploads);
            Assert.Single(uploads, (u) => u.state == "somestate" && u.uploaded_at == uploadedAt);
            Assert.Contains("SELECT state, uploaded_at FROM participant_uploads", _commandText);
            Assert.Contains("LIMIT", _commandText);
            Assert.Contains("OFFSET", _commandText);
            Assert.DoesNotContain("WHERE", _commandText);
        }

        [Fact]
        public void GetUploads_Multiple_WithoutState()
        {
            // Arrange
            var dao = Setup();

            var firstUploadedAt = DateTime.Now;
            var secondUploadedAt = DateTime.Now + TimeSpan.FromSeconds(5);
            var reader = new Mock<IDataReader>();
            reader
                .SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);
            
            reader
                .SetupSequence(m => m[0])
                .Returns("somestate")
                .Returns("someotherstate");
            reader
                .SetupSequence(m => m[1])
                .Returns(firstUploadedAt)
                .Returns(secondUploadedAt);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetUploads(null, 1, 1);

            // Assert
            Assert.NotEmpty(uploads);
            Assert.Single(uploads, (u) => u.state == "somestate" && u.uploaded_at == firstUploadedAt);
            Assert.Single(uploads, (u) => u.state == "someotherstate" && u.uploaded_at == secondUploadedAt);
            Assert.Contains("SELECT state, uploaded_at FROM participant_uploads", _commandText);
            Assert.Contains("LIMIT", _commandText);
            Assert.Contains("OFFSET", _commandText);
            Assert.DoesNotContain("WHERE", _commandText);
        }

        [Fact]
        public void GetUploads_None()
        {
            // Arrange
            var dao = Setup();

            var reader = new Mock<IDataReader>();
            reader
                .Setup(m => m.Read())
                .Returns(false);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetUploads(null, 1, 0);

            // Assert
            Assert.Empty(uploads);
        }

        [Fact]
        public void GetLatestUploadsByState()
        {
            // Arrange
            var dao = Setup();

            var firstUploadedAt = DateTime.Now;
            var secondUploadedAt = DateTime.Now + TimeSpan.FromSeconds(5);
            var reader = new Mock<IDataReader>();
            reader
                .SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);
            
            reader
                .SetupSequence(m => m[0])
                .Returns("somestate")
                .Returns("someotherstate");
            reader
                .SetupSequence(m => m[1])
                .Returns(firstUploadedAt)
                .Returns(secondUploadedAt);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetLatestUploadsByState();

            // Assert
            Assert.NotEmpty(uploads);
            Assert.Single(uploads, (u) => u.state == "somestate" && u.uploaded_at == firstUploadedAt);
            Assert.Single(uploads, (u) => u.state == "someotherstate" && u.uploaded_at == secondUploadedAt);
            Assert.Contains("SELECT state, max(uploaded_at)", _commandText);
            Assert.Contains("ORDER BY", _commandText);
            Assert.Contains("ASC", _commandText);
            Assert.Contains("GROUP BY", _commandText);
            Assert.DoesNotContain("WHERE", _commandText);
        }

        [Fact]
        public void GetLatestUploadsByState_None()
        {
            // Arrange
            var dao = Setup();

            var reader = new Mock<IDataReader>();
            reader
                .Setup(m => m.Read())
                .Returns(false);

            _command
                .Setup(m => m.ExecuteReader())
                .Returns(reader.Object);

            // Act
            var uploads = dao.GetLatestUploadsByState();

            // Assert
            Assert.Empty(uploads);
        }

        private ParticipantUploadDao Setup()
        {
            _command = new Mock<IDbCommand>();
            _command
                .Setup(m => m.CreateParameter())
                .Returns(Mock.Of<IDbDataParameter>());
            _command
                .Setup(m => m.Parameters)
                .Returns(Mock.Of<IDataParameterCollection>());
            _command
                .SetupSet(m => m.CommandText = It.IsAny<string>())
                .Callback<string>(value => _commandText = value);

            _connection = new Mock<IDbConnection>();
            _connection
                .Setup(m => m.CreateCommand())
                .Returns(_command.Object);
                
            _logger = new Mock<ILogger<ParticipantUploadDao>>();
            return new ParticipantUploadDao(_connection.Object, _logger.Object);
        }
    }
}