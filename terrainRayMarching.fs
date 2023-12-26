precision mediump float; // Add default precision qualifier for float type

vec2 hash2( vec2 p ) 
{
    const vec2 k = vec2( 0.3183099, 0.3678794 );
    float n = 111.0*p.x + 113.0*p.y;
    return fract(n*fract(k*n));
}
// noise
float noise( in vec2 p )
{
    vec2 i = floor( p );
    vec2 f = fract( p ); 
    vec2 u = f*f*(3.0-2.0*f);
    return -1.0+2.0*mix( mix( dot( hash2( i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                               dot( hash2( i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                         mix( dot( hash2( i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                               dot( hash2( i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

float dist( vec3 p ){
    p /= 3.0;
    float n = 0.0;
    float a = 0.5224235;
    mat2 rot = mat2( cos(a), sin(a), -sin(a), cos(a) );
    
    n += noise(p.xz*0.9)*2.2;
     n += noise((p.xz*1.9+12.232)*rot)*1.1;
     n += noise((p.xz*3.9+78.321)*rot)*0.3;
     n += noise((p.xz*6.9+12.1)*rot)*0.01;
     n += noise((p.xz*32.9+1345.0)*rot)*0.02;

    n /= 4.0;
    return p.y -  n;
}
float raymarch( vec3 ro, vec3 rd, float tmin, float tmax ){
    float t = tmin;
    for( int i=0; i<240 && t<tmax; i++ ){
        float h = dist( ro+rd*t );
        if( h<0.0001 ) break;
        t += h;
    }
    return t;
}


mat3 setCamera( in vec3 ro, in vec3 ta, float cr )
{
    vec3 cw = normalize(ta-ro);
    vec3 cp = vec3(sin(cr), cos(cr),0.0);
    vec3 cu = normalize( cross(cw,cp) );
    vec3 cv = normalize( cross(cu,cw) );
    return mat3( cu, cv, cw );
}

vec3 calcNormal( vec3 pos ){
    vec2 e = vec2(1.0,-1.0)*0.0005;
    return normalize( e.xyy*dist( pos + e.xyy ) + 
                      e.yyx*dist( pos + e.yyx ) + 
                      e.yxy*dist( pos + e.yxy ) + 
                      e.xxx*dist( pos + e.xxx ) );
}
// iq color map sunset
vec3 map( float v, float i )
{
    vec3 a = vec3(0.2,0.4,0.0);
    vec3 b = vec3(0.0,0.3,0.0);
    vec3 c = vec3(0.5,0.5,0.3);
    vec3 d = vec3(0.9,0.3,0.1);
    return a + b*cos( 6.28318*(c*v+d*i) );
    
}
void main() {
    vec4 sky = vec4(0.4,0.4,0.1,0.1);
    const float tmax = 100.0;
    vec2 o = hash2( vec2(iFrame,1) ) - 0.5;
    
    vec2 p = (2.0*(gl_FragCoord.xy+o)-iResolution.xy)/ iResolution.y;
    
//    vec3 sun = vec3( cos(0.1*iTime), sin(0.1*iTime), 0.0 );
    vec3 sun = vec3( 10.0, 20.0, -100.0 );
    // camera
    float time = iTime;
    vec3 ro = vec3(0.0, 0.0, 26.0);
    vec3 ta = vec3(0.0, -1.0, 0.0 );
    
    //ro += vec3(10.0*sin(0.02*time),0.0,-10.0*sin(0.2+0.031*time));
    
     ro.x -= 80.0*sin(0.01*time);
     ro.y  = 1.0+2.0*cos(0.1*time);
     ta.x -= 86.0*sin(0.01*time);

    // ray
    mat3 ca = setCamera( ro, ta, 0.0 );
    vec3 rd = ca * normalize( vec3(p,16.1));

    int   obj = 0;
    float t = raymarch( ro, rd, 1.0, tmax );
     if( t<tmax ){
        vec3 pos = ro + t*rd;
    vec3 difc = map( sin(pos.y), 0.0 );

        vec3 nor = calcNormal( pos-rd*0.01 );
        vec3 lightDir = normalize( sun );
        float dif = clamp( dot( nor, lightDir ), 0.0, 1.0 );
        float spec = pow( clamp( dot( reflect(normalize(lightDir-pos),nor), rd ), 0.0, 1.0 ), 16.0 );
        vec3 col = vec3(0.0);
        col += 0.80*difc*vec3(1.0,0.9,0.6);
        col += spec*sky.xyz;
        float fog = clamp(0.0,1.0,1.0-exp( -0.000345*t*t ));
        col = mix( col, sky.xyz, fog );
        gl_FragColor = vec4( col, 1.0 );
    

    }else{
        gl_FragColor = vec4(sky.xyz,1.0);
        }
    
}