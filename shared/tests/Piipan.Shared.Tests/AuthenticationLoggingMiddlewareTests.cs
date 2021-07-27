using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Piipan.Shared.Logging.Tests
{
    public class AuthenticationLoggingMiddlewareTests
    {
        [Fact]
        public async void InvokeAsync_NewSession()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim> 
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));
            
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);
            
            byte[] val = null;
            var session = new Mock<ISession>();

            session
                .Setup(m => m.Id)
                .Returns("ABCD1234");

            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);
            
            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k,v) => val = v);
            
            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object);

            // Assert
            foreach (var claim in claims)
            {
                logger.Verify(m => m.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"[Session: ABCD1234][CLAIM] {claim.Type}: {claim.Value}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once());
            }
            Assert.NotNull(val);
        }

        [Fact]
        public async void InvokeAsync_ReturningSession()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new AuthenticationLoggingMiddleware(requestDelegate);

            var claims = new List<Claim> 
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3")
            };

            var claimsPrincipal = new ClaimsPrincipal();
            claimsPrincipal.AddIdentity(new ClaimsIdentity(claims));
            
            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.User)
                .Returns(claimsPrincipal);

            byte[] val = BitConverter.GetBytes(1);
            var session = new Mock<ISession>();
            session
                .Setup(m => m.TryGetValue(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, out val))
                .Returns(true);
            
            session
                .Setup(m => m.Set(AuthenticationLoggingMiddleware.CLAIMS_LOGGED_KEY, It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k,v) => val = v);
            
            httpContext
                .Setup(m => m.Session)
                .Returns(session.Object);

            var logger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object);

            // Assert
            logger.Verify(m => m.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains($"[CLAIM]")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never());
        }
    }
}