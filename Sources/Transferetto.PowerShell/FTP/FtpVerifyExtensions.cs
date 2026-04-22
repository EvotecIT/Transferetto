using FluentFTP;

namespace Transferetto.PowerShell;

internal static class FtpVerifyExtensions
{
	internal static FtpVerify CombineFlags(this FtpVerify[]? values)
	{
		if (values == null || values.Length == 0)
		{
			return FtpVerify.None;
		}
		FtpVerify ftpVerify = FtpVerify.None;
		foreach (FtpVerify ftpVerify2 in values)
		{
			ftpVerify |= ftpVerify2;
		}
		return ftpVerify;
	}
}

