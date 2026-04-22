using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
/// <summary>
/// Initializes and cleans up assembly resolution for the Transferetto PowerShell module.
/// </summary>

public class OnModuleImportAndRemove : IModuleAssemblyInitializer, IModuleAssemblyCleanup {
	/// <inheritdoc/>
	public void OnImport() {
		if (IsNetFramework()) {
			AppDomain.CurrentDomain.AssemblyResolve += MyResolveEventHandler;
		}
	}

	/// <inheritdoc/>
	public void OnRemove(PSModuleInfo module) {
		if (IsNetFramework()) {
			AppDomain.CurrentDomain.AssemblyResolve -= MyResolveEventHandler;
		}
	}

	private static Assembly? MyResolveEventHandler(object? sender, ResolveEventArgs args) {
		string? moduleDirectory = Path.GetDirectoryName(typeof(OnModuleImportAndRemove).Assembly.Location);
		if (string.IsNullOrWhiteSpace(moduleDirectory)) {
			return null;
		}

		List<string> directoriesToSearch = new() { moduleDirectory };
		if (Directory.Exists(moduleDirectory)) {
			directoriesToSearch.AddRange(Directory.GetDirectories(moduleDirectory, "*", SearchOption.AllDirectories));
		}

		string requestedAssemblyFileName = new AssemblyName(args.Name).Name + ".dll";
		foreach (string directory in directoriesToSearch) {
			string assemblyPath = Path.Combine(directory, requestedAssemblyFileName);
			if (!File.Exists(assemblyPath)) {
				continue;
			}

			try {
				return Assembly.LoadFrom(assemblyPath);
			} catch {
				// Keep probing other candidate paths and preserve the original resolve failure.
			}
		}

		return null;
	}

	private static bool IsNetFramework() {
		return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
	}
}
