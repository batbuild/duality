<root dataType="Struct" type="Duality.Resources.FragmentShader" id="129723834">
  <source dataType="String">#version 440

uniform sampler2D mainTex;

void main()
{
	gl_FragColor = vec4(gl_Color.rgb, step(0.5, texture2D(mainTex, gl_TexCoord[0].st).a));
}</source>
  <sourcePath dataType="String">Source\Media\Default\FragmentShader\Picking.frag</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
