using System.Linq;
using FluentAssertions;
using Journals.Model;

namespace Journals.Web.Tests.Framework
{
    public static class AssertionExtensions
    {
        /// <summary>
        /// Asserts whether the specified model is equivalent to the specified ViewModel.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        public static void ShouldBeEquivalentTo(this Journal model, JournalUpdateViewModel viewModel)
        {
            model.Should().NotBeNull();

            model.Id.Should().Be(viewModel.Id);
            model.Title.Should().Be(viewModel.Title);
            model.Description.Should().Be(viewModel.Description);
            model.UserId.Should().Be(viewModel.UserId);
        }

    }
}