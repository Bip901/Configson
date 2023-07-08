using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Configson
{
    public static class ConfigurationIO
    {
        public static string BaseDirectory { get; private set; } = ".";

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

        public static JsonSerializerOptions? JsonSerializerOptions { get; set; } = null;

        /// <summary>
        /// Loads the saved object from disk.
        /// </summary>
        /// <exception cref="InvalidDataException"/>
        /// <exception cref="IOException"/>
        public static T Load<T>(string relativePath)
        {
            string path = GetFilePath(relativePath);

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

        public static void Save<T>(T settings, string relativeFilename)
        {
            string path = GetFilePath(relativeFilename);
            using FileStream stream = new(path, FileMode.Create, FileAccess.Write);
            using Utf8JsonWriter jWriter = new(stream, new JsonWriterOptions() { Indented = true });
            JsonSerializer.Serialize(jWriter, settings, JsonSerializerOptions);
        }

        /// <summary>
        /// Returns the path to the given file, creating all parents if necessary.
        /// </summary>
        /// <param name="relativePath">A file path relative to <see cref="BaseDirectory"/>.</param>
        public static string GetFilePath(string relativePath)
        {
            string filePath = Path.Combine(BaseDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            return filePath;
        }
    }
}