using Xunit;
using Moq;
using SmartClipboard.ViewModels;
using SmartClipboard.Utilities;
using SmartClipboard.Models;

namespace SmartClipboard
{
    public class MainViewModelTests
    {
        [Fact]
        public void InsertValidText()
        {
            var mockDB = new Mock<IDatabaseService>();
            mockDB.Setup(x => x.GetAllItems()).Returns(new List<ClipboardItem>());

            var viewModel = new MainViewModel(mockDB.Object);
            string text = "Hello, clipboard!";

            viewModel.SaveClipboardText(text);

            Assert.Single(viewModel.ClipboardItems);
            Assert.Equal(text, viewModel.ClipboardItems[0].Content);
        }
    }
}