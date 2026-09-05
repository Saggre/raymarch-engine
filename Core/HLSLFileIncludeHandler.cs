// Modified on 20-Jun-2013 by Justin Stenning
// From original code by Alexandre Mutel.
// -------------------------------------------------------------------
// Original source in SharpDX.Toolkit.Graphics.FileIncludeHandler
// -------------------------------------------------------------------
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.Core.Rendering;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.IO;
using Plane = RaymarchEngine.Core.Primitives.Plane;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Enables the usage of #include directive
    /// </summary>
    public class HLSLFileIncludeHandler : CallbackBase, Include
    {
        /// <summary>
        /// Directory of the file currently being read. Nested includes push and pop it, so a file
        /// can include a sibling by name.
        /// </summary>
        public readonly Stack<string> CurrentDirectory;

        /// <summary>
        /// Extra directories searched when an include is not found next to the including file
        /// </summary>
        public readonly List<string> IncludeDirectories;

        /// <summary>
        /// Creates a handler rooted at a directory
        /// </summary>
        /// <param name="initialDirectory">Directory the first shader file is read from</param>
        public HLSLFileIncludeHandler(string initialDirectory)
        {
            IncludeDirectories = new List<string>();
            CurrentDirectory = new Stack<string>();
            CurrentDirectory.Push(initialDirectory);
        }

        #region Include Members

        /// <summary>
        /// Creates and returns a file stream with constants such as baked array lengths
        /// </summary>
        /// <returns>Stream with HLSL code</returns>
        private Stream GetShaderConstantsStream()
        {
            // Counted from the scene, which is what RenderDevice.Draw uploads. Counting renderers as
            // they are constructed would include ones never added, overrunning the structured buffer.
            string hlslString = $"static const int sphereCount = {SceneRendererCount<Sphere>()};" +
                                $"static const int boxCount = {SceneRendererCount<Box>()};" +
                                $"static const int planeCount = {SceneRendererCount<Plane>()};" +
                                $"static const int torusCount = {SceneRendererCount<Torus>()};" +
                                $"static const int octahedronCount = {SceneRendererCount<Octahedron>()};" +
                                $"static const int ellipsoidCount = {SceneRendererCount<Ellipsoid>()};" +
                                $"static const int cylinderCount = {SceneRendererCount<Cylinder>()};";

            Debug.WriteLine(hlslString);
            byte[] byteArray = Encoding.ASCII.GetBytes(hlslString);
            return new MemoryStream(byteArray);
        }

        /// <summary>
        /// How many renderers of a given primitive type the current scene holds
        /// </summary>
        /// <typeparam name="T">Primitive type to count</typeparam>
        /// <returns>Number of matching renderers in the current scene</returns>
        private static int SceneRendererCount<T>() where T : IPrimitive
        {
            return Scene.CurrentScene.Components<RaymarchRenderer<T>>().Length;
        }

        /// <summary>
        /// Resolves an #include. The name "raymarchengine" is special and returns generated
        /// constants rather than a file.
        /// </summary>
        /// <param name="type">Whether the include used quotes or angle brackets, not used here</param>
        /// <param name="fileName">The name as written in the #include directive</param>
        /// <param name="parentStream">Stream of the including file, not used here</param>
        /// <returns>A stream over the included HLSL</returns>
        /// <exception cref="FileNotFoundException">The file is not in the current directory or any include directory</exception>
        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            Debug.WriteLine(fileName);

            // Include dynamic (:D) constants
            if (fileName.ToLower().Equals("raymarchengine"))
            {
                return GetShaderConstantsStream();
            }

            string currentDirectory = CurrentDirectory.Peek();
            if (currentDirectory == null)
            {
#if NETFX_CORE
                currentDirectory = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#else
                currentDirectory = Environment.CurrentDirectory;
#endif
            }

            string filePath = fileName;

            if (!Path.IsPathRooted(filePath))
            {
                var directoryToSearch = new List<string> {currentDirectory};
                directoryToSearch.AddRange(IncludeDirectories);
                foreach (string dirPath in directoryToSearch)
                {
                    string selectedFile = Path.GetFullPath(Path.Combine(dirPath, fileName));
                    if (NativeFile.Exists(selectedFile))
                    {
                        filePath = selectedFile;
                        break;
                    }
                }
            }

            if (filePath == null || !NativeFile.Exists(filePath))
            {
                throw new FileNotFoundException(String.Format("Unable to find file [{0}]", filePath ?? fileName));
            }

            NativeFileStream fs = new NativeFileStream(filePath, NativeFileMode.Open, NativeFileAccess.Read);
            CurrentDirectory.Push(Path.GetDirectoryName(filePath));
            return fs;
        }

        /// <summary>
        /// Closes a stream handed out by Open and pops the directory it pushed
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public void Close(Stream stream)
        {
            stream.Dispose();
            if (stream.GetType() != typeof(MemoryStream))
            {
                CurrentDirectory.Pop();
            }
        }

        #endregion
    }
}