using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SharpDX.D3DCompiler;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Keeps compiled shader bytecode on disk so that starting the engine does not mean compiling
    /// the raymarch shader again.
    ///
    /// That compile takes about four and a half seconds, and it is the whole of the wait before
    /// the first frame. The obvious fix is to compile at build time instead, but this shader
    /// cannot be: HLSLFileIncludeHandler bakes the scene's per primitive counts into it, so the
    /// bytecode depends on what the game logic built and is not known until the engine is running.
    /// dxc is out for a second reason, it dropped the HLSL interfaces the primitive system uses.
    ///
    /// Caching what the first run produced sidesteps both. D3D bytecode is an intermediate form
    /// rather than machine code, so it is not tied to the GPU or the driver it was made on.
    /// </summary>
    public static class ShaderCache
    {
        private const string CacheDirectory = "ShaderCache";

        /// <summary>
        /// Returns bytecode for a shader, compiling it only if nothing usable is cached.
        ///
        /// The key covers every input the compiler saw: the source of every shader file in the
        /// folder, the constants the include handler generated, and the stage and flags. Editing
        /// any shader, or building a scene with a different number of primitives, produces a
        /// different key and a fresh compile rather than a stale hit.
        /// </summary>
        /// <param name="folderPath">Folder holding the shader source</param>
        /// <param name="identity">What is being compiled, such as the file name and profile</param>
        /// <param name="generatedSource">Source the include handler produced for this build</param>
        /// <param name="compile">Runs the real compile when there is no usable cache entry</param>
        /// <returns>Shader bytecode, ready to hand to the device</returns>
        public static byte[] GetOrCompile(string folderPath, string identity, string generatedSource,
            Func<byte[]> compile)
        {
            string key = BuildKey(folderPath, identity, generatedSource);
            string path = Path.Combine(CacheDirectory, key + ".cso");

            try
            {
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
            }
            catch (IOException)
            {
                // An unreadable cache is not a reason to fail, it is a reason to compile
            }

            byte[] bytecode = compile();

            try
            {
                Directory.CreateDirectory(CacheDirectory);
                File.WriteAllBytes(path, bytecode);
            }
            catch (IOException)
            {
                // Read only install directory, or two engines racing. Neither is fatal, it just
                // means the next start pays for the compile again.
            }
            catch (UnauthorizedAccessException)
            {
            }

            return bytecode;
        }

        /// <summary>
        /// Turns bytecode back into something the device can consume
        /// </summary>
        /// <param name="bytecode">Bytes from GetOrCompile</param>
        /// <returns>The same bytecode wrapped for SharpDX</returns>
        public static ShaderBytecode ToShaderBytecode(byte[] bytecode)
        {
            return new ShaderBytecode(bytecode);
        }

        private static string BuildKey(string folderPath, string identity, string generatedSource)
        {
            StringBuilder inputs = new StringBuilder();
            inputs.Append(identity).Append('\n');
            inputs.Append(generatedSource).Append('\n');

            // Sorted, so the key does not depend on the order the file system happens to list in
            string[] files = Directory.GetFiles(folderPath, "*.hlsl", SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (string file in files)
            {
                inputs.Append(Path.GetFileName(file)).Append('\n');
                inputs.Append(File.ReadAllText(file)).Append('\n');
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(inputs.ToString()));

                return BitConverter.ToString(hash).Replace("-", string.Empty).Substring(0, 32);
            }
        }
    }
}
