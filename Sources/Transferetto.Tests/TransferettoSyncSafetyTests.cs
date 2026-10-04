namespace Transferetto.Tests;

public sealed class TransferettoSyncSafetyTests {
    [Fact]
    public void CaseInsensitiveMirrorDoesNotDeleteTheFileItJustUpdated() {
        TransferettoSyncEntry[] source = { Entry("report.txt", false, 10) };
        TransferettoSyncEntry[] destination = { Entry("Report.txt", false, 1) };
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(source, destination,
            new TransferettoSyncOptions {
                Direction = TransferettoSyncDirection.Download,
                Mode = TransferettoSyncMode.Mirror,
                PathComparison = TransferettoSyncPathComparison.OrdinalIgnoreCase
            });
        Assert.Equal(TransferettoSyncAction.DownloadFile, Assert.Single(plan).Action);
    }

    [Fact]
    public void CaseInsensitiveDestinationRejectsCollidingSourceNamesBeforePlanning() {
        Assert.Throws<ArgumentException>(() => TransferettoSyncPlanner.Plan(
            new[] { Entry("Readme.md", false), Entry("README.md", false) },
            Array.Empty<TransferettoSyncEntry>(),
            new TransferettoSyncOptions { PathComparison = TransferettoSyncPathComparison.OrdinalIgnoreCase }));
    }

    [Fact]
    public void UnreplaceableAncestorSkipsDirectoriesAndFilesBelowIt() {
        TransferettoSyncEntry[] source = { Entry("parent", true), Entry("parent/nested", true), Entry("parent/nested/file.txt", false) };
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(source,
            new[] { Entry("parent", false) }, new TransferettoSyncOptions { OverwriteExisting = false });
        Assert.Equal(3, plan.Count);
        Assert.All(plan, item => Assert.Equal(TransferettoSyncAction.Skip, item.Action));
    }

    [Fact]
    public void DeepMirrorProcessesEachDirectoryWithoutRecursiveDescendantRescans() {
        List<TransferettoSyncEntry> destination = new();
        string path = "old";
        for (int depth = 0; depth < 128; depth++) {
            destination.Add(Entry(path, true));
            path += "/nested";
        }
        IReadOnlyList<TransferettoSyncPlanItem> plan = TransferettoSyncPlanner.Plan(
            Array.Empty<TransferettoSyncEntry>(), destination, new TransferettoSyncOptions { Mode = TransferettoSyncMode.Mirror });
        Assert.Equal(128, plan.Count);
        Assert.All(plan, item => Assert.Equal(TransferettoSyncAction.DeleteRemoteDirectory, item.Action));
        Assert.Equal(destination.Last().RelativePath, plan.First().RelativePath);
        Assert.Equal("old", plan.Last().RelativePath);
    }

    [Fact]
    public void PlannerHonorsCancellationBeforeEnumeratingManifests() {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => TransferettoSyncPlanner.Plan(
            new[] { Entry("file", false) }, Array.Empty<TransferettoSyncEntry>(), null, cancellation.Token));
    }

    private static TransferettoSyncEntry Entry(string path, bool directory, long length = 0) => new() {
        RelativePath = path, IsDirectory = directory, Length = directory ? null : length
    };
}
