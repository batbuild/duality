<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

uniform sampler2D mainTex;

in vec2 iTexCoord;
in vec4 colour;
in float animBlendVar;

out vec4 oColour;


void main()
{
	// Retrieve frames
	vec4 texClrOld = texture2D(mainTex, iTexCoord);
	vec4 texClrNew = texture2D(mainTex, iTexCoord);

	// This code prevents nasty artifacts when blending between differently masked frames
	float accOldNew = (texClrOld.w - texClrNew.w) / (texClrOld.w + texClrNew.w);
	accOldNew *= mix(min(min(animBlendVar, 1.0 - animBlendVar) * 4.0, 1.0), 1.0, abs(accOldNew));
	texClrNew.xyz = mix(texClrNew.xyz, texClrOld.xyz, max(accOldNew, 0.0));
	texClrOld.xyz = mix(texClrOld.xyz, texClrNew.xyz, max(-accOldNew, 0.0));

	// Blend between frames
	oColour = colour * mix(texClrOld, texClrNew, animBlendVar);
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\SmoothAnim.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
