# Apricot

Apricot is my game engine I decided to make in free time out of boredom. If I succeed it should be usable for creation
of simple 2d games for game jams. Core concepts I want to follow during development:
- Engine should support extreme fast iteration speed, meaning support of .NET hot reload and general flexibility
- Support both desktop and web builds
- "Good enough" principle without too much of overengineering - I am mostly jamming, not creating 3D high-fidelity apps
- Test coverage and proper CI 

As of writing this nothing of this is supported as I am getting started, but let's see where it would take me

# Etymology
Apricot is a good fruit to make jam and put it into jar

# Dependencies
Project is developed with .NET 9 and C# 13

Engine itself is planned to be Graphical API\Backend\OS-agnostic, yet main implementation as for now is working with use
of SDL3 which was pre-compiled.

# Building
First, clone repository with all submodules. This should be enough to just run `dotnet run` on sample projects. Although
if you want to update all dependencies to latest possible versions you may want to use [cake](https://github.com/cake-build/cake]
build project. At the moment it can only do:
- Build SDL3 from source and copy its artifact to Deps folder
- Regenerate SDl3-CS bindings (without calling c2ffi)
- Build Apricot and Apricot.Essential projects

Soon I want to add web deps build to it, c2ffi run if I manage to build it for MacOS, and prebuilding assets once I
finish assets framework in Engine :)

# Some other notes 
- I mainly work with Unity yet have experience of various game engines for inspiration. Although, I don't hesitate to 
see what's going on inside [Foster](https://github.com/FosterFramework/Foster), [libGDX](https://libgdx.com), and some
others
- Atm engine looks pretty close to Foster, FNA but later on I have plans to extend with proper object and scene systems
