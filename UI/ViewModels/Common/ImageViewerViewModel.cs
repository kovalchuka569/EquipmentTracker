using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Events.Common;
using Core.Services.Common;
using Syncfusion.UI.Xaml.ImageEditor;
using UI.Views.Common;

namespace UI.ViewModels.Common
{
    public class ImageViewerViewModel : BindableBase, INavigationAware
    {
        private ImageViewerTempStorage _tempStorage;
        
        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
        
        public DelegateCommand<ImageSavingEventArgs> ImageSavingCommand { get; set; }
        
        public ImageViewerViewModel(ImageViewerTempStorage tempStorage)
        {
            _tempStorage = tempStorage;
            
            ByteArrayToImage(_tempStorage.ImageData);

            ImageSavingCommand = new DelegateCommand<ImageSavingEventArgs>(OnImageSaving);
        }

        private void OnImageSaving(ImageSavingEventArgs args)
        {
            args.FileName = _tempStorage.ImageName;
        }

        private void ByteArrayToImage(byte[] byteArray)
        {
            Console.WriteLine("ByteArrayToImage");
            try
            {
                ImageSource = null;
                if(byteArray == null || byteArray.Length == 0) ImageSource = null;
            
                var image = new BitmapImage();
                using (var memStream = new MemoryStream(byteArray))
                {
                    memStream.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = memStream;
                    image.EndInit();
                    image.Freeze();
                }
                ImageSource = image;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext) { }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}

