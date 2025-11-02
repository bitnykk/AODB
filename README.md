AODB is a 3rd party tool to import fbx 3D model files into AO's Ctree database

Building it requires to have a C# IDE such as VStudio with adequate environment
Also requires complementary libraries as :
AssimpNet.5.0.0-beta1
CommandLineParser.2.9.1
System.Numerics.Vectors.4.5.0
UkooLabs.FbxSharpie.1.0.99

Once all dependancies are fixed, generate each section of the project :
AODB
Common
Encoding
Importer
(Test is optionnal)

Finally use the command line prompt into the resulting bin folder
(Which could be either Debug or Release, depending on your settings)
