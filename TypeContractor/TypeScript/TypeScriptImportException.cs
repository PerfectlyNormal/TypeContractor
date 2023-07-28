using System.Diagnostics.CodeAnalysis;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

public class TypeScriptImportException : Exception
{
    public TypeScriptImportException(string message) : base(message)
    {
    }

    public TypeScriptImportException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TypeScriptImportException()
    {
    }

    public TypeScriptImportException([NotNull] OutputType type, [NotNull] OutputType importedType, ArgumentException ex) 
        : this($"Attempted to import {importedType.Name} ({importedType.FullName}) in {type.Name} ({type.FullName})", ex)
    {
    }
}
