﻿namespace Breezy.Core.IO.Attributes;

public static class SplitOnAttribute
{
    public const string ShortName = "SplitOn";

    public const string Name = $"{ShortName}Attribute";
		
    public const string FullName = $"{BreezyGenerator.Namespace}.{Name}";
    
    public const string Content = $$"""
		// <auto-generated />
		using System;

		namespace {{BreezyGenerator.Namespace}};

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		public sealed class {{Name}} : Attribute
		{
			public int[] Index { get; init; }

			public {{Name}}(params int[] index) => Index = index ?? throw new ArgumentNullException("index not defined"); 
		}
		""";
}