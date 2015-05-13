<root dataType="Struct" type="Duality.Resources.VertexShader" id="129723834">
  <source dataType="String">#version 440

attribute float animBlend;
varying float animBlendVar;

void main()
{
	gl_Position = ftransform();
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_FrontColor = gl_Color;
	animBlendVar = animBlend;
}</source>
  <sourcePath dataType="String">Source\Media\Default\VertexShader\SmoothAnim.vert</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
