
using Godot;

namespace Shaders {
    public static class ShaderHelper {
        public static RenderingDevice renderingDevice;

        public static void CreateRendereringDevice() {
            renderingDevice = RenderingServer.CreateLocalRenderingDevice();
        }

        public static Rid LoadShader(string shaderPath) {
            
            RDShaderFile shaderFile = GD.Load<RDShaderFile>(shaderPath);
            RDShaderSpirV shaderByteCode = shaderFile.GetSpirV();
            Rid shader = renderingDevice.ShaderCreateFromSpirV(shaderByteCode);

            return shader;
        }

        public static RDUniform CreateStorageBufferUniform(byte[] bytes, int binding = 0) {
            Rid buffer = renderingDevice.StorageBufferCreate((uint)bytes.Length,bytes);
            RDUniform uniform = new()
            {
                UniformType = RenderingDevice.UniformType.StorageBuffer,
                Binding = binding
            };
            uniform.AddId(buffer);
            return uniform;
        }
    }
}