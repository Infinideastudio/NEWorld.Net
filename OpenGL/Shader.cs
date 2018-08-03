// 
// OpenGL: Shader.cs
// NEWorld: A Free Game with Similar Rules to Minecraft.
// Copyright (C) 2015-2018 NEWorld Team
// 
// NEWorld is free software: you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// NEWorld is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General 
// Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with NEWorld.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Utilities;

namespace OpenGL
{
    static partial class Gl
    {
        public const uint FragmentShader = 0x8B30;
        public const uint VertexShader = 0x8B31;
        public const uint GeometryShader = 0x8DD9;
        internal const uint InfoLogLength = 0x8B84;
        internal const uint CompileStatus = 0x8B81;
        internal const uint LinkStatus = 0x8B82;

        internal delegate void CompileShaderProc(uint shader);

        internal delegate uint CreateProgramProc();

        internal delegate uint CreateShaderProc(uint type);

        internal delegate void DeleteProgramProc(uint program);

        internal delegate void DeleteShaderProc(uint shader);

        internal unsafe delegate void GetProgramInfoLogProc(uint prog, int bufSize, int* length, char* log);

        internal unsafe delegate void GetProgramivProc(uint program, uint pname, int* param);

        internal unsafe delegate void GetShaderInfoLogProc(uint shader, int bufSize, int* length, char* log);

        internal unsafe delegate void GetShaderivProc(uint shader, uint pname, int* param);

        internal delegate void AttachShaderProc(uint program, uint shader);

        internal delegate void LinkProgramProc(uint program);

        internal unsafe delegate void ShaderSourceProc(uint shader, int count, char** str, int* length);

        internal delegate void UseProgramProc(uint program);

        internal delegate void ProgramUniform1IProc(uint program, int location, int v0);

        internal static CompileShaderProc CompileShader;
        internal static CreateProgramProc CreateProgram;
        internal static CreateShaderProc CreateShader;
        internal static DeleteProgramProc DeleteProgram;
        internal static DeleteShaderProc DeleteShader;
        internal static GetProgramInfoLogProc GetProgramInfoLog;
        internal static GetProgramivProc GetProgramiv;
        internal static GetShaderInfoLogProc GetShaderInfoLog;
        internal static GetShaderivProc GetShaderiv;
        internal static AttachShaderProc AttachShader;
        internal static LinkProgramProc LinkProgram;
        internal static ShaderSourceProc ShaderSource;
        internal static UseProgramProc UseProgram;
        internal static ProgramUniform1IProc ProgramUniform1I;

        static partial void InitShader()
        {
            CompileShader = Get<CompileShaderProc>("glCompileShader");
            CreateProgram = Get<CreateProgramProc>("glCreateProgram");
            CreateShader = Get<CreateShaderProc>("glCreateShader");
            DeleteProgram = Get<DeleteProgramProc>("glDeleteProgram");
            DeleteShader = Get<DeleteShaderProc>("glDeleteShader");
            GetProgramInfoLog = Get<GetProgramInfoLogProc>("glGetProgramInfoLog");
            GetProgramiv = Get<GetProgramivProc>("glGetProgramiv");
            GetShaderInfoLog = Get<GetShaderInfoLogProc>("glGetShaderInfoLog");
            GetShaderiv = Get<GetShaderivProc>("glGetShaderiv");
            AttachShader = Get<AttachShaderProc>("glAttachShader");
            LinkProgram = Get<LinkProgramProc>("glLinkProgram");
            ShaderSource = Get<ShaderSourceProc>("glShaderSource");
            UseProgram = Get<UseProgramProc>("glUseProgram");
            ProgramUniform1I = Get<ProgramUniform1IProc>("glProgramUniform1iEXT");
        }
    }

    public class Shader : StrictDispose
    {
        public unsafe Shader(uint eShaderType, string strFileData)
        {
            var shader = Gl.CreateShader(eShaderType);
            var file = Gl.Utf8ToNative(strFileData);
            var ptr = Marshal.AllocHGlobal(file.Length);
            Marshal.Copy(file, 0, ptr, file.Length);
            var cString = (char*) ptr;
            Gl.ShaderSource(shader, 1, &cString, null);
            Marshal.FreeHGlobal(ptr);
            Gl.CompileShader(shader);
            int status;
            Gl.GetShaderiv(shader, Gl.CompileStatus, &status);
            if (status == 0)
            {
                int infoLogLength;
                Gl.GetShaderiv(shader, Gl.InfoLogLength, &infoLogLength);
                var strInfoLog = Marshal.AllocHGlobal(infoLogLength + 1);
                Gl.GetShaderInfoLog(shader, infoLogLength, null, (char*) strInfoLog);
                var infoLog = Gl.Utf8ToManaged(strInfoLog);
                Marshal.FreeHGlobal(strInfoLog);
                throw new Exception("Compile failure in " + GetShaderTypeString(eShaderType) + " shader:" + infoLog);
            }

            _hdc = shader;
        }

        private static string GetShaderTypeString(uint type)
        {
            switch (type)
            {
                case Gl.VertexShader:
                    return "vertex";
                case Gl.GeometryShader:
                    return "geometry";
                case Gl.FragmentShader:
                    return "fragment";
                default:
                    return "unknown type";
            }
        }

        protected override void Release() => Gl.DeleteShader(_hdc);

        public uint Raw() => _hdc;

        private readonly uint _hdc;
    }

    public class Program : StrictDispose
    {
        public unsafe void Link(IEnumerable<Shader> shaderList)
        {
            var programId = Gl.CreateProgram();
            foreach (var sd in shaderList)
                Gl.AttachShader(programId, sd.Raw());
            Gl.LinkProgram(programId);
            int status;
            Gl.GetProgramiv(programId, Gl.LinkStatus, &status);
            if (status == 0)
            {
                int infoLogLength;
                Gl.GetProgramiv(programId, Gl.InfoLogLength, &infoLogLength);
                var strInfoLog = Marshal.AllocHGlobal(infoLogLength + 1);
                Gl.GetProgramInfoLog(programId, infoLogLength, null, (char*) strInfoLog);
                var infoLog = Gl.Utf8ToManaged(strInfoLog);
                Marshal.FreeHGlobal(strInfoLog);
                throw new Exception("Linker failure: " + infoLog);
            }

            _hdc = programId;
        }

        protected override void Release() => Gl.DeleteProgram(_hdc);

        public void Use() => Gl.UseProgram(_hdc);

        public void Uniform(int location, int val) => Gl.ProgramUniform1I(_hdc, location, val);

        public uint Raw() => _hdc;

        private uint _hdc;
    }
}