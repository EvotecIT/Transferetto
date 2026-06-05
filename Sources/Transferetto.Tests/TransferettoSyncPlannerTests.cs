using Transferetto;

namespace Transferetto.Tests;

public sealed class TransferettoSyncPlannerTests {
    [Fact]
    public void PlannerUploadsMissingAndChangedFilesThenMirrorDeletesExtras() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry[] source = {
            Directory("assets", @"C:\site\assets", "/wwwroot/assets"),
            File("assets/app.css", @"C:\site\assets\app.css", "/wwwroot/assets/app.css", 10, now),
            File("index.html", @"C:\site\index.html", "/wwwroot/index.html", 20, now),
            File("current.txt", @"C:\site\current.txt", "/wwwroot/current.txt", 30, now)
        };
        TransferettoSyncEntry[] destination = {
            File("index.html", null, "/wwwroot/index.html", 15, now),
            File("current.txt", null, "/wwwroot/current.txt", 30, now),
            File("old.txt", null, "/wwwroot/old.txt", 5, now)
        };

        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            source,
            destination,
            new TransferettoSyncOptions {
                Direction = TransferettoSyncDirection.Upload,
                Mode = TransferettoSyncMode.Mirror,
                Comparison = TransferettoSyncComparison.Size
            });

        Assert.Contains(plan, item => item.Action == TransferettoSyncAction.CreateDirectory && item.RelativePath == "assets");
        Assert.Contains(plan, item => item.Action == TransferettoSyncAction.UploadFile && item.RelativePath == "assets/app.css");
        Assert.Contains(plan, item => item.Action == TransferettoSyncAction.UploadFile && item.RelativePath == "index.html");
        Assert.Contains(plan, item => item.Action == TransferettoSyncAction.Skip && item.RelativePath == "current.txt");
        Assert.Equal(TransferettoSyncAction.DeleteRemoteFile, plan.Last().Action);
        Assert.Equal("old.txt", plan.Last().RelativePath);
    }

    [Fact]
    public void PlannerDownloadsRemoteSourceWhenDirectionIsDownload() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry[] remoteSource = {
            File("exports/report.csv", null, "/exports/report.csv", 100, now)
        };

        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            remoteSource,
            Array.Empty<TransferettoSyncEntry>(),
            new TransferettoSyncOptions {
                Direction = TransferettoSyncDirection.Download
            });

        TransferettoSyncPlanItem item = Assert.Single(plan);
        Assert.Equal(TransferettoSyncAction.DownloadFile, item.Action);
        Assert.Equal("exports/report.csv", item.RelativePath);
        Assert.Equal("/exports/report.csv", item.RemotePath);
    }

    [Fact]
    public void PlannerHonorsIncludeAndExcludePatterns() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry[] source = {
            Directory("logs", @"C:\site\logs", "/wwwroot/logs"),
            File("logs/app.log", @"C:\site\logs\app.log", "/wwwroot/logs/app.log", 10, now),
            File("logs/skip.log", @"C:\site\logs\skip.log", "/wwwroot/logs/skip.log", 20, now),
            File("data/app.json", @"C:\site\data\app.json", "/wwwroot/data/app.json", 30, now)
        };

        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            source,
            Array.Empty<TransferettoSyncEntry>(),
            new TransferettoSyncOptions {
                IncludePatterns = new[] { "*.log" },
                ExcludePatterns = new[] { "*skip*" }
            });

        Assert.Contains(plan, item => item.Action == TransferettoSyncAction.CreateDirectory && item.RelativePath == "logs");
        TransferettoSyncPlanItem transfer = Assert.Single(plan, item => item.Action == TransferettoSyncAction.UploadFile);
        Assert.Equal("logs/app.log", transfer.RelativePath);
        Assert.DoesNotContain(plan, item => item.RelativePath == "logs/skip.log");
        Assert.DoesNotContain(plan, item => item.RelativePath == "data/app.json");
    }

    [Fact]
    public void PlannerSkipsChangedFileWhenOverwriteIsDisabled() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry source = File("site.zip", @"C:\site\site.zip", "/wwwroot/site.zip", 100, now);
        TransferettoSyncEntry destination = File("site.zip", null, "/wwwroot/site.zip", 50, now);

        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            new[] { source },
            new[] { destination },
            new TransferettoSyncOptions {
                OverwriteExisting = false,
                Comparison = TransferettoSyncComparison.Size
            });

        TransferettoSyncPlanItem item = Assert.Single(plan);
        Assert.Equal(TransferettoSyncAction.Skip, item.Action);
        Assert.Contains("overwrite is disabled", item.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PlannerUsesTimestampToleranceForFileComparison() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry source = File("report.csv", @"C:\site\report.csv", "/exports/report.csv", 100, now);
        TransferettoSyncEntry destinationWithinTolerance = File("report.csv", null, "/exports/report.csv", 100, now.AddSeconds(-1));
        TransferettoSyncEntry destinationOutsideTolerance = File("report.csv", null, "/exports/report.csv", 100, now.AddSeconds(-10));

        IReadOnlyList<TransferettoSyncPlanItem> currentPlan = TransferettoSyncPlanner.Plan(
            new[] { source },
            new[] { destinationWithinTolerance },
            new TransferettoSyncOptions {
                Comparison = TransferettoSyncComparison.LastWriteTime,
                TimestampTolerance = TimeSpan.FromSeconds(2)
            });
        IReadOnlyList<TransferettoSyncPlanItem> changedPlan = TransferettoSyncPlanner.Plan(
            new[] { source },
            new[] { destinationOutsideTolerance },
            new TransferettoSyncOptions {
                Comparison = TransferettoSyncComparison.LastWriteTime,
                TimestampTolerance = TimeSpan.FromSeconds(2)
            });

        Assert.Equal(TransferettoSyncAction.Skip, Assert.Single(currentPlan).Action);
        Assert.Equal(TransferettoSyncAction.UploadFile, Assert.Single(changedPlan).Action);
    }

    [Fact]
    public void PlannerOrdersMirrorDirectoryDeletesDeepestFirst() {
        DateTime now = DateTime.UtcNow;
        TransferettoSyncEntry[] destination = {
            Directory("old", @"C:\sync\old", "/sync/old"),
            Directory("old/deep", @"C:\sync\old\deep", "/sync/old/deep"),
            File("old/deep/file.txt", @"C:\sync\old\deep\file.txt", "/sync/old/deep/file.txt", 1, now)
        };

        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            Array.Empty<TransferettoSyncEntry>(),
            destination,
            new TransferettoSyncOptions {
                Direction = TransferettoSyncDirection.Download,
                Mode = TransferettoSyncMode.Mirror
            });
        TransferettoSyncAction[] actions = plan.Select(item => item.Action).ToArray();

        Assert.Equal(
            new[] {
                TransferettoSyncAction.DeleteLocalFile,
                TransferettoSyncAction.DeleteLocalDirectory,
                TransferettoSyncAction.DeleteLocalDirectory
            },
            actions);
        Assert.Equal("old/deep/file.txt", plan[0].RelativePath);
        Assert.Equal("old/deep", plan[1].RelativePath);
        Assert.Equal("old", plan[2].RelativePath);
    }

    private static TransferettoSyncEntry Directory(string relativePath, string? localPath, string? remotePath) {
        return new TransferettoSyncEntry {
            RelativePath = relativePath,
            LocalPath = localPath,
            RemotePath = remotePath,
            IsDirectory = true
        };
    }

    private static TransferettoSyncEntry File(string relativePath, string? localPath, string? remotePath, long length, DateTime lastWriteTimeUtc) {
        return new TransferettoSyncEntry {
            RelativePath = relativePath,
            LocalPath = localPath,
            RemotePath = remotePath,
            IsDirectory = false,
            Length = length,
            LastWriteTimeUtc = lastWriteTimeUtc
        };
    }
}
