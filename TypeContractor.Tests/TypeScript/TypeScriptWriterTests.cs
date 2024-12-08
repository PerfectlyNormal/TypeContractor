using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor.Tests.TypeScript;

public class TypeScriptWriterTests : IDisposable
{
	private readonly DirectoryInfo _outputDirectory;
	private readonly TypeContractorConfiguration _configuration;
	private readonly TypeScriptConverter _converter;

	public TypeScriptWriter Sut { get; }

	public TypeScriptWriterTests()
	{
		var assembly = typeof(TypeScriptWriterTests).Assembly;
		_outputDirectory = Directory.CreateTempSubdirectory();
		_configuration = TypeContractorConfiguration
			.WithDefaultConfiguration()
			.AddAssembly(assembly.FullName!, assembly.Location)
			.SetOutputDirectory(_outputDirectory.FullName);
		_converter = new TypeScriptConverter(_configuration, BuildMetadataLoadContext());
		Sut = new TypeScriptWriter(_configuration.OutputPath);
	}

	[Fact]
	public void Can_Write_Simple_Types()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(SimpleTypes));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		var file = File.ReadAllLines(result).Select(x => x.TrimStart());
		file.Should()
			.NotBeEmpty()
			.And.NotContainMatch("import * from")
			.And.Contain("export interface SimpleTypes {")
			.And.Contain("stringProperty: string;")
			.And.Contain("numberProperty?: number;")
			.And.Contain("numbersProperty: number[];")
			.And.Contain("doubleTime: number;")
			.And.Contain("timeyWimeySpan: string;")
			.And.Contain("someObject: any;");
	}

	[Fact]
	public void Handles_Dictionary_With_Complex_Values()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(ComplexValueDictionary));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		var file = File.ReadAllText(result);
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { FormulaDto } from './FormulaDto';")
			.And.Contain("formulas: { [key: string]: FormulaDto[] };");
	}

	[Fact]
	public void Handles_Dictionary_With_Nested_Dictionary_Values()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(NestedValueDictionary));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		var file = File.ReadAllText(result);
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { FormulaDto } from './FormulaDto';")
			.And.Contain("formulas: { [key: string]: { [key: string]: FormulaDto[] } };");
	}

	[Fact]
	public void Includes_Deprecated_JSDoc()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(ObsoleteResponse));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		var file = File.ReadAllText(result);
		file.Should()
			.NotBeEmpty()
			.And.NotContain("import ")
			.And.Contain("export interface ObsoleteResponse {")
			.And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated\r?\n\s+\*/\r?\n\s+ obsoleteNoDesc: number;")
			.And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated Use NonObsoleteProp instead\r?\n\s+\*/\r?\n\s+ obsolete: number;")
			.And.MatchRegex(@"\s+nonObsoleteProp: number;");
	}

	[Fact]
	public void Includes_Deprecated_JSDoc_For_Enum_Members()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(ObsoleteEnum));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		var file = File.ReadAllText(result);
		file.Should()
			.NotBeEmpty()
			.And.NotContain("import ")
			.And.Contain("export enum ObsoleteEnum {")
			.And.Contain("None = 0,")
			.And.Contain("Pending = 1,")
			.And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated No longer used\r?\n\s+\*/\r?\n\s+ Paid = 2,")
			.And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated\r?\n\s+\*/\r?\n\s+ Rejected = 3,")
			.And.Contain("Done = 4,");
	}

	[Fact]
	public void Handles_Nested_Nullable_Records()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TopLevelRecord));

		// Act
		var topLevelResult = Sut.Write(outputTypes.First(), outputTypes, false);
		var secondStoryResult = Sut.Write(outputTypes.First(x => x.Name == "SecondStoryRecord"), outputTypes, false);
		var someOtherDeeplyNestedResult = Sut.Write(outputTypes.First(x => x.Name == "SomeOtherDeeplyNestedRecord"), outputTypes, false);

		// Assert
		var topLevelFile = File.ReadAllText(topLevelResult);
		topLevelFile.Should()
			.NotBeEmpty()
			.And.Contain("import { SecondStoryRecord } from './SecondStoryRecord';")
			.And.Contain("export interface TopLevelRecord {")
			.And.Contain("  name: string;")
			.And.Contain("  secondStoryRecord?: SecondStoryRecord;")
			.And.Contain("}");

		var secondStoryFile = File.ReadAllText(secondStoryResult);
		secondStoryFile.Should()
			.NotBeEmpty()
			.And.Contain("import { SomeOtherDeeplyNestedRecord } from './SomeOtherDeeplyNestedRecord';")
			.And.Contain("export interface SecondStoryRecord {")
			.And.Contain("  description: string;")
			.And.Contain("  someOtherDeeplyNestedRecord?: SomeOtherDeeplyNestedRecord;")
			.And.Contain("}");

		var deeplyNestedFile = File.ReadAllText(someOtherDeeplyNestedResult);
		deeplyNestedFile.Should()
			.NotBeEmpty()
			.And.NotContain("import {")
			.And.Contain("export interface SomeOtherDeeplyNestedRecord {")
			.And.Contain("  extra: string;")
			.And.Contain("}");
	}

	[Fact]
	public void Writes_Basic_Zod_Schema()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(SimpleTypes));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("export const SimpleTypesSchema = z.object({")
			.And.Contain("  stringProperty: z.string(),")
			.And.Contain("  numberProperty: z.number().nullable(),")
			.And.Contain("  numbersProperty: z.array(z.number()),")
			.And.Contain("  doubleTime: z.number(),")
			.And.Contain("  timeyWimeySpan: z.string(),")
			.And.Contain("  someObject: z.any(),")
			.And.Contain("});");
	}

	[Fact]
	public void Writes_Zod_Schema_With_Reference_To_Custom_Types()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(ReferenceType));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { SimpleTypes, SimpleTypesSchema } from './SimpleTypes';")
			.And.Contain("export const ReferenceTypeSchema = z.object({")
			.And.Contain("  name: z.string().nullable(),")
			.And.Contain("  simpleReference: SimpleTypesSchema,")
			.And.Contain("});");
	}

	[Fact]
	public void Writes_Zod_Schema_With_Enum_Reference()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TypeWithEnum));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { ObsoleteEnum } from './ObsoleteEnum';")
			.And.Contain("export const TypeWithEnumSchema = z.object({")
			.And.Contain("  status: z.nativeEnum(ObsoleteEnum),")
			.And.Contain("});");
	}

	[Fact]
	public void Writes_Zod_Schema_With_Nullable_Enum()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TypeWithNullableEnum));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { ObsoleteEnum } from './ObsoleteEnum';")
			.And.Contain("export const TypeWithNullableEnumSchema = z.object({")
			.And.Contain("  status: z.nativeEnum(ObsoleteEnum).nullable(),")
			.And.Contain("});");
	}

	[Fact]
	public void Writes_Zod_Schema_With_Custom_Types_In_Dictionaries()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TypeWithCustomDictionaryValues));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { ReferenceType, ReferenceTypeSchema } from './ReferenceType';")
			.And.Contain("export const TypeWithCustomDictionaryValuesSchema = z.object({")
			.And.Contain("  references: z.record(z.string(), ReferenceTypeSchema),")
			.And.Contain("});");
	}

	[Fact]
	public void Writes_Zod_Schema_With_List_Of_Custom_Types_In_Dictionaries()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TypeWithCustomEnumerableDictionaryValues));

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, true);
		var file = File.ReadAllText(result);

		// Assert
		file.Should()
			.NotBeEmpty()
			.And.Contain("import { z } from 'zod';")
			.And.Contain("import { ReferenceType, ReferenceTypeSchema } from './ReferenceType';")
			.And.Contain("export const TypeWithCustomEnumerableDictionaryValuesSchema = z.object({")
			.And.Contain("  references: z.record(z.string(), z.array(ReferenceTypeSchema)),")
			.And.Contain("});");
	}

	[Fact]
	public void Zod_Schema_Handles_Nested_Nullable_Records()
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(TopLevelRecord));

		// Act
		var topLevelResult = Sut.Write(outputTypes.First(), outputTypes, true);
		var secondStoryResult = Sut.Write(outputTypes.First(x => x.Name == "SecondStoryRecord"), outputTypes, true);
		var someOtherDeeplyNestedResult = Sut.Write(outputTypes.First(x => x.Name == "SomeOtherDeeplyNestedRecord"), outputTypes, true);

		// Assert
		var topLevelFile = File.ReadAllText(topLevelResult);
		topLevelFile.Should()
			.NotBeEmpty()
			.And.Contain("import { SecondStoryRecord, SecondStoryRecordSchema } from './SecondStoryRecord';")
			.And.Contain("export const TopLevelRecordSchema = z.object({")
			.And.Contain("  name: z.string(),")
			.And.Contain("  secondStoryRecord: SecondStoryRecordSchema.nullable(),")
			.And.Contain("});");

		var secondStoryFile = File.ReadAllText(secondStoryResult);
		secondStoryFile.Should()
			.NotBeEmpty()
			.And.Contain("import { SomeOtherDeeplyNestedRecord, SomeOtherDeeplyNestedRecordSchema } from './SomeOtherDeeplyNestedRecord';")
			.And.Contain("export const SecondStoryRecordSchema = z.object({")
			.And.Contain("  description: z.string(),")
			.And.Contain("  someOtherDeeplyNestedRecord: SomeOtherDeeplyNestedRecordSchema.nullable(),")
			.And.Contain("});");

		var deeplyNestedFile = File.ReadAllText(someOtherDeeplyNestedResult);
		deeplyNestedFile.Should()
			.NotBeEmpty()
			.And.Contain("import { z }")
			.And.NotMatchRegex("import \\{ [^z]+ \\}")
			.And.Contain("export const SomeOtherDeeplyNestedRecordSchema = z.object({")
			.And.Contain("  extra: z.string(),")
			.And.Contain("});");
	}

	[Theory]
	[InlineData(Casing.Camel)]
	[InlineData(Casing.Pascal)]
	[InlineData(Casing.Kebab)]
	[InlineData(Casing.Snake)]
	public void Changes_File_Name_According_To_Casing(Casing casing)
	{
		// Arrange
		var outputTypes = BuildOutputTypes(typeof(SimpleTypes), casing);

		// Act
		var result = Sut.Write(outputTypes.First(), outputTypes, false);

		// Assert
		switch (casing)
		{
			case Casing.Camel:
				result.Should().EndWith(Path.Join("typeContractor", "tests", "typeScript", "simpleTypes.ts"));
				break;
			case Casing.Pascal:
				result.Should().EndWith(Path.Join("TypeContractor", "Tests", "TypeScript", "SimpleTypes.ts"));
				break;
			case Casing.Kebab:
				result.Should().EndWith(Path.Join("type-contractor", "tests", "type-script", "simple-types.ts"));
				break;
			case Casing.Snake:
				result.Should().EndWith(Path.Join("type_contractor", "tests", "type_script", "simple_types.ts"));
				break;
		}
	}

	private List<OutputType> BuildOutputTypes(Type type, Casing casing = Casing.Pascal)
	{
		var oldCasing = _configuration.Casing;
		try
		{
			_configuration.SetCasing(casing);
			var contractedTypes = new[] { type }
						.Select(t => ContractedType.FromName(t.FullName!, t, _configuration));

			return contractedTypes
					.Select(_converter.Convert)
					.ToList() // Needed so `converter.Convert` runs before we concat
					.Concat(_converter.CustomMappedTypes.Values)
					.ToList();
		}
		finally
		{
			_configuration.SetCasing(oldCasing);
		}
	}

	#region Test input
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private class SimpleTypes
	{
		public string StringProperty { get; set; }
		public int? NumberProperty { get; set; }
		public IEnumerable<int> NumbersProperty { get; set; }
		public double DoubleTime { get; set; }
		public TimeSpan TimeyWimeySpan { get; set; }
		public object SomeObject { get; set; }
	}

	private class ReferenceType
	{
		public string? Name { get; set; }
		public SimpleTypes SimpleReference { get; set; }
	}

	private class TypeWithEnum
	{
		public ObsoleteEnum Status { get; set; }
	}

	private class TypeWithNullableEnum
	{
		public DateTime SubmittedDateTimeUtc { get; set; }
		public ObsoleteEnum? Status { get; set; }
	}

	private class TypeWithCustomDictionaryValues
	{
		public Dictionary<string, ReferenceType> References { get; set; }
	}

	private class TypeWithCustomEnumerableDictionaryValues
	{
		public Dictionary<string, IEnumerable<ReferenceType>> References { get; set; }
	}

	private class ObsoleteResponse
	{
		[Obsolete]
		public int ObsoleteNoDesc { get; set; }

		[Obsolete("Use NonObsoleteProp instead")]
		public double Obsolete { get; set; }

		public decimal NonObsoleteProp { get; set; }
	}

	private enum ObsoleteEnum
	{
		None,
		Pending,
		[Obsolete("No longer used")]
		Paid,
		[Obsolete]
		Rejected,
		Done,
	}

	private class ComplexValueDictionary
	{
		public Dictionary<Guid, IEnumerable<FormulaDto>> Formulas { get; set; }
	}

	private class NestedValueDictionary
	{
		public Dictionary<Guid, Dictionary<string, IEnumerable<FormulaDto>>> Formulas { get; set; }
	}

	private class FormulaDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Definition { get; set; }
	}

	private record TopLevelRecord(string Name, SecondStoryRecord? SecondStoryRecord);
	private record SecondStoryRecord(string Description, SomeOtherDeeplyNestedRecord? SomeOtherDeeplyNestedRecord);
	private record SomeOtherDeeplyNestedRecord(string Extra);
	#endregion
#pragma warning restore CS8618

	private MetadataLoadContext BuildMetadataLoadContext()
	{
		// Get the array of runtime assemblies.
		var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

		// Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
		var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

		var resolver = new PathAssemblyResolver(paths);

		return new MetadataLoadContext(resolver);
	}

	public void Dispose()
	{
		if (_outputDirectory.Exists)
			_outputDirectory.Delete(true);
	}
}
