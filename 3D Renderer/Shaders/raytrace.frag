#version 430 core

struct Sphere {
    vec3 center;
    float radius;
    vec3 color;
    float _padding;
};

layout(std430, binding = 0) readonly buffer Spheres {
    Sphere spheres[];
};

uniform vec3 camPos;
uniform mat4 invProj;
uniform vec2 resolution;

out vec4 fragColor;

bool intersectSphere(vec3 ro, vec3 rd, vec3 c, float r, out float t) {
    vec3 oc = ro - c;
    float b = dot(oc, rd);
    float d = b*b - dot(oc, oc) + r*r;
    if (d < 0.0) return false;
    float sqrtD = sqrt(d);
    float t0 = -b - sqrtD;
    float t1 = -b + sqrtD;
    t = (t0 > 0.0) ? t0 : t1;
    return t > 0.0;
}

void main() {
    vec2 uv = (gl_FragCoord.xy / resolution) * 2.0 - 1.0;
    vec4 clipPos = vec4(uv.x, uv.y, -1.0, 1.0);
    vec4 worldPos = invProj * clipPos;
    vec3 rayDir = normalize(worldPos.xyz - camPos);

    float minT = 1e9;
    vec3 hitColor = vec3(0);
    vec3 hitNormal;
    bool hit = false;

    for (int i = 0; i < spheres.length(); ++i) {
        float t;
        if (intersectSphere(camPos, rayDir, spheres[i].center, spheres[i].radius, t) && t < minT) {
            minT = t;
            vec3 hitPoint = camPos + t * rayDir;
            vec3 N = normalize(hitPoint - spheres[i].center);
            vec3 L = normalize(vec3(1, 1, -1));
            vec3 V = normalize(camPos - hitPoint);
            vec3 R = reflect(-L, N);
            float diff = max(dot(N, L), 0.0);
            float spec = pow(max(dot(R, V), 0.0), 32.0);
            float rim = pow(1.0 - max(dot(N, V), 0.0), 2.0);

            hitColor = spheres[i].color * diff + spec * vec3(1.0) + rim * 0.2;
            hitNormal = N;
            hit = true;
        }
    }

    if (!hit) {
        if(rayDir.y < 1000000000)
        {
            fragColor = vec4(0.0, 0.0, 0.0, 1.0);
            return;
        }

        if(rayDir.y > 0.1)
        {
            fragColor = vec4(1.0, 1.0, 1.0, 1.0);
            return;
        }
        
        float t = clamp(rayDir.y * 0.5 + 0.5, 0.0, 1.0);
        vec3 skyColor = mix(vec3(0.1, 0.2, 0.6), vec3(1.0, 1.0, 1.0), t);
        fragColor = vec4(skyColor, 1.0);
        return;
    }

    float outline = pow(1.0 - abs(hitNormal.z), 8.0);
    hitColor = mix(hitColor, vec3(0), outline);

    fragColor = vec4(hitColor, 1.0);
}
