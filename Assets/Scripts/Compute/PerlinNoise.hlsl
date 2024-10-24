// preview of perlin noise implementation


float scale;
float iTime;
float TimeSpeed;

// returns a pseudo-random float between 0 and 1
float rand(float2 position)
{
    position = position;
    return frac(sin(dot(position, float2(12.9898,78.233))) * 43758.5453);
}

// returns a pseudo-random angle from 0 to 2 * pi
float randomAngle(float2 position)
{
    return rand(position) * 2 * 3.1415;
}

// returns a pseudo-random unit length vector
float2 randomVector(float2 position)
{
    position = position + 0.02;
    float angle = randomAngle(position);
    float2 vec = float2(cos(angle), sin(angle));
    vec = vec * 12345;
    vec = sin(vec + iTime * TimeSpeed / 50);
    return vec;
}


float4 main(in float2 uv: TEXCOORD0) : SV_TARGET {

    uv = uv / scale;

    float3 black = float3(0.0, 0.0, 0.0);
    float3 white = float3(1.0, 1.0, 1.0);
    float3 color = black;

    // set up cell in grid
    float2 gridId = floor(uv);
    float2 gridUv = frac(uv);

    // find the coords of grid corners
    float2 bl = gridId + float2(0.0, 0.0);
    float2 br = gridId + float2(1.0, 0.0);
    float2 tl = gridId + float2(0.0, 1.0);
    float2 tr = gridId + float2(1.0, 1.0);


    // find random gradient for each grid corner
    float2 gradBl = randomVector(bl);
    float2 gradBr = randomVector(br);
    float2 gradTl = randomVector(tl);
    float2 gradTr = randomVector(tr);


    // find the grid cell of the point
    float2 gridCell = gridId + gridUv;


    // find displacement vectors from each corner to candidate point (offset vector)
    float2 offset1 = gridUv - float2(0,0);
    float2 offset2 = gridUv - float2(1,0);
    float2 offset3 = gridUv - float2(0,1);
    float2 offset4 = gridUv - float2(1,1);

    // calculate the dot products of gradients + distances
    float dot1 = dot(gradBl, offset1);
    float dot2 = dot(gradBr, offset2);
    float dot3 = dot(gradTl, offset3);
    float dot4 = dot(gradTr, offset4);

    // smooth out gridUvs
    gridUv = smoothstep(0.1, 1, gridUv);

    // interpolate between the dot products
    float bottom = lerp(dot1, dot2, gridUv.x);
    float top = lerp(dot3, dot4, gridUv.x);
    float perlin = lerp(bottom, top, gridUv.y);

    // display perlin noise
    color = float3(perlin + 0.2, perlin + 0.2, perlin + 0.2);

    return float4(color, 1.0);
}