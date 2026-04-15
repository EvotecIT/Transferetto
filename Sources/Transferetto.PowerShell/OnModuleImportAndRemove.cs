using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
/// <summary>
/// Initializes and cleans up assembly resolution for the Transferetto PowerShell module.
/// </summary>

public class OnModuleImportAndRemove : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
	/// <inheritdoc/>
	public void OnImport()
	{
		AppDomain.CurrentDomain.AssemblyResolve += MyResolveEventHandler;
	}

	/// <inheritdoc/>
	public void OnRemove(PSModuleInfo module)
	{
		AppDomain.CurrentDomain.AssemblyResolve -= MyResolveEventHandler;
	}

	private static Assembly? MyResolveEventHandler(object? sender, ResolveEventArgs args)
	{
		if (args.Name.StartsWith("System.Memory,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Memory.dll");
		}
		if (args.Name.StartsWith("System.Runtime.CompilerServices.Unsafe,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Runtime.CompilerServices.Unsafe.dll");
		}
		if (args.Name.StartsWith("System.Numerics.Vectors,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Numerics.Vectors.dll");
		}
		if (args.Name.StartsWith("System.Drawing.Common,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Drawing.Common.dll");
		}
		if (args.Name.StartsWith("System.Buffers,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Buffers.dll");
		}
		if (args.Name.StartsWith("System.ValueTuple,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.ValueTuple.dll");
		}
		if (args.Name.StartsWith("System.Text.Encoding.CodePages,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("System.Text.Encoding.CodePages.dll");
		}
		if (args.Name.StartsWith("BouncyCastle.Cryptography,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("BouncyCastle.Cryptography.dll");
		}
		if (args.Name.StartsWith("FluentFTP,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("FluentFTP.dll");
		}
		if (args.Name.StartsWith("Microsoft.Bcl.AsyncInterfaces,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("Microsoft.Bcl.AsyncInterfaces.dll");
		}
		if (args.Name.StartsWith("Transferetto,", StringComparison.Ordinal))
		{
			return LoadLocalAssembly("Transferetto.dll");
		}
		return null;
	}

	private static Assembly LoadLocalAssembly(string fileName)
	{
		string path = Path.GetDirectoryName(typeof(OnModuleImportAndRemove).Assembly.Location) ?? throw new InvalidOperationException("Unable to resolve module assembly path.");
		string path2 = Path.Combine(path, fileName);
		return Assembly.LoadFile(path2);
	}
}

