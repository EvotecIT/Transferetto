# Transferetto.Cli

Install the .NET tool to copy, resume, inspect, list, and synchronize files across Transferetto endpoints:

```shell
dotnet tool install --global Transferetto.Cli
transferetto --help
```

The following example copies a local file into an S3 bucket and checks the committed object by reading it back:

```shell
transferetto copy file:///data/in report.csv s3://reports/archive report.csv --verify
```

`resume` keeps a checkpoint and reuses verified local chunks or provider-staged parts after cancellation. Choose a checkpoint path writable only by the automation account. The source must support identity-checked ranged reads; destinations may be local files, S3, or Azure Blob Storage.

```shell
transferetto resume file:///data/in large.bin s3://reports/archive large.bin /data/state/large.checkpoint.json --verify
```

Use `sync ... --dry-run` to inspect an update or mirror plan. `--mirror` deletes destination files absent from the source; an empty source also requires `--allow-empty-source`. Endpoint sync operates on files and leaves empty directories alone.

Endpoint URIs select a provider and optional prefix: `file:///absolute/root`, `s3://bucket/prefix`, `azureblob://container/prefix`, `sftp://host/prefix`, `ftp://host/prefix`, or `ftps://host/prefix`. Item paths are separate arguments relative to that endpoint. The CLI rejects credentials and query strings in URIs. Supply credentials and trust settings through environment variables:

| Provider | Variables |
| --- | --- |
| S3 | `TRANSFERETTO_S3_REGION`, `TRANSFERETTO_S3_SERVICE_URL`, `TRANSFERETTO_S3_PATH_STYLE`, `TRANSFERETTO_S3_ACCESS_KEY`, `TRANSFERETTO_S3_SECRET_KEY`, `TRANSFERETTO_S3_SESSION_TOKEN` |
| Azure Blob | `TRANSFERETTO_AZURE_CONNECTION_STRING` |
| SFTP | `TRANSFERETTO_SFTP_USER` and `TRANSFERETTO_SFTP_KNOWN_HOSTS` or `TRANSFERETTO_SFTP_HOST_KEY`; optionally `TRANSFERETTO_SFTP_PASSWORD`, `TRANSFERETTO_SFTP_PRIVATE_KEY`, `TRANSFERETTO_SFTP_KEY_PASSPHRASE` |
| FTP/FTPS | `TRANSFERETTO_FTP_USER`, `TRANSFERETTO_FTP_PASSWORD` |

The CLI returns 0 for success, 2 for invalid arguments, 3 for a missing item, and 4 for a transfer failure. It prints only the exception type on failure because provider error text may contain credentials or signed URLs.
