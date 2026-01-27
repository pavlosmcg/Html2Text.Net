// This file provides polyfills for C# language features not available in older target frameworks
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER
/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// This dummy class is required to compile records when targeting .NET Standard.
/// </summary>
internal static class IsExternalInit
{
}
#endif
