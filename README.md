Kethane-Customized
=======

(This major modification or rewrite of the mod is for personal use only mainly, I am not planning to puplish it etc. unless I can take out the parts I rewrote and did and publish that as a seperate plugin module or something)

With the standard heat management that comes with normal resource harvesters

This will end up as a rewrite, Kethane mining modules will no longer be seperate modules, all other modules will stay seperate.

Kethane mining modules will now be more dynamic and will plug into custom and non custom ResourceHarvester modules and manage&disable these if no resources are present and also substract neccessary resources from below resource node if these core base modules are active by grabbing their resource rate (I had to run my "ExtendedInstalled" to expose a field called _resFlow from these core base ResourceHarvesters modules so the mod will break unless you also do that, this will lead to a minor rewrite of the core game logic, many ways to do it with many libraries to help you with this)

Building
--------

just build it via .csproj file
