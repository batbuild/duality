<root dataType="Struct" type="Duality.Resources.VertexShader" id="129723834">
  <source dataType="String">#version 440

layout(location = 0) in vec4 colour;
layout(location = 1) in vec3 position;
layout(location = 2) in vec2 texCoord;

// Camera-constant data
uniform    vec3  camPos;         // Position of the camera in world coordinates
uniform    float camZoom;        // Zoom factor of the camera.
uniform    float  camParallax;    // If true, 2D parallax projection is applied by the camera

uniform	   float vertexZOffset;  // Optional: The (sorting) Z offset that shouldn't affect parallax scale

uniform mat4 matProj;

out vec2 iTexCoord;
out vec4 iColour;

void main()
{
    // Apply parallax 2D projection
    float parallaxScale = camZoom;
    if (camParallax > 0)
    {
        // Determine object scale based on camera properties and relative vertex position
        parallaxScale = camZoom / max(position.z - camPos.z, 0);
    }

    // Transform vertex to view coordinates and account for parallax scale and Z-offset
    vec3 viewPos = position - camPos;
    viewPos.xy *= parallaxScale;
    viewPos.z += vertexZOffset;

    iTexCoord = texCoord;
    iColour = colour;
    gl_Position = vec4(viewPos, 1) * matProj;
}</source>
  <sourcePath dataType="String">Source\Media\Default\VertexShader\Minimal.vert</sourcePath>
</root>
<!-- XmlFormatterBase Document Separator -->
