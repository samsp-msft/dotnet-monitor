﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.WebApi.Stacks
{
    internal sealed class TextStacksFormatter : StacksFormatter
    {
        private const string Indent = "  ";

        public TextStacksFormatter(Stream outputStream) : base(outputStream)
        {
        }

        public override async Task FormatStack(CallStackResult stackResult, CancellationToken token)
        {
            await using StreamWriter writer = new StreamWriter(this.OutputStream, Encoding.UTF8, leaveOpen: true);
            var builder = new StringBuilder();
            foreach (var stack in stackResult.Stacks)
            {
                token.ThrowIfCancellationRequested();

                await writer.WriteLineAsync(FormatThreadName(stack.ThreadId, stack.ThreadName));
                foreach (var frame in stack.Frames)
                {
                    builder.Clear();
                    builder.Append(Indent);
                    BuildFrame(builder, stackResult.NameCache, frame);
                    await writer.WriteLineAsync(builder, token);
                }
                await writer.WriteLineAsync();
            }

            await writer.FlushAsync();
        }

        private void BuildFrame(StringBuilder builder, NameCache cache, CallStackFrame frame)
        {
            if (frame.FunctionId == 0)
            {
                builder.Append(NativeFrame);
            }
            else if (cache.FunctionData.TryGetValue(frame.FunctionId, out FunctionData functionData))
            {
                builder.Append(GetModuleName(cache, functionData.ModuleId));
                builder.Append(ModuleSeparator);
                BuildClassName(builder, cache, functionData);
                builder.Append(ClassSeparator);
                builder.Append(functionData.Name);
                BuildGenericParameters(builder, cache, functionData.TypeArgs);
            }
            else
            {
                builder.Append(UnknownFunction);
            }
        }
    }
}
