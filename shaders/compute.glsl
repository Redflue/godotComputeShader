#[compute]
#version 450

layout(local_size_x = 2, local_size_y = 1, local_size_z = 1) in;

struct Agent {
    float x;
    float y;
    float angle;
};

layout(set = 0, binding = 0, std430) restrict buffer MyDataBuffer {
    Agent agents[];
} my_data_buffer;

void main() {
    my_data_buffer.agents[gl_GlobalInvocationID.x].x *= 2.0;
    my_data_buffer.agents[gl_GlobalInvocationID.x].y *= 2.0;
}