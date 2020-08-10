#version 330 core

out vec4 FragColor;
//in vec3 vertex_color;
//in vec3 vertex_normal;

// GT: Update In/Out
in vec3 vertexInViewToFS;
in vec3 normalInViewToFS;
in vec4 colorInVertex;
vec4 colorInPerPixel = vec4(0, 0, 0, 0);
// GT: Light Structure
struct LightInfo{
    vec4 position;
    vec4 spotDirection;
    vec4 La;            // Ambient light intensity
    vec4 Ld;            // Diffuse light intensity
    vec4 Ls;            // Specular light intensity
    float spotExponent;
    float spotCutoff;
    float constantAttenuation;
    float linearAttenuation;
    float quadraticAttenuation;
};
struct MaterialInfo{
    vec4 Ka;
    vec4 Kd;
    vec4 Ks;
    float shininess;
};
uniform int lightIdx;            // Use this variable to contrl lighting mode
uniform mat4 shaderV;                // Camera viewing transformation matrix
uniform LightInfo light[3];
uniform MaterialInfo material;
uniform int vertex_or_perpixel;
vec4 directionalLight(vec3 N, vec3 V){
    vec4 lightInView = shaderV * light[0].position;    // the position of the light in camera space
    vec3 S = normalize(lightInView.xyz + V);
    vec3 H = normalize(S + V);                        // Half vector
    // GT: calculate diffuse coefficient and specular coefficient here
    float dc = dot(N,S);
//    float sc = pow(max(dot(N, H), 0), 64);
    float sc = pow(max(dot(N, H), 0), material.shininess);
    return light[0].La * material.Ka + dc * light[0].Ld * material.Kd + sc * light[0].Ls * material.Ks;
}

vec4 pointLight(vec3 N, vec3 V){
    // GT: Calculate point light intensity here
    vec4 lightInView = shaderV * light[1].position;    // the position of the light in camera space
//    lightInView *=-1;
    vec3 S = normalize(lightInView.xyz + V);
    vec3 H = normalize(S + V);                        // Half vector
    // GT: calculate diffuse coefficient and specular coefficient here
    float dc = max(dot(N,S), 0);
//    float sc = pow(max(dot(N, H), 0), 64);
    float sc = pow(max(dot(N, H), 0), material.shininess);
//    float d = length(lightInView.xyz + V)-1;
    float d = length(vertexInViewToFS - lightInView.xyz);
    float fp = 1.0/(light[1].constantAttenuation + light[1].linearAttenuation * d + light[1].quadraticAttenuation * d * d);
    return light[1].La * material.Ka + fp * (dc * light[1].Ld * material.Kd + sc * light[1].Ls * material.Ks);
}

//vec4 spotLight(vec3 N, vec3 V){
//    // GT: Calculate spot light intensity here
//    vec4 lightInView = shaderV * light[2].position;    // the position of the light in camera space
//    vec3 S = normalize(lightInView.xyz + V);
//    vec3 H = normalize(S + V);                        // Half vector
//    // GT: calculate diffuse coefficient and specular coefficient here
//    float dc = max(dot(N,S), 0);
////    float sc = pow(max(dot(N, H), 0), 64);
//    float sc = pow(max(dot(N, H), 0), material.shininess);
//    float d = length(lightInView.xyz + V)-2;
//    float fp = 1 / (light[2].constantAttenuation + light[2].linearAttenuation * d + light[2].quadraticAttenuation * d * d);
//    float theta = dot(normalize(-(lightInView.xyz + V)), normalize(light[2].spotDirection.xyz));
//    float spotlightEffect = 0;
//    if(theta > cos(light[2].spotCutoff)){
//      // do lighting calculations
//      spotlightEffect = pow( max( dot(-S,light[2].spotDirection.xyz), 0), light[2].spotExponent);
//    }
//    return light[2].La * material.Ka + fp * spotlightEffect * (dc * light[2].Ld * material.Kd + sc * light[2].Ls * material.Ks);
//}
vec4 spotLight(vec3 N, vec3 V){
    // GT: Calculate spot light intensity here
    vec4 lightInView = shaderV * light[2].position;
    vec3 S = normalize(lightInView.xyz + V);
    vec3 H = normalize(S + V);
    // GT: also need diffuse and specular coefficient
    float dc = dot(N, S);
//    float sc = pow(max(dot(N, H), 0), 64);
    float sc = pow(max(dot(N, H), 0), material.shininess);
//    vec3 VP = vertexInViewToFS - lightInView.xyz;
    float Distance = length(vertexInViewToFS - lightInView.xyz);
    vec3 spotDirectionInView = vec4(shaderV * light[2].spotDirection).xyz;
    float spotExponent = light[2].spotExponent;
    float spotDot = dot(normalize(vertexInViewToFS - lightInView.xyz), normalize(spotDirectionInView));
    float cutoff = cos(light[2].spotCutoff);
    float spotFactor = (spotDot > cutoff) ? pow(spotDot, spotExponent) : 0.0;
    float f_att = min(1.0 / (light[2].constantAttenuation +
                      light[2].linearAttenuation * Distance +
                      light[2].quadraticAttenuation * Distance * Distance), 1) * spotFactor;
    return light[2].La * material.Ka + (dc * light[2].Ld * material.Kd + sc * light[2].Ls * material.Ks) * f_att;
}
void main() {
//	FragColor = vec4(vertex_normal, 1.0f);
    vec3 N = normalize(normalInViewToFS);        // N represents normalized normal of the model in camera space
    vec3 V = -vertexInViewToFS;    // V represents the vector from the vertex of the model to the camera position
//    colorInPerPixel = vec4(0, 0, 0, 0);
    // Handle lighting mode
    if(lightIdx == 0){
        colorInPerPixel += directionalLight(N, V);
    }
    else if(lightIdx == 1){
        colorInPerPixel += pointLight(N, V);
    }
    else if(lightIdx == 2){
        colorInPerPixel += spotLight(N ,V);
    }
    if (vertex_or_perpixel == 1) FragColor = colorInVertex;
    else FragColor = colorInPerPixel;
}
