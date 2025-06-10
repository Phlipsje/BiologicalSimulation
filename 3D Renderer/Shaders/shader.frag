#version 430 core
out vec4 FragColor;
in vec2 uv;

uniform vec3 cameraPos;
uniform vec3 cameraFront;
uniform vec3 cameraUp;
uniform vec3 cameraRight;
uniform float aspect;

struct Sphere {
    vec3 center;
    float radius;
    vec3 color;
    float padding;
};

layout(std430, binding = 0) buffer SpheresBuffer {
    Sphere spheres[];
};
uniform int sphereCount;

float RaySphereIntersect(vec3 ro, vec3 rd, vec3 center, float radius)
{
    vec3 oc = ro - center;
    float a = dot(rd, rd);
    float b = 2.0 * dot(oc, rd);
    float c = dot(oc, oc) - radius * radius;
    float d = b*b - 4.0*a*c;
    if (d < 0.0) return -1.0;
    return (-b - sqrt(d)) / (2.0 * a);
}

void main()
{
    vec2 screenPos = uv * 2.0 - 1.0;
    screenPos.x *= aspect;

    vec3 rayDir = normalize(cameraFront + screenPos.x * cameraRight + screenPos.y * cameraUp);
    vec3 rayOrigin = cameraPos;

    float minT = 1e20;
    int hitIndex = -1;

    for (int i = 0; i < sphereCount; ++i)
    {
        float t = RaySphereIntersect(rayOrigin, rayDir, spheres[i].center, spheres[i].radius);
        if (t > 0.0 && t < minT)
        {
            minT = t;
            hitIndex = i;
        }
    }

    //If pixel should be sphere color
    if (hitIndex >= 0)
    {
        vec3 hitPos = rayOrigin + minT * rayDir;
        vec3 normal = normalize(hitPos - spheres[hitIndex].center);
        float lighting = dot(normal, normalize(vec3(1.0, 1.0, -1.0))) * 0.5 + 0.5;
        FragColor = vec4(spheres[hitIndex].color * lighting, 1.0);
    }
    else //If sphere should be skybox (background) color
    {
        float t = clamp(rayDir.y * 0.5 + 0.5, 0.0, 1.0);
        vec3 skyColor = mix(vec3(0.1, 0.2, 0.6), vec3(1.0, 1.0, 1.0), t);
        FragColor = vec4(skyColor, 1.0);
    }
}
