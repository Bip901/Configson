# Configson

A tiny package for easy saving and loading of CSharp classes as JSON configuration files.

[Download from NuGet](https://www.nuget.org/packages/Configson/)

## Usage

```csharp
//Define a data class
record Config(string Color, int Size);

// Example instance
config = Config("Red", 3);

// Save
ConfigurationIO.Save(config, "config.json");

// Load
config = ConfigurationIO.Load<Config>("config.json");
```
