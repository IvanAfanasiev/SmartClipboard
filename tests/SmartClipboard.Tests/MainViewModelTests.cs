using Xunit;
using Moq;
using SmartClipboard.ViewModels;
using SmartClipboard.Utilities;
using SmartClipboard.Models;
using SmartClipboard.Services;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SmartClipboard
{
    public class MainViewModelTests
    {
        #region Text insert tests
        [Fact]
        public void InsertValidText()
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object, new SettingsService());
            string text = "Hello, clipboard!";

            viewModel.SaveClipboardText(text);

            Assert.Single(viewModel.ClipboardItems);
            Assert.Equal(text, viewModel.ClipboardItems[0].Content);
        }
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\t")]
        [InlineData(null)]
        public void InvalidText_DoNotInsert(string text)
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object, new SettingsService());

            viewModel.SaveClipboardText(text);

            Assert.Empty(viewModel.ClipboardItems);
        }
        [Fact]
        public void InsertClipboardText_DoNotDuplicate_MoveExistingToEnd()
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object, new SettingsService());
            string text1 = "Hello, clipboard!";
            string text2 = "Bye, clipboard!";

            viewModel.SaveClipboardText(text1);
            viewModel.SaveClipboardText(text2);
            viewModel.SaveClipboardText(text1);

            Assert.Equal(2, viewModel.ClipboardItems.Count);
            Assert.Equal(text2, viewModel.ClipboardItems[0].Content);
            Assert.Equal(text1, viewModel.ClipboardItems[1].Content);
        }
        #endregion

        #region Image insert tests
        [Fact]
        public void InsertValidImage_AddsToClipboard()
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object, new SettingsService());

            var imageSource = CreateTestBitmapSource();

            viewModel.SaveClipboardImage(imageSource);

            Assert.Single(viewModel.ClipboardItems);
            Assert.Equal(ContentType.Image, viewModel.ClipboardItems[0].Type);
            Assert.NotNull(viewModel.ClipboardItems[0].ImagePreview);
        }
        BitmapSource CreateTestBitmapSource()
        {
            var pixel = new byte[] { 0, 0, 0, 255 };
            return BitmapSource.Create(
                1, 1,
                96, 96,
                PixelFormats.Bgra32,
                null,
                pixel,
                4
            );
        }
        #endregion

        #region File insert tests
        [Fact]
        public void InsertValidFileList_AddsToClipboard()
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object, new SettingsService());

            var files = new List<string> { @"C:\temp\file1.txt", "C:\\temp\\file2.txt" };

            viewModel.SaveClipboardFiles(files);

            string[] paths = viewModel.ClipboardItems[0].FilePathList?.
                Split(new[] { ';' }, 
                StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            Assert.Single(viewModel.ClipboardItems);
            Assert.Equal(ContentType.File, viewModel.ClipboardItems[0].Type);
            Assert.Equal(files.Count, paths?.Length);
        }
        #endregion
    }
}