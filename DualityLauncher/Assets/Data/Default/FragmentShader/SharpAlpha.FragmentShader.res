<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

uniform sampler2D mainTex;
uniform float smoothness;

const float Gamma = 2.2;

in vec2 iTexCoord;
in vec4 colour;

out vec4 oColour;

void main()
{
	// Retrieve base color
	vec4 texClr = texture2D(mainTex, iTexCoord);
	
	// Do some anti-aliazing
	float w = clamp(smoothness * (abs(dFdx(iTexCoord.x)) + abs(dFdy(iTexCoord.y))), 0.0, 0.5);
	float a = smoothstep(0.5 - w, 0.5 + w, texClr.a);

	// Perform Gamma Correction to achieve a linear attenuation
	texClr.a = pow(a, 1.0 / Gamma);

	// Compose result color
	oColour = texClr * colour; 
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\SharpAlpha.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
