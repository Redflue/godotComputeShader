using Godot;
using System;
using ShaderTools;
using Godot.Collections;
using Agents;

using System.Runtime.InteropServices;

public partial class node_2d : Node2D
{
    public override void _Ready()
    {
        base._Ready();

        RenderingDevice rd = RenderingServer.CreateLocalRenderingDevice();

        RDShaderFile shaderFile = GD.Load<RDShaderFile>("res://shaders/compute.glsl");
        RDShaderSpirV shaderByteCode = shaderFile.GetSpirV();
        Rid shader = rd.ShaderCreateFromSpirV(shaderByteCode);

        int agentsNum = 10;
        int sizeOfAgent = AgentPacker.agentSize;
        Agent[] agents = new Agent[agentsNum];
        byte[] agentBytes = new byte[agentsNum * sizeOfAgent];

        for (int i = 0; i<agentsNum; i++) {
            int a = i*2;
            agents[i] = new Agent {
                x=a,
                y=a+1,
                angle=Mathf.Pi
            };
            AgentPacker.PackAgentInByteArray(agents[i], agentBytes, i*sizeOfAgent);
        }
        
        Rid buffer = rd.StorageBufferCreate((uint)agentBytes.Length, agentBytes);

        RDUniform uniform = new RDUniform {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 0,
        };
        uniform.AddId(buffer);

        Rid uniformSet = rd.UniformSetCreate(new Array<RDUniform> {uniform}, shader, 0);

        var pipeline = rd.ComputePipelineCreate(shader);
        var computeList = rd.ComputeListBegin();
        rd.ComputeListBindComputePipeline(computeList, pipeline);
        rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        rd.ComputeListDispatch(computeList, (uint)agentsNum/2, 1,1);
        rd.ComputeListEnd();

        rd.Submit();
        rd.Sync();

        byte[] outBytes = rd.BufferGetData(buffer);
        // Agent[] output = new Agent[agentsNum];
        //Buffer.BlockCopy(outBytes, 0, output, 0, outBytes.Length);
        Agent[] output = AgentPacker.PackBytesToAgentArray(outBytes);

        for (int i = 0; i<agentsNum; i++) {
            GD.Print("in x: ", agents[i].x, " | out x: ", output[i].x, " | angle: ", agents[i].angle);
            GD.Print("in y: ", agents[i].y, " | out y: ", output[i].y);
            GD.Print("_________________________________________");
        }

        GD.Print("HELLO! TEST");
    }
}
