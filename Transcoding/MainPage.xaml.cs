using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Transcoding.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Transcoding
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FolderPicker folderPicker = new FolderPicker();
        private FileOpenPicker fileOpenPicker = new FileOpenPicker();
        private StorageFolder folder;
        private ObservableCollection<MediaFile> mediaFiles = new ObservableCollection<MediaFile>();
        private int currentItemNum = 0;


        public MainPage()
        {
            this.InitializeComponent();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.FileTypeFilter.Add("*");
            fileOpenPicker.FileTypeFilter.Add(".mp4");


        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                this.txt_filePath.Text = folder.Path;
            }
            else
            {
                this.txt_filePath.Text = "Operation cancelled.";
            }
        }

        private async void add_Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<StorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (StorageFile file in files)
                {
                    MediaFile mediafile = new MediaFile();
                    mediafile.Name = file.Name;
                    mediafile.Path = file.Path;
                    mediafile.Progress = 0;
                    mediaFiles.Add(mediafile);
                }

            }
        }


        void TranscodeProgress(IAsyncActionWithProgress<double> asyncInfo, double percent)
        {
            var file = mediaFiles[currentItemNum - 1];
            file.Progress = percent;
        }

        void TranscodeComplete(IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status)
        {
            asyncInfo.GetResults();
            if (asyncInfo.Status == AsyncStatus.Completed)
            {
                // Display or handle complete info.
            }
            else if (asyncInfo.Status == AsyncStatus.Canceled)
            {
                // Display or handle cancel info.
            }
            else
            {
                // Display or handle error info.
            }
        }


        private async void Start_clicked(object sender, RoutedEventArgs e)
        {
            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Uhd4320p);
            MediaTranscoder transcoder = new MediaTranscoder();


            foreach (var file in mediaFiles)
            {
                currentItemNum++;
                StorageFile source = await StorageFile.GetFileFromPathAsync(file.Path);
                StorageFile targetFile = await folder.CreateFileAsync("Done_" + DateTime.Now.ToShortDateString() + "_" + file.Name);
                PrepareTranscodeResult prepareOp = await
                    transcoder.PrepareFileTranscodeAsync(source, targetFile, profile);

                if (prepareOp.CanTranscode)
                {
                    var transcodeOp = prepareOp.TranscodeAsync();

                    transcodeOp.Progress +=
                        new AsyncActionProgressHandler<double>(TranscodeProgress);
                    transcodeOp.Completed +=
                        new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);
                }
                else
                {
                    switch (prepareOp.FailureReason)
                    {
                        case TranscodeFailureReason.CodecNotFound:
                            System.Diagnostics.Debug.WriteLine("Codec not found.");
                            break;
                        case TranscodeFailureReason.InvalidProfile:
                            System.Diagnostics.Debug.WriteLine("Invalid profile.");
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Unknown failure.");
                            break;
                    }
                }
            }
        }
    }
}
