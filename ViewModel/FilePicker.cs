using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using URManager.View;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace URManager.Backend.ViewModel
{
    public class FilePicker
    {
        /// <summary>
        /// Open File Dialog, zum öffnen von Dateien. Gibt den Pfad zurück.
        /// </summary>
        /// <returns>Task</returns>
        public static async Task<string> OpenAsync(string title, string filter)
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            filePicker.FileTypeFilter.Add(filter);
            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file != null)
            {
                return file.Path;
            }
            return null;
        }

        /// <summary>
        /// Save File Dialog, zum speichern von Dateien. Gibt den Pfad zurück.
        /// </summary>
        /// <returns>Task</returns>
        public static async Task<string> SaveAsync(string title, string filter, string name = "")
        {
            var filePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            filePicker.FileTypeChoices.Add(title, new List<string>() { filter });
            filePicker.SuggestedFileName = name;
            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            var file = await filePicker.PickSaveFileAsync();
            if (file != null)
            {
                return file.Path;
            }
            return null;
        }

        /// <summary>
        /// Open File Dialog, zum öffnen von Dateien. Gibt den Pfad zurück.
        /// </summary>
        /// <returns>Task</returns>
        public static async Task<string> OpenFolderAsync()
        {
            var folderPicker = new FolderPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");
            //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                return folder.Path;
            }
            return null;
        }
    }
}
