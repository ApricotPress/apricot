# Apricot
Core game engine project. Holds as little dependencies as possible and provides low-level part of engine.

# Apricot.Build
Build scripts that are used for automatic baking of artifacts, updating git submodules and other CI\CD stuff.

# Apricot.Essentials
Bootstrapping and essential part of the engine that has easy-to-use APIs. Also holds Assets you would probably like to 
use. They are embedded into assembly (look for [EmbeddedAssetsSource](./Apricot/Assets/Embedded/EmbeddedAssetsSource.cs)
to find more info about it) and artifacts are produced and cached with Apricot.Build project.

Other notable parts of essentials:
- Bootstrap (and Game system)
- ImGui wrapper for Apricot
- Image importer using [StbImageSharp](https://github.com/StbSharp/StbImageSharp)

# Apricot.Jobs
Simple multi-thread job system. 

# Apricot.OpenGl
OpenGL backend for graphics. Can be used for WebGL if compiled with emscripten.

# Apricot.Sdl
SDL3 backend of apricot. Also implements graphics API using SDL GPU api. Does not include importers as they are pretty 
heavy because of Microsoft DX shader compiler used by SDL_shadercross

# Apricot.Sdl.GlBindings
Bindings that help to get OpenGL context from SDL windows if you want to use OpenGL on top of SDL. Used for WebGL, for 
instance.

# Apricot.Sdl.Importers
Asset importers for SDL gpu graphics. Mainly, it utilizes SDL_shadercross for shader compilation into SDL_gpu compatible
shader byte codes.

# Apricot.Sample
Example of runnable game.

# Apricot.WebSample
Example of runnable in browser game.

# Apricot.Utils
Some fancy extensions used inside the engine.
