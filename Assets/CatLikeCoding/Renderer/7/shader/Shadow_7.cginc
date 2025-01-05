#if !defined(SHADOW_7_INCLUDED)
#define SHADOW_7_INCLUDED

#include "UnityCG.cginc"

struct VertexData {
    float4 position : POSITION;
};

float4 MyShadowVertexProgram (VertexData v) : SV_POSITION {
    float4 position = UnityObjectToClipPos(v.position);
    return UnityApplyLinearShadowBias(position);
}

float4 MyShadowFragmentProgram () : SV_TARGET {
    return 1;
}

#endif