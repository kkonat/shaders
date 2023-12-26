precision mediump float; // Add default precision qualifier

float noise(vec2 p){
    return fract(sin(dot(p.xy, vec2(12.9898,78.233))) * 43758.5453);
}   
// iq palette   
vec3 pal( in float t, in vec3 a, in vec3 b, in vec3 c, in vec3 d )
{
    return a + b*cos( 6.28318*(c*t+d) );
}
vec3 pal1( float t){
    return pal( t, vec3(0.8,0.5,0.4),vec3(0.2,0.4,0.2),vec3(2.0,1.0,1.0),vec3(0.0,0.25,0.25) );
}
mat2 rot(float a) {
    float s=sin(a),c=cos(a);
    return mat2(c,-s,s,c);
} 
float hash21(vec2 p) {
    p = fract(p*vec2(123.34,456.21));
    p += dot(p,p+45.32);
    return fract(p.x*p.y);
}
vec4 diamond(vec2 uv, vec2 id, vec2 shft,float tm, float flare){
    float d;
    vec2 t = uv+shft;
    vec2 offs;
    id = 1.0 + (id-shft)*123.321;
    offs.x = hash21(id)*(0.5*sin(tm*hash21(id*13.1)))-0.25;
    offs.y = hash21(id*11.23)*0.5*cos(tm*hash21(id*7.0))-0.25;
    t+=offs;
    for( int i=1; i<6; i++ ){
        
        t = t.xy*rot(0.0115*tm*hash21(id)*float(i*4));
        // uv += 0.5;

        float rays = max(0., 1.-abs(t.x*t.y*500.)); 
        d += rays*flare;
    }
    d = 10.5/length(t)*+d*d;
    vec3 col = pal1( id.x*id.y);
    return vec4(col*d,clamp(12.0-d*2.0,0.0,1.0));
}


vec4 layer(vec2 uv, float mult, float speed){
    float tm = iTime*speed;
    uv *= rot(tm*0.05);
    vec2 id = floor(uv*mult);
    uv= fract(uv*mult);

   // shimmer effect
   // uv -= 0.5+vec2(0.5*(0.10*vec2(noise(uv*iTime*-2.0),noise(uv*iTime*10.0))));   
   
    vec2 p = uv-0.5;
    
    vec4 dc = vec4(0.0);
    dc = diamond(p, id, vec2(0.0),tm,0.2);
    dc += diamond(p, id,vec2(1.0,0.0), tm,0.2);
    dc += diamond(p, id,vec2(0.0,1.0), tm,0.2);
    dc += diamond(p, id,vec2(1.0,1.0), tm,0.2);
    
    dc /=4.0;
    return  dc;
}

void main()
{
    float mult = 3.0;

    vec2 uv = gl_FragCoord.xy / vec2(iResolution.x, iResolution.y); 
    uv.x *= iResolution.x / iResolution.y;  
    uv *= iResolution.x / iResolution.y; 
    uv -= 0.5;
    
    float speed = 1.0;
    vec4 c;
    c += layer(uv, mult, speed);
    c +=  layer(uv, mult*1.73, -speed*2.1);
    gl_FragColor = c/1.0;
}