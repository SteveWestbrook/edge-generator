# edge-generator

A [node.js](https://nodejs.org) package with .NET components that generates proxies to allow users of node to hold references to .NET objects in JavaScript code.

This package generates proxies, while the related [edge-reference](https://github.com/SteveWestbrook/edge-reference) package is used at runtime to bind the proxies to their .NET counterparts.

**Installation**

```
$ npm i edge-generator -g
```

**Usage**

```
edge-generator -a <assembly-path> [-t <target-directory>] Namespace.Type1 Namespace.Type2
```

Proxies will be generated for specified types contained in the assembly that is located at the path specified in <assembly-path>.  Each type, its base type, and any types referenced in its public members, will generate a JS file with a name corresponding to the fully-qualified type name.  These files will be written to the target directory specified, or to the current directory if no target directory is given.

**Example**

The following example:

```
edge-generator -a ./bin/Debug/ExampleCo.Utils.dll -t ./proxies ExampleCo.Utils.Connector ExampleCo.Utils.Widget ExampleCo.Utils.Factory
```

will generate three JavaScript files in the ./proxies directory, assuming the DLL specified exists, and contains the three types named.  The files generated will be named

ExampleCo-Utils-Connector.js
ExampleCo-Utils-Widget.js
ExampleCo-Utils-Factory.js

It is possible to rename generated files after generation.

When deploying proxy files, the .NET assemblies referenced by them must also be deployed, in the node project's root folder.

## Limitations
Several features of the .NET framework and the C# language are not supported at this time.  Notable missing features are below:

  * Constructor parameters - At the moment, only parameterless constructors are supported.
  * Overloaded members - Due to limitations of the JavaScript language, overloading is not directly possible in these proxies.  Solutions are being investigated.
  * Generics - generic parameters are not supported at this time, and generated proxies will generally not function correctly when calling generic methods.
  * out/ref parameters - There is currently no support for this feature.  Since JavaScript does not support two-way arguments, there will never be full support without additional data structures.
  * optional arguments - Generated proxies receive a callback as the last argument.  For this reason, default values should be explicitly supplied for optional arguments.

## Disclaimer
This package is in no way affiliated with the edge.js package.

## Dedication
Dedicated to Dorothy Gant (1925-2017)

## License
Copyright (c) 2017 Steve Westbrook

[MIT](LICENSE)
