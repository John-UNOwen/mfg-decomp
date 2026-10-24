// asmmeta - metadata inspector for the asmdef-probe build.
//
// Reads a managed assembly through System.Reflection.Metadata (nothing is loaded, so a
// Unity-targeted dll can be inspected on a machine with no Unity and no .NET Framework)
// and answers the questions the compile probe needs about the shipped Mahjong Fight
// Girl assemblies:
//
//   tfm <dll>               assembly identity, assembly-level attributes, references
//   dim <dll> [regex]       interfaces whose declared methods carry IL bodies, i.e.
//                           default interface implementations as compiled IL
//   members <dll>           type / member counts
//   diff <a.dll> <b.dll>    member-identity set diff between two assemblies
//
// run: build\tools\asmmeta\bin\Debug\net8.0\asmmeta.exe <command> <dll> [args]

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace AsmMeta
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.Error.WriteLine("usage: asmmeta <tfm|dim|members|diff> <dll> [args]");
				return 2;
			}

			switch (args[0])
			{
				case "diff":
					if (args.Length < 3)
					{
						Console.Error.WriteLine("asmmeta diff <a.dll> <b.dll>");
						return 2;
					}
					return Diff(args[1], args[2]);
				case "tfm":
					return Tfm(args[1]);
				case "dim":
					return Dim(args[1], args.Length > 2 ? args[2] : ".");
				case "members":
					return Members(args[1]);
				default:
					Console.Error.WriteLine("unknown command " + args[0]);
					return 2;
			}
		}

		private static int Diff(string leftPath, string rightPath)
		{
			HashSet<string> left = MemberSet(leftPath, false, 0);
			HashSet<string> right = MemberSet(rightPath, false, 0);
			HashSet<string> leftSig = MemberSet(leftPath, true, 0);
			HashSet<string> rightSig = MemberSet(rightPath, true, 0);
			HashSet<string> leftHand = MemberSet(leftPath, true, 1);
			HashSet<string> rightHand = MemberSet(rightPath, true, 1);
			Report(leftHand, rightHand, leftPath, rightPath, "hand-written members: every member of a non-generated type, with signature");
			HashSet<string> leftVis = MemberSet(leftPath, true, 2);
			HashSet<string> rightVis = MemberSet(rightPath, true, 2);
			Report(leftVis, rightVis, leftPath, rightPath, "public/protected hand-written surface");
			Report(left, right, leftPath, rightPath, "name+access+attributes");
			Report(leftSig, rightSig, leftPath, rightPath, "name+access+attributes+signature");
			return 0;
		}

		private static void Report(HashSet<string> left, HashSet<string> right, string leftPath, string rightPath, string label)
		{
			List<string> onlyLeft = left.Except(right).OrderBy(x => x, StringComparer.Ordinal).ToList();
			List<string> onlyRight = right.Except(left).OrderBy(x => x, StringComparer.Ordinal).ToList();
			Console.WriteLine($"[{label}] left {Path.GetFileName(leftPath)}={left.Count} right {Path.GetFileName(rightPath)}={right.Count} shared={left.Count - onlyLeft.Count} only-left={onlyLeft.Count} only-right={onlyRight.Count}");
			foreach (string m in onlyLeft.Take(60))
			{
				Console.WriteLine($"  - {m}");
			}
			foreach (string m in onlyRight.Take(60))
			{
				Console.WriteLine($"  + {m}");
			}
		}

		private static int Tfm(string path)
		{
			using PEReader pe = new PEReader(File.OpenRead(path));
			MetadataReader md = pe.GetMetadataReader();
			AssemblyDefinition ad = md.GetAssemblyDefinition();
			Console.WriteLine($"assembly   {md.GetString(ad.Name)} {ad.Version} ");
			foreach (CustomAttributeHandle cah in ad.GetCustomAttributes())
			{
				CustomAttribute ca = md.GetCustomAttribute(cah);
				Console.WriteLine($"attribute  {AttributeName(md, ca)} = {AttributeValue(md, ca)}");
			}
			Console.WriteLine("references:");
			foreach (AssemblyReferenceHandle arh in md.AssemblyReferences)
			{
				AssemblyReference ar = md.GetAssemblyReference(arh);
				Console.WriteLine($"  {md.GetString(ar.Name)} {ar.Version}");
			}
			return 0;
		}

		private static int Dim(string path, string pattern)
		{
			using PEReader pe = new PEReader(File.OpenRead(path));
			MetadataReader md = pe.GetMetadataReader();
			Regex rx = new Regex(pattern, RegexOptions.IgnoreCase);
			int typesWithBodies = 0;
			foreach (TypeDefinitionHandle th in md.TypeDefinitions)
			{
				TypeDefinition td = md.GetTypeDefinition(th);
				string full = QualifiedName(md, td);
				if (!rx.IsMatch(full))
				{
					continue;
				}
				string baseName = BaseName(md, td);
				int bodies = 0;
				int declared = 0;
				Console.WriteLine($"type {full} attrs={td.Attributes} base={baseName}");
				foreach (MethodDefinitionHandle mh in td.GetMethods())
				{
					MethodDefinition m = md.GetMethodDefinition(mh);
					declared++;
					string flags = m.Attributes.ToString();
					bool hasBody = m.RelativeVirtualAddress != 0;
					if (hasBody)
					{
						bodies++;
					}
					Console.WriteLine($"  {(hasBody ? "BODY" : "nobdy")} {md.GetString(m.Name)}{Safe(() => DecodeMethod(md, m))} flags={flags}");
				}
				if (bodies > 0)
				{
					typesWithBodies++;
					Console.WriteLine($"-> {full}: {bodies}/{declared} declared methods carry IL bodies");
				}
			}
			Console.WriteLine($"types-with-bodies={typesWithBodies}");
			return 0;
		}

		private static string BaseName(MetadataReader md, TypeDefinition td)
		{
			try
			{
				EntityHandle bh = td.BaseType;
				if (bh.IsNil)
				{
					return "<none>";
				}
				if (bh.Kind == HandleKind.TypeReference)
				{
					TypeReference tr = md.GetTypeReference((TypeReferenceHandle)bh);
					return Qualified(md.GetString(tr.Namespace), md.GetString(tr.Name));
				}
				if (bh.Kind == HandleKind.TypeDefinition)
				{
					return QualifiedName(md, md.GetTypeDefinition((TypeDefinitionHandle)bh));
				}
				return bh.Kind.ToString();
			}
			catch (Exception)
			{
				return "<unreadable>";
			}
		}
		private static int Members(string path)
		{
			using PEReader pe = new PEReader(File.OpenRead(path));
			MetadataReader md = pe.GetMetadataReader();
			int types = 0, methods = 0, fields = 0, props = 0, events = 0;
			foreach (TypeDefinitionHandle th in md.TypeDefinitions)
			{
				types++;
				TypeDefinition td = md.GetTypeDefinition(th);
				methods += td.GetMethods().Count;
				fields += td.GetFields().Count;
				props += td.GetProperties().Count;
				events += td.GetEvents().Count;
			}
			Console.WriteLine($"{Path.GetFileName(path)}: types={types} methods={methods} fields={fields} properties={props} events={events} members={methods + fields + props + events}");
			return 0;
		}

		private static HashSet<string> MemberSet(string path, bool withSignature, int mode)
		{
			var set = new HashSet<string>(StringComparer.Ordinal);
			using PEReader pe = new PEReader(File.OpenRead(path));
			MetadataReader md = pe.GetMetadataReader();
			foreach (TypeDefinitionHandle th in md.TypeDefinitions)
			{
				TypeDefinition td = md.GetTypeDefinition(th);
				string t = QualifiedName(md, td);
				string tname = md.GetString(td.Name);
				string tflags = td.Attributes.ToString();
				if (mode >= 1 && Generated(tname))
				{
					continue;
				}
				if (mode >= 2 && !(tflags.Contains("Public") || tflags.Contains("Family")))
				{
					continue;
				}
				foreach (MethodDefinitionHandle mh in td.GetMethods())
				{
					MethodDefinition m = md.GetMethodDefinition(mh);
					string flags = m.Attributes.ToString();
					string access = Access(flags);
					string name = md.GetString(m.Name);
					if (mode >= 1 && Generated(name))
					{
						continue;
					}
					if (mode >= 2 && (access == "priv" || access == "asm"))
					{
						continue;
					}
					string sig = withSignature ? Safe(() => DecodeMethod(md, m)) : "";
					set.Add($"{t}::M {name}{sig}|{access}|{flags}|body={(m.RelativeVirtualAddress != 0)}");
				}
				foreach (FieldDefinitionHandle fh in td.GetFields())
				{
					FieldDefinition f = md.GetFieldDefinition(fh);
					string flags = f.Attributes.ToString();
					string access = Access(flags);
					string name = md.GetString(f.Name);
					if (mode >= 1 && Generated(name))
					{
						continue;
					}
					if (mode >= 2 && (access == "priv" || access == "asm"))
					{
						continue;
					}
					string sig = withSignature ? Safe(() => DecodeField(md, f)) : "";
					set.Add($"{t}::F {name}{sig}|{access}|{flags}");
				}
				foreach (PropertyDefinitionHandle ph in td.GetProperties())
				{
					PropertyDefinition p = md.GetPropertyDefinition(ph);
					string sig = withSignature ? Safe(() => DecodeProperty(md, p)) : "";
					set.Add($"{t}::P {md.GetString(p.Name)}{sig}");
				}
				foreach (EventDefinitionHandle eh in td.GetEvents())
				{
					EventDefinition e = md.GetEventDefinition(eh);
					set.Add($"{t}::E {md.GetString(e.Name)}");
				}
			}
			return set;
		}

		private static string Access(string flags)
		{
			if (flags.Contains("Public"))
			{
				return "pub";
			}
			if (flags.Contains("FamilyOrAssembly"))
			{
				return "famorassem";
			}
			if (flags.Contains("Family"))
			{
				return "fam";
			}
			if (flags.Contains("Assembly"))
			{
				return "asm";
			}
			return "priv";
		}

		private static string Safe(Func<string> f)
		{
			try
			{
				return f();
			}
			catch (Exception e)
			{
				return "<sig:" + e.GetType().Name + ">";
			}
		}

		private static string DecodeMethod(MetadataReader md, MethodDefinition m)
		{
			var s = m.DecodeSignature(new DumpProvider(md), null);
			return ":m" + s.ReturnType + "(" + string.Join(",", s.ParameterTypes) + ")";
		}

		private static string DecodeField(MetadataReader md, FieldDefinition f)
		{
			return ":f" + f.DecodeSignature(new DumpProvider(md), null);
		}

		private static string DecodeProperty(MetadataReader md, PropertyDefinition p)
		{
			return ":p";
		}

		private static string AttributeName(MetadataReader md, CustomAttribute ca)
		{
			try
			{
				if (ca.Constructor.Kind == HandleKind.MemberReference)
				{
					MemberReference mr = md.GetMemberReference((MemberReferenceHandle)ca.Constructor);
					if (mr.Parent.Kind == HandleKind.TypeReference)
					{
						TypeReference tr = md.GetTypeReference((TypeReferenceHandle)mr.Parent);
						return Qualified(md.GetString(tr.Namespace), md.GetString(tr.Name));
					}
				}
				else if (ca.Constructor.Kind == HandleKind.MethodDefinition)
				{
					MethodDefinition m = md.GetMethodDefinition((MethodDefinitionHandle)ca.Constructor);
					return QualifiedName(md, md.GetTypeDefinition(m.GetDeclaringType()));
				}
			}
			catch (Exception)
			{
			}
			return "<unknown>";
		}

		private static string AttributeValue(MetadataReader md, CustomAttribute ca)
		{
			try
			{
				byte[] bytes = md.GetBlobReader(ca.Value).ReadBytes(md.GetBlobReader(ca.Value).RemainingBytes).ToArray();
				if (bytes.Length == 0)
				{
					return "(no args)";
				}
				int take = Math.Min(bytes.Length, 96);
				return BitConverter.ToString(bytes, 0, take).Replace("-", " ");
			}
			catch (Exception)
			{
				return "<unreadable>";
			}
		}

		// Metadata names of nested types are only the simple name and nested types carry no
		// namespace, so the enclosing chain has to be spliced in or the compiler-generated
		// closure / state-machine types of unrelated methods collide in a member set.
		private static string QualifiedName(MetadataReader md, TypeDefinition td)
		{
			string name = md.GetString(td.Name);
			string ns = md.GetString(td.Namespace);
			EntityHandle en = NestedHandle(md, td);
			if (!en.IsNil && en.Kind == HandleKind.TypeDefinition)
			{
				string chain = QualifiedName(md, md.GetTypeDefinition((TypeDefinitionHandle)en));
				return chain + "/" + name;
			}
			return Qualified(ns, name);
		}

		// Compiler-generated names: closures, async/iterator state machines, switch case
		// maps, lambda caches, auto-property backing fields.
		private static bool Generated(string name)
		{
			return name.Length > 0 && (name[0] == '<' || name.StartsWith("<>") || name.Contains("__") || name.Contains("_002E") || name.Contains("_003C"));
		}


		private static EntityHandle NestedHandle(MetadataReader md, TypeDefinition td)
		{
			return td.GetDeclaringType();
		}
		private static string Qualified(string ns, string name)
		{
			return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
		}

		// Signature strings are only compared against the output of this same decoder on
		// another assembly, so they are readable rather than canonical. Type arguments of
		// instantiated generic types collapse to their arity, so two overloads differing
		// only there compare equal; both sides decode identically.
		sealed class DumpProvider : ISignatureTypeProvider<string, object>
		{
			readonly MetadataReader md;

			public DumpProvider(MetadataReader md)
			{
				this.md = md;
			}

			public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[]";
			public string GetByReferenceType(string elementType) => elementType + "&";
			public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
			public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments)
				=> genericType + "<" + typeArguments.Length + ">";
			public string GetGenericMethodParameter(object genericContext, int index) => "!!" + index;
			public string GetGenericTypeParameter(object genericContext, int index) => "!" + index;
			public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
			public string GetPinnedType(string elementType) => elementType;
			public string GetPointerType(string elementType) => elementType + "*";
			public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
			public string GetSZArrayType(string elementType) => elementType + "[]";

			public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
			{
				return QualifiedName(reader, reader.GetTypeDefinition(handle));
			}

			public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
			{
				try
				{
					TypeReference tr = reader.GetTypeReference(handle);
					return Qualified(reader.GetString(tr.Namespace), reader.GetString(tr.Name));
				}
				catch (Exception)
				{
					return "typeref?";
				}
			}

			public string GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
			{
				return "spec";
			}

			public string GetTypeFromSerializedName(string name) => name;
			public PrimitiveTypeCode GetUnderlyingEnumType(string type) => PrimitiveTypeCode.Int32;
			public bool IsSystemType(string type) => false;
		}
	}
}
