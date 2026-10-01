using System.Text.Json;
using InsuranceApp.Api.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Api.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_ReturnsTrueAnd500ProblemDetails()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("Database failed.");

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;

        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(httpContext.Response.Body, JsonOptions);

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("An unexpected error occurred.", problemDetails.Title);
        Assert.Equal("An unexpected error occurred while processing the request.", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_CancelledRequest_ReturnsFalse()
    {
        // Arrange
        var httpContext = CreateHttpContext();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        
        var exception = new OperationCanceledException();

        // Act
        var handled = await _handler.TryHandleAsync(
            httpContext,
            exception,
            cancellationTokenSource.Token);

        // Assert
        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal(0, httpContext.Response.Body.Length);
    }

    [Fact]
    public async Task TryHandleAsync_OperationCanceledWithoutCancelledToken_IsHandledAs500()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var exception = new OperationCanceledException();

        // Act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogsSummarizedError()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("Database failed.");

        // Act
        await _handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Unhandled exception") &&
                    state.ToString()!.Contains("InvalidOperationException") &&
                    state.ToString()!.Contains("Database failed.") &&
                    state.ToString()!.Contains("GET") &&
                    state.ToString()!.Contains("/api/test")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/api/test";
        httpContext.Response.Body = new MemoryStream();

        return httpContext;
    }
}