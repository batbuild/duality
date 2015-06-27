<root dataType="Struct" type="Duality.Resources.VertexShader" id="129723834">
  <source dataType="String">#version 440

layout(location = 0) in vec4 colour;
layout(location = 1) in vec3 position;
layout(location = 2) in vec2 texCoord;

out vec2 iTexCoord;
out vec4 oColour;

uniform mat4 matProj;

void main()
{
	gl_Position = vec4(position, 1) * matProj;
	iTexCoord = texCoord;
	oColour = colour;
}</source>
  <sourcePath dataType="String">Source\Media\Default\VertexShader\Minimal.vert</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
