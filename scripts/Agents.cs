using System;
using Godot;
using System.Runtime.InteropServices;

namespace Agents {
    public struct Agent {
        public float x;
        public float y;
        public float angle;
    }

    public static class AgentPacker {

        static int floatSize = sizeof(float);
        public static int agentSize = Marshal.SizeOf(typeof(Agent));
        public static void PackAgentInByteArray(Agent agent, byte[] dest, int offset) {
            int floatSize = sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(agent.x), 0, dest, offset + floatSize*0, floatSize);
            Buffer.BlockCopy(BitConverter.GetBytes(agent.y), 0, dest, offset + floatSize*1, floatSize);
            Buffer.BlockCopy(BitConverter.GetBytes(agent.angle), 0, dest, offset + floatSize*2, floatSize);
        }

        public static Agent[] PackBytesToAgentArray(byte[] bytes) {
            Agent[] agents = new Agent[bytes.Length / agentSize];
            for (int i = 0; i < (agents.Length); i++) {
                agents[i].x = BitConverter.ToSingle(bytes, i*agentSize);
                agents[i].y = BitConverter.ToSingle(bytes, i*agentSize + floatSize);
                agents[i].angle = BitConverter.ToSingle(bytes, i*agentSize + floatSize*2);
            }
            return agents;
        }
    }
}