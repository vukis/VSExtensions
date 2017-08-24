# VSExtensions

## Demangled build output 

Sometimes I have troubles trying to de-code Visual Studio linker errors with weird symbols. There is a nice online tool https://demangler.com/ that works for GCC and MSVC symbols. The basic idea is that it transforms the C++ linker symbols like
?foo@MyClass@@IAEXV?$vector@V?$basic_string@DU?$char_traits@D@std@@V?$allocator@D@2@@std@@V?$allocator@V?$basic_string@DU?$char_traits@D@std@@V?$allocator@D@2@@std@@@2@@std@@@Z to a relatively readable C++ code. I think it is nice to have this tool integratated  into Visual Studio as extension.

Here you can see mangled symbols.

![Build output](https://github.com/vukis/VSExtensions/blob/master/BuildDemangledOutput/BuildOutput.jpg)

Enabling demangled build output.

![Build output](https://github.com/vukis/VSExtensions/blob/master/BuildDemangledOutput/EnablingPlugin.jpg)

After demangled build output enabled symbols are readable.

![Demangled build output](https://github.com/vukis/VSExtensions/blob/master/BuildDemangledOutput/DemangledBuildOutput.jpg)
