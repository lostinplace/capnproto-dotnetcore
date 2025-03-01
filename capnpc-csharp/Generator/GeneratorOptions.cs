﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CapnpC.Generator
{
    class GeneratorOptions
    {
        public string TopNamespaceName { get; set; } = "CapnpGen";
        public string ReaderStructName { get; set; } = "READER";
        public string WriterStructName { get; set; } = "WRITER";
        public string ReaderParameterName { get; set; } = "reader";
        public string WriterParameterName { get; set; } = "writer";
        public string ReaderCreateMethodName { get; set; } = "create";
        public string ReaderContextFieldName { get; set; } = "ctx";
        public string ContextParameterName { get; set; } = "ctx";
        public string GroupReaderContextArgName { get; set; } = "ctx";
        public string GroupWriterContextArgName { get; set; } = "ctx";
        public string UnionDisciminatorEnumName { get; set; } = "WHICH";
        public string UnionDiscriminatorPropName { get; set; } = "which";
        public string UnionDiscriminatorFieldName { get; set; } = "_which";
        public string UnionDisciminatorUndefinedName { get; set; } = "undefined";
        public string UnionContentFieldName { get; set; } = "_content";
        public string SerializeMethodName { get; set; } = "serialize";
        public string ApplyDefaultsMethodName { get; set; } = "applyDefaults";
        public string AnonymousParameterName { get; set; } = "arg_";
        public string CancellationTokenParameterName { get; set; } = "cancellationToken_";
        public string ParamsLocalName { get; set; } = "in_";
        public string DeserializerLocalName { get; set; } = "d_";
        public string SerializerLocalName { get; set; } = "s_";
        public string ResultLocalName { get; set; } = "r_";
        public string ParamsStructFormat { get; set; } = "Params_{0}";
        public string ResultStructFormat { get; set; } = "Result_{0}";
        public string PropertyNamedLikeTypeRenameFormat { get; set; } = "The{0}";
        public string InstLocalName { get; set; } = "inst";
        public string GenericTypeParameterFormat { get; set; } = "T{0}";
        public string PipeliningExtensionsClassName { get; set; } = "PipeliningSupportExtensions";
        public string MemberAccessPathNameFormat { get; set; } = "Path_{0}_{1}";
        public string TaskParameterName { get; set; } = "task";
        public string EagerMethodName { get; set; } = "Eager";
    }
}
