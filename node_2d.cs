using Godot;
using System;
using Godot.Collections;
using Agents;
using Shaders;

using System.Runtime.InteropServices;

public partial class node_2d : Node2D
{

    Rid image;

    public override void _Ready()
    {
        base._Ready();
        ShaderHelper.CreateRendereringDevice();
        Rid shader = ShaderHelper.LoadShader("res://shaders/compute.glsl");

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
        
        Rid buffer = ShaderHelper.renderingDevice.StorageBufferCreate((uint)agentBytes.Length, agentBytes);
        RDUniform uniform = new RDUniform {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 0,
        };
        uniform.AddId(buffer);
        Rid uniformSet = ShaderHelper.renderingDevice.UniformSetCreate(new Array<RDUniform> {uniform}, shader, 0);

        Rid pipeline = ShaderHelper.renderingDevice.ComputePipelineCreate(shader);
        long computeList = ShaderHelper.renderingDevice.ComputeListBegin();
        ShaderHelper.renderingDevice.ComputeListBindComputePipeline(computeList, pipeline);
        ShaderHelper.renderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        ShaderHelper.renderingDevice.ComputeListDispatch(computeList, (uint)agentsNum/2, 1,1);
        ShaderHelper.renderingDevice.ComputeListEnd();

        ShaderHelper.renderingDevice.Submit();
        ShaderHelper.renderingDevice.Sync();

        byte[] outBytes = ShaderHelper.renderingDevice.BufferGetData(buffer);
        Agent[] output = AgentPacker.PackBytesToAgentArray(outBytes);

        for (int i = 0; i<agentsNum; i++) {
            GD.Print("in x: ", agents[i].x, " | out x: ", output[i].x, " | angle: ", agents[i].angle);
            GD.Print("in y: ", agents[i].y, " | out y: ", output[i].y);
            GD.Print("_________________________________________");
        }

        GD.Print("HELLO! TEST");
    }

    private void CreateImage() {
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);


    }
}