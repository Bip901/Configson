using System;
using System.IO;
using System.Text.Json;

namespace Configson;

/// <summary>
/// A utility class for saving and loading configurations.
/// </summary>
public static class ConfigurationIO
{
    private const string BACKUP_FILE_EXTENSION = ".bak";

    /// <summary>
    /// The directory that relative paths will be relative to. Defaults to the current working directory.
    /// </summary>
    public static string BaseDirectory { get; private set; } = ".";

    /// <summary>
    /// Sets <see cref="BaseDirectory"/>. A common pattern is to call this once in a static constructor so you only need to specify relative paths when saving and loading objects later.
    /// </summary>
    public static void SetBaseDirectory(
        string path,
        RelativePathType pathType = RelativePathType.RelativeToCurrentWorkingDirectory
    )
    {
        if (pathType == RelativePathType.RelativeToAppData)
        {
            path = Path.Join(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create
                ),
                path
            );
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
    /// Custom options for writing output JSONs. Defaults to indented.
    /// </summary>
    public static JsonWriterOptions JsonWriterOptions { get; set; } =
        new JsonWriterOptions() { Indented = true };

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
        return Load<T>(stream);
    }

    /// <summary>
    /// Loads the saved object from the given input stream.
    /// </summary>
    /// <param name="inputStream">The stream to read from.</param>
    /// <exception cref="InvalidDataException"/>
    /// <exception cref="IOException"/>
    public static T Load<T>(Stream inputStream)
    {
        string json;
        using (inputStream)
        {
            using StreamReader reader = new(inputStream);
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
    /// Saves the given object to disk atomically.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="path">The path of the file to write to. If relative, treated as relative to <see cref="BaseDirectory"/>.</param>
    /// <param name="jsonWriterOptions">Custom json writer options. If null, defaults to <see cref="JsonWriterOptions"/>.</param>
    public static void Save<T>(T obj, string path, JsonWriterOptions? jsonWriterOptions = null)
    {
        path = GetFilePath(path);
        string backupFilePath = path + BACKUP_FILE_EXTENSION;
        using (FileStream stream = new(backupFilePath, FileMode.Create, FileAccess.Write))
        {
            Save(obj, stream, jsonWriterOptions);
        }
        File.Move(backupFilePath, path);
    }

    /// <summary>
    /// Saves the given object to the given output stream.
    /// </summary>
    /// <param name="obj">The object to save.</param>
    /// <param name="outputStream">The stream to write to.</param>
    /// <param name="jsonWriterOptions">Custom json writer options. If null, defaults to <see cref="JsonWriterOptions"/>.</param>
    public static void Save<T>(
        T obj,
        Stream outputStream,
        JsonWriterOptions? jsonWriterOptions = null
    )
    {
        using Utf8JsonWriter jWriter = new(outputStream, jsonWriterOptions ?? JsonWriterOptions);
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
