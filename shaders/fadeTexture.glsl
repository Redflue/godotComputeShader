#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

struct Agent {
    float x;
    float y;
    float angle;
};

layout(set = 0, binding = 0, std430) restrict buffer AgentBuffer {
    Agent agents[];
} agentBuffer;
layout(rgba8, binding = 1) restrict uniform image2D colorMap;

bool isPosInside(ivec2 coords, ivec2 size) {
    return (coords.x < size.x && coords.x > 0 && coords.y < size.y && coords.y > 0);
}

ivec2 clampInside(ivec2 coords, ivec2 size) {
    return clamp(coords, ivec2(1,1), size);
}

float PI = 3.141592654;

float senseAverageOf(ivec2 pos) {
    float surroundingAverage = 0.0;
    for (int x = -1; x < 2; x++) {
        for (int y = -1; y < 2; y++) {
            vec4 c = imageLoad(colorMap, pos + ivec2(x,y));
            surroundingAverage += c.r;
        }
    }
    return surroundingAverage/9.0;
}

vec4 senseAverageVecOf(ivec2 pos) {
    vec4 surroundingAverage = vec4(0.0,0.0,0.0,1.0);
    for (int x = -1; x < 2; x++) {
        for (int y = -1; y < 2; y++) {
            vec4 c = imageLoad(colorMap, pos + ivec2(x,y));
            surroundingAverage += c;
        }
    }
    surroundingAverage = surroundingAverage/9.0;
    surroundingAverage.a = 1.0;
    return surroundingAverage;
}

void main() {

    ivec2 imgSize = imageSize(colorMap);
    ivec2 coords = ivec2(gl_GlobalInvocationID.xy);

    vec4 surroundingAverage = senseAverageVecOf(coords);
    surroundingAverage.r *= 0.99;
    surroundingAverage.g *= 0.95;
    surroundingAverage.b *= 0.99;
    
    vec4 color = surroundingAverage;
    // vec4 color = imageLoad(colorMap, coords);
    // color.r -= surroundingAverage;
    // color.g -= 0.05;
    // color.b -= 0.05;
    // vec4 color = vec4(coords.x / float(imgSize.x), 0.0, coords.y / float(imgSize.y), 1.0);

    //imageStore(colorMap, coords, color);
    imageStore(colorMap, coords, color);
    //imageStore()
}