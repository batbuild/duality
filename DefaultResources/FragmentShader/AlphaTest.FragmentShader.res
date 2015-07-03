<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

in vec2 iTexCoord;
in vec4 colour;

out vec4 oColour;

uniform sampler2D mainTex;

void main()
{
	vec4 colour = texture2D(mainTex, iTexCoord);

	if(colour.a &lt; 0.5)
		discard;

	oColour = colour;
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\AlphaTest.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
