using System;
using System.IO;
using System.Text.Json;

namespace Configson;

/// <summary>
/// A utility class for saving and loading configurations.
/// </summary>
public static class ConfigurationIO
{
    /// <summary>
    /// The directory that relative paths will be relative to. Defaults to the current working directory.
    /// </summary>
    public static string BaseDirectory { get; private set; } = ".";

    /// <summary>
    /// Sets <see cref="BaseDirectory"/>. A common pattern is to call this once in a static constructor so you only need to specify relative paths when saving and loading objects later.
    /// </summary>
    public static void SetBaseDirectory(string path, RelativePathType pathType = RelativePathType.RelativeToCurrentWorkingDirectory)
    {
        if (pathType == RelativePathType.RelativeToAppData)
        {
            path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), path);
        }
        else if (pathType == RelativePathType.RelativeToAppBaseDirectory)
        {
            path = Path.Join(AppContext.BaseDirectory, path);
        }
        BaseDirectory = path;
    }

    /// <summary>
    /// Custom options for serializing/deserializing objects. Defaults to null.
    /// </summary>
    public static JsonSerializerOptions? JsonSerializerOptions { get; set; } = null;

    /// <summary>
    /// Loads the saved object from disk.
    /// </summary>
    /// <param name="path">The path of the file to read from. If relative, treated as relative to <see cref="BaseDirectory"/>.</param>
    /// <exception cref="InvalidDataException"/>
    /// <exception cref="IOException"/>
    public static T Load<T>(string path)
    {
        path = GetFilePath(path);

        FileStream stream;
        try
        {
            stream = new(path, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex) when (ex is not IOException)
        {
            throw new IOException(ex.Message, ex);
        }
        string json;
        using (stream)
        {
            using StreamReader reader = new(stream);
            json = reader.ReadToEnd();
        }
        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(ex.Message, ex);
        }
        if (result is null)
        {
            throw new InvalidDataException("Root object must not be null.");
        }
        return result;
    }

    /// <summary>
    /// Saves the given object to disk.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="path">The path of the file to write to. If relative, treated as relative to <see cref="BaseDirectory"/>.</param>
    public static void Save<T>(T obj, string path)
    {
        path = GetFilePath(path);
        using FileStream stream = new(path, FileMode.Create, FileAccess.Write);
        using Utf8JsonWriter jWriter = new(stream, new JsonWriterOptions() { Indented = true });
        JsonSerializer.Serialize(jWriter, obj, JsonSerializerOptions);
    }

    /// <summary>
    /// Returns the path to the given file, creating all parents if necessary.
    /// </summary>
    /// <param name="relativePath">A file path relative to <see cref="BaseDirectory"/>. If absolute, returned as-is.</param>
    public static string GetFilePath(string relativePath)
    {
        string filePath = Path.Combine(BaseDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        return filePath;
    }
}