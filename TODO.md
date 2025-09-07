This file holds more of "strategical" TODOs I wish to implement eventually.

# Shaders

- Implement HLSL to GLSL compilation
  Tries to use GLSL -> spv -> HLSL with glslang and shader-cross but could no make it run 

- Move away from DX compiler

  I prefer HLSL over GLSL because of very subjective reasons and wish to avoid using GLSL as main shading language.
  But using HLSL puts me in position of bundling HLSL compiler with engine. As of right now I simply use Microsoft
  DX compiler as it is how SDL_shadercross was built. But it is very large portion of software with many
  dependencies and even proprietary OS-locked parts (dxil.dll). Possible option for resolving this issue is to use
  Slang shading language which seems universal and supports all needed platforms.

- Use C# as shader language

  There is a PoC by mellinoe called [ShaderGen](https://github.com/mellinoe/ShaderGen) that could be used as entry point
  and may be rewritten to source generators. I also was thinking of producing SPIR-V directly from C# but have not yet
  figured out where to find proper examples and docs for that idea.

# Asset importing

- Asset import is very robust, produces artifacts for all platforms and then stores them in memory even when they
  are unused. Eventually it needs to be updated to work properly

- Add support for ome type of bundles with compression

- ASTC support for textures

- Import settings nearby asset files 
