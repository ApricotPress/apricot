using Apricot.Assets.Sources;

namespace Apricot.Assets;

/// <summary>
/// Reads assets from local file system.
/// </summary>
public class FilesAssetsSource : IAssetsSource, IDisposable
{
    private readonly string _basePath;
    private readonly FileSystemWatcher _watcher;

    private Action<string>? _onAssetChange;

    public string Scheme { get; }

    public event Action<string> OnAssetChange
    {
        add => _onAssetChange += value;
        remove => _onAssetChange -= value;
    }

    public FilesAssetsSource(string scheme, string basePath)
    {
        Scheme = scheme;
        _basePath = Path.GetFullPath(basePath);
        _watcher = new FileSystemWatcher(_basePath);

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Renamed += OnRenamed;
    }

    public IEnumerable<string> ListAssetsPaths(string localPath, ListAssetsType listType) => Directory
        .EnumerateFiles(Path.Join(_basePath, localPath), "*", listType switch
        {
            ListAssetsType.Recursive => SearchOption.AllDirectories,
            ListAssetsType.Direct => SearchOption.TopDirectoryOnly,
            _ => throw new ArgumentOutOfRangeException(nameof(listType), listType, null)
        })
        .Where(p => !Path.GetFileName(p).StartsWith('.'))
        .Select(p => Path.GetRelativePath(_basePath, p));

    public bool Exists(string localPath) => File.Exists(Path.Join(_basePath, localPath));

    public Stream Open(string localPath) => File.OpenRead(Path.Join(_basePath, localPath));

    public void Dispose() => _watcher.Dispose();


    private void OnCreated(object sender, FileSystemEventArgs e) =>
        _onAssetChange?.Invoke(Path.GetRelativePath(_basePath, e.FullPath));

    private void OnChanged(object sender, FileSystemEventArgs e) =>
        _onAssetChange?.Invoke(Path.GetRelativePath(_basePath, e.FullPath));

    private void OnRenamed(object sender, RenamedEventArgs e) =>
        _onAssetChange?.Invoke(Path.GetRelativePath(_basePath, e.FullPath));
}
