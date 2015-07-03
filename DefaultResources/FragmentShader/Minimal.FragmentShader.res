<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

in vec2 iTexCoord;
in vec4 colour;

out vec4 oColour;

uniform sampler2D mainTex;

void main()
{
	oColour = texture2D(mainTex, iTexCoord);
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\Minimal.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
