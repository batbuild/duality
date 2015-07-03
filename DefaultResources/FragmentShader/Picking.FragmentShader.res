<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

uniform sampler2D mainTex;

in vec2 iTexCoord;
in vec4 iColour;

out vec4 oColour;

void main()
{
	oColour = vec4(iColour.rgb, step(0.5, texture2D(mainTex, iTexCoord).a));
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\Picking.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
