#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 1, local_size_z = 1) in;

struct Agent {
    float x;
    float y;
    float angle;
};

layout(set = 0, binding = 0, std430) restrict buffer AgentBuffer {
    Agent agents[];
} agentBuffer;
layout(rgba8, binding = 1) restrict uniform image2D colorMap;
layout(set = 0, binding = 2, std430) restrict buffer SettingsBuffer {
    int scanDist;
    float speed;
} settings;

bool isPosInside(ivec2 coords, ivec2 size) {
    return (coords.x < size.x && coords.x > 0 && coords.y < size.y && coords.y > 0);
}

ivec2 clampInside(ivec2 coords, ivec2 size) {
    return clamp(coords, ivec2(1,1), size);
}

float PI = 3.141592654;

int scanDist = settings.scanDist;
float movespeed = settings.speed;

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

float random(vec2 st)
{
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void main() {

    ivec2 imgSize = imageSize(colorMap);
    vec4 color = vec4(1.0,0.0,0.0,1.0);

    Agent agent = agentBuffer.agents[gl_GlobalInvocationID.x];

    vec2 heading = vec2(cos(agent.angle) * movespeed, sin(agent.angle) * movespeed);

    agent.x += heading.x;
    agent.y += heading.y;

    float randHeading = random(vec2(agent.x, agent.y));
    randHeading = (randHeading-0.5)/2.0;

    ivec2 coords = ivec2(int(agent.x), int(agent.y));
    float currentAvg = senseAverageOf(coords);
    if (isPosInside(coords, imgSize)) {
        vec4 prevColor = imageLoad(colorMap, coords);
        prevColor.r = 1;

        if (currentAvg > 0.95) {
            prevColor.g += (1.0 - currentAvg);
            prevColor.g = clamp(prevColor.g, 0.0, 0.5);
        }
        imageStore(colorMap, coords, prevColor);
    } else {
        ivec2 insidePos = clampInside(coords, imgSize);
        agent.x = float(insidePos.x);
        agent.y = float(insidePos.y);

        ivec2 cenvec = ivec2(imgSize.x/2 - insidePos.x, imgSize.y/2 - insidePos.y);
        if (cenvec.x == 0 && cenvec.y == 0) {
            agent.angle -= PI/2;
        } else {
            float cenangl = atan(cenvec.y, cenvec.x);
            agent.angle = cenangl;
        }

    }

    float leftAngle = agent.angle - PI/4.0;
    float rightAngle = agent.angle + PI/4.0;

    ivec2 leftOff = ivec2(cos(leftAngle) * scanDist, sin(leftAngle) *scanDist);
    ivec2 rightOff = ivec2(cos(rightAngle) * scanDist, sin(rightAngle) *scanDist);
    ivec2 cenOff = ivec2(cos(agent.angle) * scanDist, sin(agent.angle) *scanDist);

    float leftAvg = senseAverageOf(coords + leftOff);
    float rightAvg = senseAverageOf(coords + rightOff);
    float cenAvg = senseAverageOf(coords + cenOff);

    if (rightAvg > cenAvg || leftAvg > cenAvg) {
        if (leftAvg > rightAvg) {
            agent.angle -= PI/8 * leftAvg;
        } else {
            if (rightAvg > leftAvg) {
                agent.angle += PI/8 * rightAvg;
            }
        }
    }

    agent.angle += randHeading;
    agent.angle = mod(agent.angle, PI*2);

    //Save the changes to the agent in the buffer:
    agentBuffer.agents[gl_GlobalInvocationID.x].x = agent.x;
    agentBuffer.agents[gl_GlobalInvocationID.x].y = agent.y;
    agentBuffer.agents[gl_GlobalInvocationID.x].angle = agent.angle;
}