using Godot;
using System;
using Godot.Collections;
using Agents;
using Shaders;

public partial class node_2d : Node2D
{

    uint po2Dim = 512;
    Rid colorMapRid;
    RDUniform colorMapUniform;
    [Export] TextureRect texRect {get; set;}

    RDUniform agentUniform;
    RDUniform settingsUniform;

    Rid uniformSet;
    Rid pipeline;
    int agentsNum = 500000;

    Rid fadeShader;
    Rid fadePipeline;
    Rid agentShader;

    public override void _Ready()
    {
        po2Dim = (uint)Mathf.NearestPo2((int)po2Dim);

        base._Ready();
        ShaderHelper.CreateRendereringDevice();
        Rid shader = ShaderHelper.LoadShader("res://shaders/compute.glsl");
        CreateImage();

        // int agentsNum = 10;
        int sizeOfAgent = AgentPacker.agentSize;
        Agent[] agents = new Agent[agentsNum];
        byte[] agentBytes = new byte[agentsNum * sizeOfAgent];

        Random rand = new Random();
        for (int i = 0; i<agentsNum; i++) {
            int a = i*2;
            float randomAngle = rand.NextSingle() * Mathf.Pi*2;
            agents[i] = new Agent {
                x = (float)po2Dim/2,
                y = (float)po2Dim/2,
                angle = randomAngle
            };
            AgentPacker.PackAgentInByteArray(agents[i], agentBytes, i*sizeOfAgent);
        }
        
        Rid buffer = ShaderHelper.renderingDevice.StorageBufferCreate((uint)agentBytes.Length, agentBytes);
        RDUniform uniform = new RDUniform {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 0,
        };
        uniform.AddId(buffer);
        agentUniform = uniform;

        byte[] settingsBuffer = AgentPacker.CreateSettingsBuffer(3,1f);
        settingsUniform = ShaderHelper.CreateStorageBufferUniform(settingsBuffer, 2);
        agentShader = shader;

        uniformSet = ShaderHelper.renderingDevice.UniformSetCreate(new Array<RDUniform> {uniform, colorMapUniform, settingsUniform}, shader, 0);

        pipeline = ShaderHelper.renderingDevice.ComputePipelineCreate(shader);
        long computeList = ShaderHelper.renderingDevice.ComputeListBegin();
        ShaderHelper.renderingDevice.ComputeListBindComputePipeline(computeList, pipeline);
        ShaderHelper.renderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        ShaderHelper.renderingDevice.ComputeListDispatch(computeList, (uint)agentsNum/8, 1,1);
        ShaderHelper.renderingDevice.ComputeListEnd();

        //GD.Print();

        ShaderHelper.renderingDevice.Submit();
        ShaderHelper.renderingDevice.Sync();

        byte[] outBytes = ShaderHelper.renderingDevice.BufferGetData(buffer);
        Agent[] output = AgentPacker.PackBytesToAgentArray(outBytes);

        // for (int i = 0; i<agentsNum; i++) {
        //     GD.Print("in x: ", agents[i].x, " | out x: ", output[i].x, " | angle: ", agents[i].angle);
        //     GD.Print("in y: ", agents[i].y, " | out y: ", output[i].y);
        //     GD.Print("_________________________________________");
        // }

        GD.Print("HELLO! TEST");

        initFadeShader();

        RunFadeShader();

        DisplayImage();
    }

    private void GenerateUniformSet() {
        if (uniformSet != null)
            ShaderHelper.renderingDevice.FreeRid(uniformSet);
        uniformSet = ShaderHelper.renderingDevice.UniformSetCreate(new Array<RDUniform> {agentUniform, colorMapUniform, settingsUniform}, agentShader, 0);
    }

    private void QueueFadeShader() {
        long computeList = ShaderHelper.renderingDevice.ComputeListBegin();
        ShaderHelper.renderingDevice.ComputeListBindComputePipeline(computeList, fadePipeline);
        ShaderHelper.renderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        ShaderHelper.renderingDevice.ComputeListDispatch(computeList, po2Dim/8, po2Dim/8,1);
        ShaderHelper.renderingDevice.ComputeListEnd();
    }

    private void RunFadeShader() {
        QueueFadeShader();
        ShaderHelper.renderingDevice.Submit();
        ShaderHelper.renderingDevice.Sync();
    }

    private void initFadeShader() {
        fadeShader = ShaderHelper.LoadShader("res://shaders/fadeTexture.glsl");
        fadePipeline = ShaderHelper.renderingDevice.ComputePipelineCreate(fadeShader);
    }

    private void RunAgentCompute() {
        QueueComputing();
        ShaderHelper.renderingDevice.Submit();
        ShaderHelper.renderingDevice.Sync();
    }

    private void CreateImage() {
        var colorMapFormat = new RDTextureFormat
        {
            Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
            Width = po2Dim,
            Height = po2Dim,
            UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.CanCopyFromBit
        };

        colorMapRid = ShaderHelper.renderingDevice.TextureCreate(colorMapFormat, new RDTextureView());

        colorMapUniform = new RDUniform
        {
            UniformType = RenderingDevice.UniformType.Image,
            Binding = 1
        };
        colorMapUniform.AddId(colorMapRid);
    }

    private void DisplayImage() {
        var outBytes = ShaderHelper.renderingDevice.TextureGetData(colorMapRid, 0);
        var img = Image.CreateFromData((int)po2Dim, (int)po2Dim, false, Image.Format.Rgba8, outBytes);

        var imgTex = ImageTexture.CreateFromImage(img);
        if (texRect != null) {
            //GD.Print("display");
            texRect.Texture = imgTex;
        }
    }

    private void DebugAgentPrint(int agentNum) {}

    private void QueueComputing() {
        long computeList = ShaderHelper.renderingDevice.ComputeListBegin();
        ShaderHelper.renderingDevice.ComputeListBindComputePipeline(computeList, pipeline);
        ShaderHelper.renderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        ShaderHelper.renderingDevice.ComputeListDispatch(computeList, (uint)agentsNum/8, 1,1);
        ShaderHelper.renderingDevice.ComputeListEnd();
    }

    private void ComputeImageFade() {

    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (ShaderHelper.renderingDevice != null) {
            RunAgentCompute();
            RunFadeShader();
            DisplayImage();
        }
    }

    public void applySettings(int scanDist, float speed) {
        //settingsUniform.Free();

        byte[] settingsBuffer = AgentPacker.CreateSettingsBuffer(scanDist,speed);
        settingsUniform = ShaderHelper.CreateStorageBufferUniform(settingsBuffer, 2);
        GD.Print("apply settings ; ", scanDist, " : ", speed);
        GenerateUniformSet();
    }
}