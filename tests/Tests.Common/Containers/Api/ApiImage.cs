using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace Tests.Common.Containers.Api;

/// <summary>
/// A test container image for the api.
/// </summary>
public sealed class ApiImage : IImage, IAsyncDisposable
{
    /// <summary>
    /// The http port to which the api will internally bind.
    /// </summary>
    public const ushort HttpPort = 80;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private readonly DockerImage _image = new("localhost/testcontainers", "api", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    /// <summary>
    /// Asynchronously builds the image.
    /// </summary>
    /// <remarks>
    /// Calls to build the api image will lock the resource until the build completes, as a result the same image cannot
    /// be built multiple times concurrently.
    /// </remarks>
    /// <returns>The <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task BuildAsync()
    {
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        try
        {
            await new ImageFromDockerfileBuilder()
                .WithName(this)
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
                .WithDockerfile("tests/Tests.Common/Containers/Api/Dockerfile")
                .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D")) // https://github.com/testcontainers/testcontainers-dotnet/issues/602.
                .WithDeleteIfExists(false)
                .Build()
                .CreateAsync()
                .ConfigureAwait(false);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the repository.
    /// </summary>
    /// <value>The repository</value>
    public string Repository => _image.Repository;

    /// <summary>
    /// Gets the tag.
    /// </summary>
    /// <value>The tag.</value>
    public string Tag => _image.Tag;

    /// <summary>
    /// Gets the full image name.
    /// </summary>
    /// <value>The full image name.</value>
    public string FullName => _image.FullName;

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"/>
    public string Registry => throw new NotImplementedException();

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"/>
    public string Digest => throw new NotImplementedException();

    /// <summary>
    /// Gets the registry hostname.
    /// </summary>
    /// <returns>The registry hostname.</returns>
    public string GetHostname() => _image.GetHostname();

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"/>
    public bool MatchLatestOrNightly() => throw new NotImplementedException();

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"/>
    public bool MatchVersion(Predicate<string> predicate) => throw new NotImplementedException();

    /// <summary>
    /// Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <exception cref="NotImplementedException"/>
    public bool MatchVersion(Predicate<Version> predicate) => throw new NotImplementedException();
}
